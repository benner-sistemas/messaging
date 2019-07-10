[<img align="right" src="https://emojipedia-us.s3.dualstack.us-west-1.amazonaws.com/thumbs/160/google/146/flag-for-brazil_1f1e7-1f1f7.png" width="35x"/>](https://github.com/benner-sistemas/messaging/blob/master/LEIAME.md)

# Benner.Messaging

Benner.Messaging is a .NET Standard lightweight messaging library to deal with any message broker with ease.
Benner.Messaging supports RabbitMQ, ActiveMQ, Amazon SQS and Azure Queue. It is _free and open-source_ under MIT License.

## Build Status
Branch | Status
--- | :---:
master | [![Build Status](https://dev.azure.com/benner-tecnologia/benner-tecnologia/_apis/build/status/benner-sistemas.messaging?branchName=master)](https://dev.azure.com/benner-tecnologia/benner-tecnologia/_build/latest?definitionId=2&branchName=master)


## Nuget Package
| Package Name | .NET Framework | .NET Standard |
| ------------ | :------------: | :-----------: |
| [Benner.Messaging](https://www.nuget.org/packages/Benner.Messaging/) | 4.6.1 | 2.0 |

## One Messaging API to rule them all!

We provide an extremely simple and intuitive API for sending and receiving messages to either RabbitMQ, ActiveMQ, Amazon SQS or Azure Queue:

```csharp
Messaging.Enqueue("queue-name", "hello world!");
var message = Messaging.Dequeue("queue-name");
```

Yep, you got that right! You code just once, on an extremly simple way, and run over any message broker. No vendor lock-in, no code refactoring.
All the magic relies on configuration (file or injected code).

## Behavior matters

It's important to notice that Messaging API was born to work on a specific way:
* First in, first out 
* One producer sends
* Only one consumer receives
* If consumer fails at receiving or processing, the message returns to queue (***Work In Progress***)
* The message will not be lost (***Work In Progress***)

Enqueue and Dequeue operations were designed to ensure that a sent message by the _sender_ successfully arrives on the _receiver_. 
That means that we pursuit _Publisher Confirms_ and _Consumer Acknoledgement_ approach across any supported broker. (***Work In Progress***)

## Get Started

### Sending message

Create a new project:
```shell
dotnet new console -n producer
cd producer
```

Add Benner.Messaging and open the project on vscode:
```shell
dotnet add package benner.messaging
code .
```

Add _using_ and just send a message to some queue:
```csharp
using Benner.Messaging;
Messaging.Enqueue("queue-name", "hello world!");
```

That's it! `dotnet run` it and you will get:
```shell
Messaging config not found
```

### Brokers configuration

Well, you need a `messaging.config` file, like that (don't worry, we will get deeper on configuration ahead):
```xml
<MessagingConfigSection>
   <!-- Benner.Messaging is multi broker, chose the default -->
   <brokerList default="RabbitMQ">
      <broker name="AzureMQ" type="Benner.Messaging.AzureMQConfig, Benner.Messaging">
         <add key="ConnectionString" value="DefaultEndpointsProtocol=https;AccountName=accountName;AccountKey=accountKey;EndpointSuffix=core.windows.net" />
         <add key="InvisibilityTime" value="15" />
      </broker>
      <broker name="RabbitMQ" type="Benner.Messaging.RabbitMQConfig, Benner.Messaging">
         <add key="Hostname" value="servername" />
         <add key="Username" value="username" />
         <add key="Password" value="password" />
         <add key="Port" value="port" />
      </broker>
      <broker name="AmazonSQS" type="Benner.Messaging.AmazonSQSConfig, Benner.Messaging">
         <add key="InvisibilityTime" value="15" />
      </broker>
      <broker name="ActiveMQ" type="Benner.Messaging.ActiveMQConfig, Benner.Messaging">
         <add key="Hostname" value="servername" />
         <add key="Username" value="username" />
         <add key="Password" value="password" />
         <add key="Port" value="port" />
      </broker>
   </brokerList>
   <!-- specify only queues that should use other than default broker -->
   <queues>
      <queue name="queue-name" broker="broker-name" />
   </queues>
</MessagingConfigSection>
```

You can also inject config through code:
```csharp
var config = new MessagingConfigBuilder(
	"RabbitMQ", 
	BrokerType.RabbitMQ, 
	new Dictionary<string, string> 
	{ 
		{ "Hostname", "servername" },
	})
	.Create();

Messaging.Enqueue("queue-name", "hello world!", config);
```

That's it! `dotnet run` it and you will get:
```shell
Unable to connect to RabbitMQ server
```

### Provisioning a broker

Well... you need, in this case, a RabbitMQ runnig according to your configuration.

Thankfully humanity evolved and we have Docker containers to help us out. So, let's just run it!
```shell
docker run -d -v path/to/rabbit/volume/folder:/var/lib/rabbitmq -p 15672:15672 -p 15671:15671 -p 5672:5672 -p 5671:5671 rabbitmq:3.7-management
```

Now we are good to go! `dotnet run` it. No errors? Great! Now what?

### Check broker management console

On your browser, access `http://hostname::15672` (user guest, password guest) to manage Rabbit and you'll be able to see your queue with your messages inside it.

### Receive message
Create a new project:
```shell
dotnet new console -n consumer
cd consumer
```

Add Benner.Messaging and open the project on vscode:
```shell
dotnet add package benner.messaging
code .
```

Add _using_ and just receive a message from the queue, not forgeting configuration:
```csharp
using Benner.Messaging;

var config = new MessagingConfigBuilder(
	"RabbitMQ", 
	BrokerType.RabbitMQ, 
	new Dictionary<string, string> 
	{ 
		{ "Hostname", "servername" },
	})
	.Create();

var message = Messaging.Dequeue("queue-name", config);
Console.Write(message);
```

That's it! `dotnet run` it and you receive:
```shell
hello world! 
```

### Now let's do it insanelly

Change consumer code to:
```csharp
using Benner.Messaging;

var config = new MessagingConfigBuilder(
	"RabbitMQ", 
	BrokerType.RabbitMQ, 
	new Dictionary<string, string> 
	{ 
		{ "Hostname", "servername" },
	})
	.Create();

// Create new instance of messaging
using (var client = new Messaging(config))
{
    // Listen to "my-queue"
    client.StartListening("my-queue", (args) =>
    {
        string message = args.AsString;
        // Print the message
        Console.WriteLine(message);
        return true;
    });
    // Stand-by the application so it can keep listening
    Console.ReadKey();
}
```

Ok, `dotnet run` it 3 times so you will have 3 consumers simultaneously

Change producer code to:
```csharp
using Benner.Messaging;

var config = new MessagingConfigBuilder(
	"RabbitMQ", 
	BrokerType.RabbitMQ, 
	new Dictionary<string, string> 
	{ 
		{ "Hostname", "servername" },
	})
	.Create();

// Create new instance of messaging
using (var client = new Messaging(config))
{
    // Sending 1000 messages
    for (int i = 1; i <= 1000; i++)
        client.EnqueueMessage("my-queue", "hello world #" + i);
}
```

Done, `dotnet run` it and see what happens.
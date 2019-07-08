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

`Messaging.Enqueue("queue-name", "hello world!");`

`var message = Messaging.Dequeue("queue-name");`

Yep, you got that right! You code just once, on an extremly simple way, and run over any message broker. No vendor lock-in, no code refactoring.
All the magic relies on configuration (file or injected code).

## Behavior matters

It's important to notice that Messaging API was born to work on a specific way:
* First in, first out 
* One producer sends
* Only one consumer receives
* If consumer fails at receiving or processing, the message returns to queue (***MENTIRA***)
* The message will not be lost (***MENTIRA***)

Enqueue and Dequeue operations were designed to ensure that a sent message by the _sender_ successfully arrives on the _receiver_. 
That means that we pursuit _Publisher Confirms_ and _Consumer Acknoledgement_ approach across any supported broker. (***MENTIRA***)

## Get Started

### Sending message

Create a new project:
```
dotnet new console -n producer
cd producer
```

Add Benner.Messaging and open the project on vscode:
```
dotnet add package benner.messaging
code .
```

Add _using_ and just send a message to some queue:
```
using Benner.Messaging;
Messaging.Enqueue("queue-name", "hello world!");
```

That's it! `dotnet run` it and you will get:
```
Messaging config not found
```

### Brokers configuration

Well, you need a `messaging.config` file, like that (don't worry, we will get deeper on configuration ahead):
```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
	<configSections>
		<section name="MessagingConfigSection" type="Benner.Tecnologia.Messaging.MessagingFileConfigSection, Benner.Tecnologia.Messaging" />
	</configSections>
	<MessagingConfigSection>
		<queues>
			<queue name="queue-name" broker="broker-name" />
		</queues>
		<brokerList default="RabbitMQ">
			<broker name="AzureMQ" type="Benner.Tecnologia.Messaging.AzureMQConfig, Benner.Tecnologia.Messaging">
				<add key="ConnectionString" value="DefaultEndpointsProtocol=https;AccountName=accountName;AccountKey=accountKey;EndpointSuffix=core.windows.net" />
				<add key="InvisibilityTime" value="15" />
			</broker>
			<broker name="RabbitMQ" type="Benner.Tecnologia.Messaging.RabbitMQConfig, Benner.Tecnologia.Messaging">
				<add key="Username" value="username" />
				<add key="Password" value="password" />
				<add key="Hostname" value="servername" />
				<add key="Port" value="port" />
			</broker>
			<broker name="Amazon" type="Benner.Tecnologia.Messaging.AmazonSqsConfig, Benner.Tecnologia.Messaging">
				<add key="InvisibilityTime" value="15" />
			</broker>
			<broker name="ActiveMQ" type="Benner.Tecnologia.Messaging.ActiveMQConfig, Benner.Tecnologia.Messaging">
				<add key="Hostname" value="servername" />
				<add key="Password" value="password" />
				<add key="Hostname" value="servername" />
				<add key="Port" value="port" />
			</broker>
		</brokerList>
	</MessagingConfigSection>
</configuration>
```

You can also inject config through code:
```
var config = MessagingConfigFactory
	.NewMessagingConfigFactory()
	.WithRabbitMQBroker("hostname", 5672, "user", "password")
	.Create();

Messaging.Enqueue("queue-name", "hello world!", config);
```

That's it! `dotnet run` it and you will get:
```
Unable to connect to RabbitMQ server
```

### Provisioning a broker

Well... you need, in this case, a RabbitMQ runnig according to your configuration.

Thankfully humanity evolved and we have Docker containers to help us out. So, let's just run it!
```
docker run -d -v path/to/rabbit/volume/folder:/var/lib/rabbitmq --hostname rabbit-management --name rabbit-management -p 15672:15672 -p 15671:15671 -p 5672:5672 -p 5671:5671 rabbitmq:3.7-management
```

TODO:(more details) 
Now we are good to go! `dotnet run` it. No error.. good.. so what?!

### Check broker management console

On your browser, access `http://hostname::15672` to manage rabbit and you'll be able to see your queue with your messages inside it.


### Receive message
Create a new project:
```
dotnet new console -n consumer
cd consumer
```

Add Benner.Messaging and open the project on vscode:
```
dotnet add package benner.messaging
code .
```

Add _using_ and just receive a message from the queue, not forgeting configuration:
```
using Benner.Messaging;

var config = MessagingConfigFactory
	.NewMessagingConfigFactory()
	.WithRabbitMQBroker("hostname", 5672, "user", "password")
	.Create();

var message = Messaging.Dequeue("queue-name", config);

Console.Write(message);

```

That's it! `dotnet run` it and you receive:
```
hello world! 
```

### Now let's do it insanelly

Change consumer code to:
```
TODO
```

`dotnet run` it 3 times so you will have 3 consumers simultaneously

Change producer code to:
```
TODO
```

`dotnet run` it and see what happen

### The "type" attribute
O atributo "type" do broker deve ser um nome de assembly completo, por exemplo *Benner.Messaging.ActiveMQConfig, Benner.Messaging*.
The broker's "type" attribute must be an assembly fullname, e.g. *Benner.Messaging.ActiveMQConfig, Benner.Messaging*.

### Queue names rules and restrictions
There are a few rules for the creation and usage of queues when it comes to their names:

 1. A queue name must start with a letter or number, and can only contain letters, numbers, and the dash (-) character.
 2. The first and last letters in the queue name must be alphanumeric. The dash (-) character cannot be the first or last character. Consecutive dash characters are not permitted in the queue name.
 3. All letters in a queue name must be lowercase.
 4. A queue name must be from 3 through 63 characters long.

These rules are validated by both memory and file configurations.

### File structure

The configuration file for the API, according to 'messaging.config.model', presents the following structure:

	<?xml version="1.0" encoding="utf-8" ?>
	<configuration>
		<configSections>
			<section name="MessagingConfigSection" type="Benner.Messaging.MessagingFileConfigSection, Benner.Messaging" />
		</configSections>
		<MessagingConfigSection>
			<queues>
				<queue name="nome_da_fila" broker="nome_do_broker" />
			</queues>
			<brokerList default="Name_do_broker_default">
				<broker name="nome_do_broker" type="Benner.Messaging.Classname, Benner.Messaging">
					<add key="Propriedade" value="Valor" />
				</broker>
			</brokerList>
		</MessagingConfigSection>
	</configuration>

To use this type of configuration, use the *Benner.Messaging.FileMessagingConfig* class.

|Constructor|Description|
|--|--|
|FileMessagingConfig()|It uses the *messaging.config* file found in the same directory of the executing assembly.|
|FileMessagingConfig(string)|It uses the passed *.config* file. The string must be the file's full path.|

This class does many validations about the file structure, thereby, if any structure flaw is found, an exception is thrown.

### Tags
 - **MessagingConfigSection**: Responsible for all queues and brokers configurations.
	 - **queues**: Contains all pre-configured queues.
		 - **queue**: A pre-configured queue. Contains the queue name that will be created or used, and the broker configuration name. The *name* attribute from the *broker* tag corresponds to the *broker* tag of this *queue* tag.
	 - **brokerList**: Contains all broker configurations. The *default* attribute defines the broker configuration that shall be used when one not pre-configured queue is informed. Corresponds to the *name* attribute from *broker* tag.
		 - **broker**: The broker configuration itself. The *name* attribute exists because it is possible to configure the same broker service in different ways, by using different names. The *type* attribute contains the configuration class to be used, i.e. the service. Only *classname* must be changed.
			 - **add**: Tag for filling the broker properties. Each service will have their own properties and peculiarities.

### In-memory configuration

Besides the file configuration, it is also possible to configure the API through the *Benner.Messaging.MessagingConfig* class, obtained by *Benner.Messaging.MessagingConfigFactory* class.
As an example, let's make a configuration to use 2 brokers, ActiveMQ and RabbitMQ, setting RabbitMQ as default: (***MENTIRA***)

	// RabbitMQ's configuration properties
	var configRabbit = new Dictionary<string, string>() { { "Hostname", "server-name" } };

	// ActiveMQ's configuration properties
	var configActive = new Dictionary<string, string>() { { "Hostname", "server-name" } };

	var config = MessagingConfigFactory.NewMessagingConfigFactory()
		// Adding activemq broker and its configuration
		.WithActiveMQBroker(configActive)
		// Adding rabbitmq broker and its configuration
		.WithRabbitMQBroker(configRabbit)
		// Adding some pre-configured queues
		.WithMappedQueue("fila-notas", "rabbit")
		.WithMappedQueue("fila-lancamentos", "rabbit")
		.WithMappedQueue("fila-financeiro", "active")
		// Creating the MessagingConfig instance
		.Create();

### ActiveMQConfig settings

|Name|Description|
|--|--|
|Hostname|The hostname, e.g. the server which the container is running in.|
|Username|The service log-in username. Default: admin.|
|Password|The service log-in password. Default: admin.|
|Port|The port number to the service host. Default: 61616.|

### AmazonSQSConfig settings
Amazon's service has one particularity about conecting to their servers. For more information: [Configuring AWS Credentials](https://docs.aws.amazon.com/sdk-for-net/v2/developer-guide/net-dg-config-creds.html)

|Name|Description|
|--|--|
|InvisibilityTime|The time, in minutes, in which the received message will stay invisible to other consumers until it is successfully consumed and deleted. In case of a processing taking longer to finish than the invisibility time, the message stays visible again in the queue, available to other consumers.|

### AzureMQConfig settings
|Name|Description|
|--|--|
|ConnectionString|The connection string provided by Azure Queue service to stablish connection|
|InvisibilityTime|The time, in minutes, in which the received message will stay invisible to other consumers until it is successfully consumed and deleted. In case of a processing taking longer to finish than the invisibility time, the message stays visible again in the queue, available to other consumers.|

### RabbitMQConfig settings
|Name|Description|
|--|--|
|Hostname|The hostname, e.g. the server which the container is running in.|
|Username|The service log-in username. Default: guest.|
|Password|The service log-in password. Default: guest.|
|Port|The port number to the service host. Default: 5672.|

## The *Messaging* class
The API's main class is *Benner.Messaging.Messaging*. It has the delivery and receipt methods and can be used as static or instance.

### As instance

|Constructor|Description|
|--|--|
|Messaging()|Instantiates a *Messaging* object using file configuration (*FileMessagingConfig* default constructor).|
|Messaging(IMessagingConfig)|Receives the configuration that will be used (in-memory or file).|

|Method|Description|
|--|--|
|EnqueueMessage(string, string)|Enqueues one *string* message in the passed queue.|
|EnqueueMessage(string, object)|Enqueues one *object* message (serialized) in the passed queue.|
|StartListening(string, Func<MessagingArgs, bool>)|Listens to the passed queue, receiving the messages through a method (anonymous or not) by a *MessagingArgs* object from parameter.|

When the ***StartListening*** method is used, there are 3 possible situations for the messages receipt.
The method passed to the ***StartListening***'s *Func* parameter receives a *MessagingArgs* parameter, that has the message as *string*, as *byte[]* and also a method to deserialize the message to an object.

The receiver method (Func parameter) may return *true*, *false* or throw an exception:

 - **True**: the message is considered successfully consumed, therefore it is deleted from queue.
 - **False**: the message is considered received, but something on processing went wrong. The message returns to the queue and become available again to other consumers.
 - **Exceção**: the message is considered dangerous, hence being withdrawn from the queue and enqueued in another error queue, whose name is the consumed queue name followed by '-error', e.g. if the consumed queue name is 'test-queue', the error queue will be called 'test-queue-error'.

This method must be used cautiously, once it is a listener, it is recommended to let it in stand-by, with some kind of waiting after it, e.g.:
			
	// Instantiating a new default configuration Messaging
	var messaging = new Messaging();

	// Listening to the queue
	messaging.StartListening("queue", (args) =>
	{
		if (!args.AsString.Contains("zyx"))
			throw new Exception("Invalid format.");

		if (args.AsString.Contains("abxz"))
		{
			Console.WriteLine(args.AsString);
			return true;
		}
		return false;
	});

	// Letting the application in stand-by mode, allowing the message consuming
	Console.ReadKey();

	// Freeing resources. Strongly recommended
	messaging.Dispose();

### As static

|Method|Description|
|--|--|
|Dequeue(string)|Receives the next message from queue, as *string*.|
|Dequeue(string, IMessagingConfig)|Receives the next message from queue, as *string*.|
|Dequeue<T>(string)|Receives the next message from queue and deserializes it to a type ***T*** object. In case of a deserialization error the message is lost.|
|Dequeue<T>(string, IMessagingConfig)|Receives the next message from queue and deserializes it to a type ***T*** object. In case of a deserialization error the message is lost.|
|Enqueue(string, object)|Enqueues one *string* message in the passed queue.|
|Enqueue(string, object, IMessagingConfig)|Enqueues one *string* message in the passed queue.|
|Enqueue(string, string)|Enqueues one *object* message (serialized) in the passed queue.|
|Enqueue(string, string, IMessagingConfig)|Enqueues one *object* message (serialized) in the passed queue.|

Some of these methods don't have a configuration parameter, because they use the default configuration (default *FileMessagingConfig*'s constructor). 
That being said, it is recommended to use the overloads with configuration parameter.
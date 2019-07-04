# Benner.Messaging

Benner.Messaging is a .NET Standard lightweight messaging library for deal with any message broker, easily.
Benner.Messaging support RabbitMQ, ActiveMQ, AmazonSQS and AzureMQ. It is _free and open-source_ under MIT License.

## Build Status
Branch | Status
--- | :---:
master | [![Build Status](https://dev.azure.com/benner-tecnologia/benner-tecnologia/_apis/build/status/benner-sistemas.messaging?branchName=master)](https://dev.azure.com/benner-tecnologia/benner-tecnologia/_build/latest?definitionId=2&branchName=master)


## Nuget Package
| Package Name | .NET Framework | .NET Standard |
| ------------ | :------------: | :-----------: |
| [Benner.Messaging](https://www.nuget.org/packages/Benner.Messaging/) | 4.6.1 | 2.0 |

## One Messaging API to rule them all!

We provide an extremely simple and intuitive API for sending and receiving messages to either RabbitMQ, ActiveMQ, AmazonSQS or AzureQueue:

`Messaging.Enqueue("queue-name", "hello world!");`

`var message = Messaging.Dequeue("queue-name");`

Yep, you got that right! You code just once, on a extremly simple way, and run over any message broker. No vendor lock-in, no code refactoring.
All the magic relies on configuration (file or injected code).

## Behavior matters

It's important to notice that Messaging API was born to work on an specific way:
* First in, first out 
* One producer send
* Only one consumer receive
* If consumer fails receiving or processing, message returns to queue
* Message will not be lost

Enqueue and Dequeue operations was designed to ensure that an sent message by the _sender_ sucessfuly arrives on the _receiver_. That means that we pursuit _Publisher Confirms_ and  _Consumer Acknoledgement_ approach across any supported broker.

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
TODO: Léo, colocar aqui o conteúdo do .config.modelo completo
```

Or..! You can inject config throught code:
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

Thankfully humanity evolved, we have containers Docker to help us out. So, let's just run it!
```
TODO: review all params to run RabbitMQ on container
docker run -d -v rabbitmq_data:/var/lib/rabbitmq --hostname wes-management --name wes-management -p 15672:15672 -p 15671:15671 -p 5672:5672 -p 5671:5671 rabbitmq:3.7-management
```

TODO:(more details) Now we are good to go! `dotnet run` it. No error.. good.. so what?!

### Check broker management console

and access Rabbit management console at `TODO: http?://hostname:????` and You will see your queue and your message.


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

Add _using_ and just receive a message from the queue, dont forget configuration:
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


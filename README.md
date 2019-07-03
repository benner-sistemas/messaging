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

## Get Started
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

Add using and just send a message to some queue:
```
using Benner.Messaging;

Messaging.Enqueue("queue-name", "hello world!");
```

That's it! `dotnet run` it and you will get:
```
Messaging config not found
```

Well, you need a `messaging.config` file in your bin (TODO: details here).

Or..! You can inject config throught code:
```
var config = new MessagingConfig()
   .AddRabbitMQBroker("hostname", 5672, "user", "password");
Messaging.Enqueue("queue-name", "hello world!", config);
```

That's it! `dotnet run` it and you will get:
```
TODO: Colocar aqui o erro que acontece caso nao tenha um Rabbit no endereço
```

Well... you need, in this case, a RabbitMQ runnig according to your configuration.

You can run it easily with Docker:
```
TODO: review all params to run rabbit on container
docker run -d -v rabbitmq_data:/var/lib/rabbitmq --hostname wes-management --name wes-management -p 15672:15672 -p 15671:15671 -p 5672:5672 -p 5671:5671 rabbitmq:3.7-management
```

Now we are good to go! `dotnet run` it, and access Rabbit management console at `TODO: http?://hostname:????`


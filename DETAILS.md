[<img align="right" src="https://emojipedia-us.s3.dualstack.us-west-1.amazonaws.com/thumbs/160/google/146/flag-for-brazil_1f1e7-1f1f7.png" width="35x"/>](https://github.com/benner-sistemas/messaging/blob/master/DETALHES.md)

# API configuration

### Available brokers

Up to this moment the API enables the use of 4 different services, with the following *classnames* to be filled in the configuration.

|Service|Classname|
|--|--|
|**ActiveMQ**|ActiveMQConfig|
|**Amazon SQS**|AmazonSQSConfig|
|**Azure Queue**|AzureMQConfig|
|**RabbitMQ**|RabbitMQConfig|

The broker's "type" attribute must be an assembly fullname, e.g. *Benner.Messaging.ActiveMQConfig, Benner.Messaging*.

### Queue names rules and restrictions

There are a few rules for the creation and usage of queues when it comes to their names:

 1. A queue name must start with a letter or number, and can only contain letters, numbers, and the dash (-) character.
 2. The first and last letters in the queue name must be alphanumeric. The dash (-) character cannot be the first or last character. Consecutive dash characters are not permitted in the queue name.
 3. All letters in a queue name must be lowercase.
 4. A queue name must be from 3 through 63 characters long.

These rules are validated by both memory and file configurations.

## File configuration

The configuration file for the API, according to 'messaging.config.model', presents the following structure:

```xml
<MessagingConfigSection>
   <brokerList default="default-broker-name">
      <broker name="broker-name" type="Benner.Messaging.Classname, Benner.Messaging">
         <add key="Property" value="value" />
      </broker>
   </brokerList>
   <queues>
      <queue name="queue-name" broker="broker-name" />
   </queues>
</MessagingConfigSection>
```

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

## In-memory configuration

Besides the file configuration, it is also possible to configure the API through the *Benner.Messaging.MessagingConfig* class, obtained by *Benner.Messaging.MessagingConfigBuilder* class.
As an example, let's make a configuration to use 2 brokers, ActiveMQ and RabbitMQ, setting RabbitMQ as default:

```csharp
// RabbitMQ's configuration properties
var configRabbit = new Dictionary<string, string>() { { "Hostname", "server-name" } };

// Instanciando o builder com broker default
var config = new MessagingConfigBuilder("rabbit", BrokerType.RabbitMQ, rabbitConfig)
	// Adding activemq broker and its configuration
	.WithActiveMQBroker("active", "servername")
	// Adding some pre-configured queues
	.WithMappedQueue("queue-benefits", "rabbit")
	.WithMappedQueue("queue-accounting", "rabbit")
	.WithMappedQueue("queue-humanresources", "active")
	// Creating the MessagingConfig instance
	.Create();
```

## Services' settings

### ActiveMQConfig 

|Name|Description|
|--|--|
|Hostname|The hostname, e.g. the server which the container is running in.|
|Username|The service log-in username. Default: admin.|
|Password|The service log-in password. Default: admin.|
|Port|The port number to the service host. Default: 61616.|

### AmazonSQSConfig 

Amazon's service has one particularity about conecting to their servers. For more information: [Configuring AWS Credentials](https://docs.aws.amazon.com/sdk-for-net/v2/developer-guide/net-dg-config-creds.html)

|Name|Description|
|--|--|
|InvisibilityTime|The time, in minutes, in which the received message will stay invisible to other consumers until it is successfully consumed and deleted. In case of a processing taking longer to finish than the invisibility time, the message stays visible again in the queue, available to other consumers.|

### AzureMQConfig 

|Name|Description|
|--|--|
|ConnectionString|The connection string provided by Azure Queue service to stablish connection|
|InvisibilityTime|The time, in minutes, in which the received message will stay invisible to other consumers until it is successfully consumed and deleted. In case of a processing taking longer to finish than the invisibility time, the message stays visible again in the queue, available to other consumers.|

### RabbitMQConfig 

|Name|Description|
|--|--|
|Hostname|The hostname, e.g. the server which the container is running in.|
|Username|The service log-in username. Default: guest.|
|Password|The service log-in password. Default: guest.|
|Port|The port number to the service host. Default: 5672.|

# The *Messaging* class

The API's main class is *Benner.Messaging.Messaging*. It has the delivery and receipt methods and can be used as static or instance.

## As instance

|Constructor|Description|
|--|--|
|Messaging()|Instantiates a *Messaging* object using file configuration (*FileMessagingConfig* default constructor).|
|Messaging(IMessagingConfig)|Receives the configuration that will be used (in-memory or file).|

|Method|Description|
|--|--|
|EnqueueMessage(string, string)|Enqueues one *string* message in the passed queue.|
|EnqueueMessage(string, object)|Enqueues one *object* message (serialized) in the passed queue.|
|StartListening(string, Func<MessagingArgs, bool>)|Listens to the passed queue, receiving the messages through a method (anonymous or not) by a *MessagingArgs* object from parameter.|

When the ***StartListening*** method is used, there are 3 possible situations for the messages receipt. The method passed to the ***StartListening***'s *Func* parameter receives a *MessagingArgs* parameter, that has the message as *string*, as *byte[]* and also a method to deserialize the message to an object.
The receiver method (Func parameter) may return *true*, *false* or throw an exception:

 - **True**: the message is considered successfully consumed, therefore it is deleted from queue.
 - **False**: the message is considered received, but something on processing went wrong. The message returns to the queue and become available again to other consumers.
 - **Exceção**: the message is considered dangerous, hence being withdrawn from the queue and enqueued in another error queue, whose name is the consumed queue name followed by '-error', e.g. if the consumed queue name is 'test-queue', the error queue will be called 'test-queue-error'.

This method must be used cautiously, once it is a listener, it is recommended to let it in stand-by, with some kind of waiting after it, e.g.:
			
```csharp
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
```

## As static

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
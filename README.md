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
* If consumer fails at receiving or processing, the message returns to queue
* The message will not be lost

Enqueue and Dequeue operations were designed to ensure that a sent message by the _sender_ successfully arrives on the _receiver_. That means that we pursuit _Publisher Confirms_ and  _Consumer Acknoledgement_ approach across any supported broker.

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
docker run -d -v <path/to/rabbit/volume/folder:/var/lib/rabbitmq --hostname rabbit-management --name rabbit-management -p 15672:15672 -p 15671:15671 -p 5672:5672 -p 5671:5671 rabbitmq:3.7-management
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


### Atributo "type"
O atributo "type" do broker deve ser um nome de assembly completo, por exemplo *Benner.Tecnologia.Messaging.ActiveMQConfig, Benner.Tecnologia.Messaging*.

### Restrição para nome de filas
Existem algumas regras para a criação e utilização de filas ao que diz respeito ao seu nome:

 1. Um nome de fila deve começar com uma letra ou número, e pode conter apenas letras, números e hífen (-).
 2. A primeira e última letras devem ser alfanuméricos. O hífen (-) não pode ser o primeiro nem último caractere. Hífens consecutivos também não são permitidos.
 3. Todas as letras no nome devem ser minúsculas.
 4. O nome deve possuir de 3 a 63 caracteres.

Estas regras são validadas na configuração em memória e por arquivo (a seguir).

### Estrutura do arquivo
O arquivo de configuração para o serviço de mensageria, conforme o arquivo messaging.config.modelo, tem a seguinte estrutura:

	<?xml version="1.0" encoding="utf-8" ?>
	<configuration>
	  <configSections>
	    <section name="MessagingConfigSection" type="Benner.Tecnologia.Messaging.MessagingFileConfigSection, Benner.Tecnologia.Messaging" />
	  </configSections>
	  <MessagingConfigSection>
	    <queues>
	      <queue name="nome_da_fila" broker="nome_do_broker" />
	    </queues>
	    <brokerList default="Name_do_broker_default">
	      <broker name="nome_do_broker" type="Benner.Tecnologia.Messaging.Classname, Benner.Tecnologia.Messaging">
	        <add key="Propriedade" value="Valor" />
	      </broker>
	    </brokerList>
	  </MessagingConfigSection>
	</configuration>

Para utilizar esta forma de configuração na API, utiliza-se a classe *Benner.Tecnologia.Messaging.FileMessagingConfig*.

|Construtor|Descrição|
|--|--|
|FileMessagingConfig()|Utiliza-se o arquivo *messaging.config* encontrado no mesmo diretório que a dll do seu projeto.|
|FileMessagingConfig(string)|Utiliza o arquivo *.config* informado. Deve-se informar o caminho completo do arquivo.|

Esta classe faz diversas validações na estrutura do arquivo, portanto se houver algum erro de estrutura, uma exceção é acionada.

### Tags
 - **MessagingConfigSection**: Responsável por toda a configuração de filas e serviços da API.
	 - **queues**: Contém todas as filas pré-configuradas.
		 - **queue**:Uma fila pré-configurada. Contém o nome da fila que será criada ou usada (name) e qual configuração de serviço utilizar (broker). O broker equivale ao *name* da tag *broker* (ver abaixo).
	 - **brokerList**: Contém todas as configurações de serviços. O atributo *default* define a configuração que será utilizada caso seja informada uma fila que não está pré-configurada. Equivale ao *name* da tag *broker*.
		 - **broker**: A tag de configuração. O atributo *name* nomeia a configuração, pois é possível configurar o mesmo serviço de vários modos diferentes, com diferentes nomes. O atributo *type* informa qual classe de configuração é utilizada, ou seja, o serviço em si. Deve-se alterar apenas *classname*.
			 - **add**: Tag de propriedades para os serviços. Cada serviço terá suas próprias propriedades e particularidades.

### Configuração em memória
Além da configuração fixa por arquivo, também é possível configurar a API através da classe *Benner.Tecnologia.Messaging.MemoryMessagingConfig*, que é obtida através da classe builder *Benner.Tecnologia.Messaging.MemoryMessagingConfigBuilder*.
Por exemplo, vamos criar uma configuração para utilizar 2 serviços, ActiveMQ e RabbitMQ, usando RabbitMQ como default

	// Propriedades de configuração do RabbitMQ
	var configRabbit = new Dictionary<string, string>() { { "Hostname", "bnu-vtec012" } };
	
	// Propriedades de configuração do ActiveMQ
    var configActive = new Dictionary<string, string>() { { "Hostname", "bnu-vtec012" } };
    
    // Instanciando o Builder informando o nome do broker padrão, seu tipo e configurações
	var config = new MemoryMessagingConfigBuilder("rabbit", Broker.Rabbit, configRabbit)
		 //Adicionando o broker active e sua respectiva configuração
         .WithBroker("active", Broker.ActiveMQ, configActive)
         // Adicionando algumas filas pré-configuradas para utilização
         .WithQueues(new Dictionary<string, string>
         {
             {"fila-notas", "rabbit"},
             {"fila-lancamentos", "rabbit"},
             {"fila-financeiro", "active"}
         })
         // Criando o objeto MemoryMessagingConfig
         .Create();

### ActiveMQConfig Settings
|Nome|Descrição|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o contêiner está rodando.|
|Username|O nome de usuário para login no serviço. Padrão: admin.|
|Password|A senha para login no serviço. Padrão: admin.|
|Port|O número da porta do host do serviço. Padrão: 61616.|

### AmazonSQSConfig  Settings
O serviço da amazon possui uma particularidade quanto ao modo de estabelecer uma conexão. Para mais informações: [Configurando credenciais AWS (em inglês)](https://docs.aws.amazon.com/sdk-for-net/v2/developer-guide/net-dg-config-creds.html)

|Nome|Descrição|
|--|--|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficará invisível para outros consumidores até ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica visível novamente na fila, disponível para outros consumidores.|

### AzureMQConfig Settings
|Nome|Descrição|
|--|--|
|ConnectionString|A string de conexão fornecida pelo serviço para estabelecer a conexão|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficará invisível para outros consumidores até ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica visível novamente na fila, disponível para outros consumidores.|

### RabbitMQConfig Settings
|Nome|Descrição|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o contêiner está rodando.|
|Username|O nome de usuário para login no serviço. Padrão: guest.|
|Password|A senha para login no serviço. Padrão: guest.|
|Port|O número da porta do host do serviço. Padrão: 5672.|

## A classe *Client*
A principal classe para uso da API é a *Benner.Tecnologia.Messaging.Client*. Ela possui os métodos para envio e recebimento de mensagens, e pode ser usada como estática ou instância. 


### Como instância

|Construtor|Descrição|
|--|--|
|Client()|Instancia um objeto *Client* utilizando configuração de arquivo (padrão de *FileMessagingConfig*).|
|Client(IMessagingConfig)|Recebe a configuração a ser utilizada (arquivo ou memória).|

|Método|Descrição|
|--|--|
|EnqueueMessage(string, string)|Enfileira uma mensagem *string* na fila informada|
|EnqueueMessage(string, object)|Enfileira uma mensagem de um *object* (serializado) na fila informada|
|StartListening(string, Func<MessagingArgs, bool>)|Escuta a fila informada, recebendo as mensagens através de um método (anônimo ou não) que recebe as mensagens por um objeto *MessagingArgs*|

Quando utilizado o método ***StartListening***, existem 3 simples cenários  para o recebimento das mensagens. O método informado no parâmetro *Func* recebe um parâmetro *MessagingArgs* que possui a mensagem em forma de *string* e *byte[]*, e também um método para desserializar a mensagem para um objeto. 
O método faz seus tratamentos com a mensagem e retorna *true*, *false* ou aciona uma exceção:

 - **True**: a mensagem é considerada consumida com sucesso, portanto esta é deletada da fila.
 - **False**: a mensagem é considerada recebida mas com alguma falha de processamento. A mensagem volta para fila e fica disponível para outros consumidores.
 - **Exceção**: a mensagem é considerada perigosa, sendo retirada da fila e enfileirada em uma outra fila de erros, cujo nome é o nome da fila que está sendo consumida com um sufixo '-error'. Por exemplo: se o nome  for 'fila-teste', a fila de erros será 'fila-teste-error'. 

Deve-se utilizar esse método com certo cuidado, uma vez que ele é um "escutador", recomenda-se deixá-lo parado, como algum tipo de espera após ele, por exemplo:
			
	// Instanciando um novo Client de configuração padrão
    var client = new Client();

    // Escutando a fila
    client.StartListening("fila", (args) =>
    {
        if (!args.AsString.Contains("zyx"))
            throw new Exception("Mensagem em formato incorreto.");

        if (args.AsString.Contains("abxz"))
        {
            Console.WriteLine(args.AsString);
            return true;
        }
        return false;
    });

    // Deixando o programa em espera, permitindo o consumo de mensagens
    Console.ReadKey();

    // Liberando o objeto das conexões. Fortemente recomendado
    client.Dispose();

### Como estática

|Método|Descrição|
|--|--|
|DequeueSingleMessage(string, IMessagingConfig)|Recebe a próxima mensagem da fila, retornando-a em *string*.|
|DequeueSingleMessage<T>(string, IMessagingConfig)|Recebe a próxima mensagem da fila e a desserializa para um objeto do tipo ***T***. Caso ocorra um erro de desserialização a mensagem é perdida.|
|EnqueueSingleMessage(string, object, IMessagingConfig)|Enfileira uma mensagem *string* na fila informada|
|EnqueueSingleMessage(string, string, IMessagingConfig)|Enfileira uma mensagem de um *object* (serializado) na fila informada|

Em todos estes métodos,  o parâmetro de configuração é opcional (default sendo o construtor padrão de *FileMessagingConfig*), todavia é recomendado informar a configuração.

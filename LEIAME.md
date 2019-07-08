# Benner.Messaging

Benner.Messaging é uma biblioteca leve em .NET Standard para lidar com qualquer serviço de mensageria com facilidade.
Benner.Messaging suporta RabbitMQ, ActiveMQ, Amazon SQS e Azure Queue. É uma biblioteca _gratuita e aberta_ sob a licença MIT.

## Estado de compilação
Branch | Estado
--- | :---:
master | [![Build Status](https://dev.azure.com/benner-tecnologia/benner-tecnologia/_apis/build/status/benner-sistemas.messaging?branchName=master)](https://dev.azure.com/benner-tecnologia/benner-tecnologia/_build/latest?definitionId=2&branchName=master)


## Pacote Nuget
|Nome do pacote|.NET Framework|.NET Standard|
|:--:|:--:|:--:|
|[Benner.Messaging](https://www.nuget.org/packages/Benner.Messaging/)|4.6.1|2.0|

## Uma API de mensageria para todos conquistar!

Providenciamos uma API extremamente simples e intuitiva para envio e recebimento de mensagens para os serviços RabbitMQ, ActiveMQ, Amazon SQS e Azure Queue:

`Messaging.Enqueue("nome-da-fila", "olá mundo!");`

`var mensagem = Messaging.Dequeue("nome-da-fila");`

Sim, você entendeu certo! Você programa apenas uma vez, de modo muito simples, e utiliza o código em qualquer serviço de mensageria. Sem amarras tecnológicas, sem refatoração de código.
Toda magia se baseia em configuração (de arquivo ou em código).

## O comportamento importa
É importante notar que a API nasceu para funcionar de um modo específico:
* Primeiro a entrar, primeiro a sair
* Um produtor envia
* Apenas um consumidor recebe
* Se o consumo falhar no recebimento ou processamento, a mensagem retorna para a fila (***MENTIRA PARCIAL***)
* A mensagem não será perdida (***MENTIRA***)

As operações de enfileirar e desenfileirar foram desenhadas para assegurar que uma mensagem enviada pelo _produtor_ chegue no _receptor_. 
Isso significa que seguimos a abordagem "_Publicador confirma_" e "_Consumidor reconhece_" em todos os serviços suportados. (***MENTIRA?***)

## Como começar

### Enviando mensagem

Crie um novo projeto:
```
dotnet new console -n producer
cd producer
```

Adicione Benner.Messaging e abra o projeto no VSCode:
```
dotnet add package benner.messaging
code .
```

Adicione _using_ e envie uma única mensagem para alguma fila:
```
using Benner.Messaging;
Messaging.Enqueue("nome-da-fila", "olá mundo!");
```

É isso! Execute `dotnet run` e você obterá:
```
Messaging config not found
```

### Configuração dos serviços

Você vai precisar de um arquivo `messaging.config` como a seguir:
```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
		<configSections>
			<section name="MessagingConfigSection" type="Benner.Messaging.MessagingFileConfigSection, Benner.Messaging" />
		</configSections>
		<MessagingConfigSection>
			<queues>
				<queue name="queue-name" broker="broker-name" />
			</queues>
			<brokerList default="RabbitMQ">
				<broker name="AzureMQ" type="Benner.Messaging.AzureMQConfig, Benner.Messaging">
					<add key="ConnectionString" value="DefaultEndpointsProtocol=https;AccountName=accountName;AccountKey=accountKey;EndpointSuffix=core.windows.net" />
					<add key="InvisibilityTime" value="15" />
				</broker>
				<broker name="RabbitMQ" type="Benner.Messaging.RabbitMQConfig, Benner.Messaging">
					<add key="Username" value="username" />
					<add key="Password" value="password" />
					<add key="Hostname" value="servername" />
					<add key="Port" value="port" />
				</broker>
				<broker name="Amazon" type="Benner.Messaging.AmazonSqsConfig, Benner.Messaging">
					<add key="InvisibilityTime" value="15" />
				</broker>
				<broker name="ActiveMQ" type="Benner.Messaging.ActiveMQConfig, Benner.Messaging">
					<add key="Hostname" value="servername" />
					<add key="Password" value="password" />
					<add key="Hostname" value="servername" />
					<add key="Port" value="port" />
				</broker>
			</brokerList>
		</MessagingConfigSection>
</configuration>
```

Você pode, como alternativa, injetar a configuração a partir de código:
```
var config = MessagingConfigFactory
	.NewMessagingConfigFactory()
	.WithRabbitMQBroker("hostname", 5672, "user", "password")
	.Create();

Messaging.Enqueue("nome-da-fila", "olá mundo!", config);
```

É isso! Execute `dotnet run` e você obterá:
```
Unable to connect to RabbitMQ server
```

### Providenciando um serviço

Você precisa, nesse caso, de um RabbitMQ em funcionamento.
Vamos usar um contêiner Docker para nos ajudar. Podemos executar o seguinte comando:
```
docker run -d -v path/to/rabbit/volume/folder:/var/lib/rabbitmq --hostname rabbit-management --name rabbit-management -p 15672:15672 -p 15671:15671 -p 5672:5672 -p 5671:5671 rabbitmq:3.7-management
```

TODO:(more details)
Tudo pronto! Execute `dotnet run` novamente. Sem erros? Ótimo, e agora?

### Acesse o gerenciador do seu RabbitMQ

Em seu navegador, acesse `http://hostname::15672` para gerenciar o serviço. Aqui você pode ver suas filas com suas mensagens dentro.

### Recebendo uma mensagem
Crie um novo projeto:
```
dotnet new console -n consumer
cd consumer
```

Adicione Benner.Messaging e abra o projeto no VSCode:
```
dotnet add package benner.messaging
code .
```

Adicione _using_ e receba uma mensagem da fila, sem esquecer da configuração:
```
using Benner.Messaging;

var config = MessagingConfigFactory
	.NewMessagingConfigFactory()
	.WithRabbitMQBroker("hostname", 5672, "user", "password")
	.Create();

var mensagem = Messaging.Dequeue("nome-da-fila", config);

Console.Write(mensagem);
```

É isso! Execute `dotnet run` e você receberá:
```
olá mundo! 
```

### Agora vamos fazer isso insanamente

Mude o código do consumidor para:
```
TODO
```

Execute `dotnet run` em 3 terminais diferentes e você terá 3 consumidores simultâneos.

Mude o código do produtor para:
```
TODO
```

Execute `dotnet run` e você verá o que acontece.

### Atributo "type"
O atributo "type" do broker deve ser um nome de assembly completo, por exemplo *Benner.Messaging.ActiveMQConfig, Benner.Messaging*.

### Restrição para nome de filas
Existem algumas regras para a criação e utilização de filas ao que diz respeito ao seu nome:

 1. Um nome de fila deve começar com uma letra ou número, e pode conter apenas letras, números e hífen (-).
 2. A primeira e última letras devem ser alfanuméricos. O hífen (-) não pode ser o primeiro nem último caractere. Hífens consecutivos também não são permitidos.
 3. Todas as letras no nome devem ser minúsculas.
 4. O nome deve possuir de 3 a 63 caracteres.

Estas regras são validadas na configuração em memória e por arquivo (a seguir).

### Estrutura do arquivo
O arquivo de configuração para o serviço de mensageria, conforme o arquivo messaging.config.model, tem a seguinte estrutura:

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

Para utilizar esta forma de configuração na API, utiliza-se a classe *Benner.Messaging.FileMessagingConfig*.

|Construtor|Descrição|
|--|--|
|FileMessagingConfig()|Utiliza-se o arquivo *messaging.config* encontrado no mesmo diretório que a dll do seu projeto.|
|FileMessagingConfig(string)|Utiliza o arquivo *.config* informado. Deve-se informar o caminho completo do arquivo.|

Esta classe faz diversas validações na estrutura do arquivo, portanto se houver algum erro de estrutura, uma exceção é acionada.

### Tags
 - **MessagingConfigSection**: Responsável por toda a configuração de filas e serviços da API.
	 - **queues**: Contém todas as filas pré-configuradas.
		 - **queue**: Uma fila pré-configurada. Contém o nome da fila que será criada ou usada (name) e qual configuração de serviço utilizar (broker). O broker equivale ao *name* da tag *broker* (ver abaixo).
	 - **brokerList**: Contém todas as configurações de serviços. O atributo *default* define a configuração que será utilizada caso seja informada uma fila que não está pré-configurada. Equivale ao *name* da tag *broker*.
		 - **broker**: A tag de configuração. O atributo *name* nomeia a configuração, pois é possível configurar o mesmo serviço de vários modos diferentes, com diferentes nomes. O atributo *type* informa qual classe de configuração é utilizada, ou seja, o serviço em si. Deve-se alterar apenas *classname*.
			 - **add**: Tag de propriedades para os serviços. Cada serviço terá suas próprias propriedades e particularidades.

### Configuração em memória
Além da configuração fixa por arquivo, também é possível configurar a API através da classe *Benner.Messaging.MessagingConfig*, que é obtida através da classe *Benner.Messaging.MessagingConfigFactory*.
Por exemplo, vamos criar uma configuração para utilizar 2 serviços, ActiveMQ e RabbitMQ, usando RabbitMQ como default (***MENTIRA***)

	// Propriedades de configuração do RabbitMQ
	var configRabbit = new Dictionary<string, string>() { { "Hostname", "nome-servidor" } };
	
	// Propriedades de configuração do ActiveMQ
	var configActive = new Dictionary<string, string>() { { "Hostname", "nome-servidor" } };

	var config = MessagingConfigFactory.NewMessagingConfigFactory()
		// Adicionando o broker active e sua respectiva configuração
		.WithActiveMQBroker(configActive)
		// Adicionando o broker rabbit e sua respectiva configuração
		.WithRabbitMQBroker(configRabbit)
		// Adicionando algumas filas pré-configuradas para utilização
		.WithMappedQueue("fila-notas", "rabbit")
		.WithMappedQueue("fila-lancamentos", "rabbit")
		.WithMappedQueue("fila-financeiro", "active")
		// Criando o objeto MessagingConfig
		.Create();

### Propriedades do ActiveMQConfig
|Nome|Descrição|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o contêiner está rodando.|
|Username|O nome de usuário para login no serviço. Padrão: admin.|
|Password|A senha para login no serviço. Padrão: admin.|
|Port|O número da porta do host do serviço. Padrão: 61616.|

### Propriedades do AmazonSQSConfig
O serviço da amazon possui uma particularidade quanto ao modo de estabelecer uma conexão. Para mais informações: [Configurando credenciais AWS (em inglês)](https://docs.aws.amazon.com/sdk-for-net/v2/developer-guide/net-dg-config-creds.html)

|Nome|Descrição|
|--|--|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficará invisível para outros consumidores até ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica visível novamente na fila, disponível para outros consumidores.|

### Propriedades do AzureMQConfig
|Nome|Descrição|
|--|--|
|ConnectionString|A string de conexão fornecida pelo serviço para estabelecer a conexão|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficará invisível para outros consumidores até ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica visível novamente na fila, disponível para outros consumidores.|

### Propriedades do RabbitMQConfig
|Nome|Descrição|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o contêiner está rodando.|
|Username|O nome de usuário para login no serviço. Padrão: guest.|
|Password|A senha para login no serviço. Padrão: guest.|
|Port|O número da porta do host do serviço. Padrão: 5672.|

## A classe *Messaging*
A principal classe para uso da API é a *Benner.Messaging.Messaging*. Ela possui os métodos para envio e recebimento de mensagens, e pode ser usada como estática ou instância. 

### Como instância

|Construtor|Descrição|
|--|--|
|Messaging()|Instancia um objeto *Messaging* utilizando configuração de arquivo (padrão de *FileMessagingConfig*).|
|Messaging(IMessagingConfig)|Recebe a configuração a ser utilizada (arquivo ou memória).|

|Método|Descrição|
|--|--|
|EnqueueMessage(string, string)|Enfileira uma mensagem *string* na fila informada|
|EnqueueMessage(string, object)|Enfileira uma mensagem de um *object* (serializado) na fila informada|
|StartListening(string, Func<MessagingArgs, bool>)|Escuta a fila informada, recebendo as mensagens através de um método (anônimo ou não) que recebe as mensagens por um objeto *MessagingArgs*|

Quando utilizado o método ***StartListening***, existem 3 simples cenários para o recebimento das mensagens. O método informado no parâmetro *Func* recebe um parâmetro *MessagingArgs* que possui a mensagem em forma de *string* e *byte[]*, e também um método para desserializar a mensagem para um objeto. 
O método faz seus tratamentos com a mensagem e retorna *true*, *false* ou aciona uma exceção:

 - **True**: a mensagem é considerada consumida com sucesso, portanto esta é deletada da fila.
 - **False**: a mensagem é considerada recebida mas com alguma falha de processamento. A mensagem volta para fila e fica disponível para outros consumidores.
 - **Exceção**: a mensagem é considerada perigosa, sendo retirada da fila e enfileirada em uma outra fila de erros, cujo nome é o nome da fila que está sendo consumida com um sufixo '-error'. Por exemplo: se o nome for 'fila-teste', a fila de erros será 'fila-teste-error'. 

Deve-se utilizar esse método com certo cuidado, uma vez que ele é um "escutador", recomenda-se deixá-lo parado, como algum tipo de espera após ele, por exemplo:
			
	// Instanciando um novo Messaging de configuração padrão
	var messaging = new Messaging();

	// Escutando a fila
	messaging.StartListening("fila", (args) =>
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
	messaging.Dispose();

### Como estática

|Método|Descrição|
|--|--|
|Dequeue(string)|Recebe a próxima mensagem da fila, retornando-a em *string*.|
|Dequeue(string, IMessagingConfig)|Recebe a próxima mensagem da fila, retornando-a em *string*.|
|Dequeue<T>(string)|Recebe a próxima mensagem da fila e a desserializa para um objeto do tipo ***T***. Caso ocorra um erro de desserialização a mensagem é perdida.|
|Dequeue<T>(string, IMessagingConfig)|Recebe a próxima mensagem da fila e a desserializa para um objeto do tipo ***T***. Caso ocorra um erro de desserialização a mensagem é perdida.|
|Enqueue(string, object)|Enfileira uma mensagem *string* na fila informada.|
|Enqueue(string, object, IMessagingConfig)|Enfileira uma mensagem *string* na fila informada.|
|Enqueue(string, string)|Enfileira uma mensagem de um *object* (serializado) na fila informada.|
|Enqueue(string, string, IMessagingConfig)|Enfileira uma mensagem de um *object* (serializado) na fila informada.|

Alguns destes métodos não tem um parâmetro de configuração, pois utilizam a configuração padrão (construtor padrão de *FileMessagingConfig*).
Dito isto, é recomendado usar as sobrecargas com o parâmetro de configuração.
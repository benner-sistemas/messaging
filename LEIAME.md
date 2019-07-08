# Benner.Messaging

Benner.Messaging � uma biblioteca leve em .NET Standard para lidar com qualquer servi�o de mensageria com facilidade.
Benner.Messaging suporta RabbitMQ, ActiveMQ, Amazon SQS e Azure Queue. � uma biblioteca _gratuita e aberta_ sob a licen�a MIT.

## Estado de compila��o
Branch | Estado
--- | :---:
master | [![Build Status](https://dev.azure.com/benner-tecnologia/benner-tecnologia/_apis/build/status/benner-sistemas.messaging?branchName=master)](https://dev.azure.com/benner-tecnologia/benner-tecnologia/_build/latest?definitionId=2&branchName=master)


## Pacote Nuget
|Nome do pacote|.NET Framework|.NET Standard|
|:--:|:--:|:--:|
|[Benner.Messaging](https://www.nuget.org/packages/Benner.Messaging/)|4.6.1|2.0|

## Uma API de mensageria para todos conquistar!

Providenciamos uma API extremamente simples e intuitiva para envio e recebimento de mensagens para os servi�os RabbitMQ, ActiveMQ, Amazon SQS e Azure Queue:

`Messaging.Enqueue("nome-da-fila", "ol� mundo!");`

`var mensagem = Messaging.Dequeue("nome-da-fila");`

Sim, voc� entendeu certo! Voc� programa apenas uma vez, de modo muito simples, e utiliza o c�digo em qualquer servi�o de mensageria. Sem amarras tecnol�gicas, sem refatora��o de c�digo.
Toda magia se baseia em configura��o (de arquivo ou em c�digo).

## O comportamento importa
� importante notar que a API nasceu para funcionar de um modo espec�fico:
* Primeiro a entrar, primeiro a sair
* Um produtor envia
* Apenas um consumidor recebe
* Se o consumo falhar no recebimento ou processamento, a mensagem retorna para a fila (***MENTIRA PARCIAL***)
* A mensagem n�o ser� perdida (***MENTIRA***)

As opera��es de enfileirar e desenfileirar foram desenhadas para assegurar que uma mensagem enviada pelo _produtor_ chegue no _receptor_. 
Isso significa que seguimos a abordagem "_Publicador confirma_" e "_Consumidor reconhece_" em todos os servi�os suportados. (***MENTIRA?***)

## Como come�ar

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

Adicione _using_ e envie uma �nica mensagem para alguma fila:
```
using Benner.Messaging;
Messaging.Enqueue("nome-da-fila", "ol� mundo!");
```

� isso! Execute `dotnet run` e voc� obter�:
```
Messaging config not found
```

### Configura��o dos servi�os

Voc� vai precisar de um arquivo `messaging.config` como a seguir:
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

Voc� pode, como alternativa, injetar a configura��o a partir de c�digo:
```
var config = MessagingConfigFactory
	.NewMessagingConfigFactory()
	.WithRabbitMQBroker("hostname", 5672, "user", "password")
	.Create();

Messaging.Enqueue("nome-da-fila", "ol� mundo!", config);
```

� isso! Execute `dotnet run` e voc� obter�:
```
Unable to connect to RabbitMQ server
```

### Providenciando um servi�o

Voc� precisa, nesse caso, de um RabbitMQ em funcionamento.
Vamos usar um cont�iner Docker para nos ajudar. Podemos executar o seguinte comando:
```
docker run -d -v path/to/rabbit/volume/folder:/var/lib/rabbitmq --hostname rabbit-management --name rabbit-management -p 15672:15672 -p 15671:15671 -p 5672:5672 -p 5671:5671 rabbitmq:3.7-management
```

TODO:(more details)
Tudo pronto! Execute `dotnet run` novamente. Sem erros? �timo, e agora?

### Acesse o gerenciador do seu RabbitMQ

Em seu navegador, acesse `http://hostname::15672` para gerenciar o servi�o. Aqui voc� pode ver suas filas com suas mensagens dentro.

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

Adicione _using_ e receba uma mensagem da fila, sem esquecer da configura��o:
```
using Benner.Messaging;

var config = MessagingConfigFactory
	.NewMessagingConfigFactory()
	.WithRabbitMQBroker("hostname", 5672, "user", "password")
	.Create();

var mensagem = Messaging.Dequeue("nome-da-fila", config);

Console.Write(mensagem);
```

� isso! Execute `dotnet run` e voc� receber�:
```
ol� mundo! 
```

### Agora vamos fazer isso insanamente

Mude o c�digo do consumidor para:
```
TODO
```

Execute `dotnet run` em 3 terminais diferentes e voc� ter� 3 consumidores simult�neos.

Mude o c�digo do produtor para:
```
TODO
```

Execute `dotnet run` e voc� ver� o que acontece.

### Atributo "type"
O atributo "type" do broker deve ser um nome de assembly completo, por exemplo *Benner.Messaging.ActiveMQConfig, Benner.Messaging*.

### Restri��o para nome de filas
Existem algumas regras para a cria��o e utiliza��o de filas ao que diz respeito ao seu nome:

 1. Um nome de fila deve come�ar com uma letra ou n�mero, e pode conter apenas letras, n�meros e h�fen (-).
 2. A primeira e �ltima letras devem ser alfanum�ricos. O h�fen (-) n�o pode ser o primeiro nem �ltimo caractere. H�fens consecutivos tamb�m n�o s�o permitidos.
 3. Todas as letras no nome devem ser min�sculas.
 4. O nome deve possuir de 3 a 63 caracteres.

Estas regras s�o validadas na configura��o em mem�ria e por arquivo (a seguir).

### Estrutura do arquivo
O arquivo de configura��o para o servi�o de mensageria, conforme o arquivo messaging.config.model, tem a seguinte estrutura:

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

Para utilizar esta forma de configura��o na API, utiliza-se a classe *Benner.Messaging.FileMessagingConfig*.

|Construtor|Descri��o|
|--|--|
|FileMessagingConfig()|Utiliza-se o arquivo *messaging.config* encontrado no mesmo diret�rio que a dll do seu projeto.|
|FileMessagingConfig(string)|Utiliza o arquivo *.config* informado. Deve-se informar o caminho completo do arquivo.|

Esta classe faz diversas valida��es na estrutura do arquivo, portanto se houver algum erro de estrutura, uma exce��o � acionada.

### Tags
 - **MessagingConfigSection**: Respons�vel por toda a configura��o de filas e servi�os da API.
	 - **queues**: Cont�m todas as filas pr�-configuradas.
		 - **queue**: Uma fila pr�-configurada. Cont�m o nome da fila que ser� criada ou usada (name) e qual configura��o de servi�o utilizar (broker). O broker equivale ao *name* da tag *broker* (ver abaixo).
	 - **brokerList**: Cont�m todas as configura��es de servi�os. O atributo *default* define a configura��o que ser� utilizada caso seja informada uma fila que n�o est� pr�-configurada. Equivale ao *name* da tag *broker*.
		 - **broker**: A tag de configura��o. O atributo *name* nomeia a configura��o, pois � poss�vel configurar o mesmo servi�o de v�rios modos diferentes, com diferentes nomes. O atributo *type* informa qual classe de configura��o � utilizada, ou seja, o servi�o em si. Deve-se alterar apenas *classname*.
			 - **add**: Tag de propriedades para os servi�os. Cada servi�o ter� suas pr�prias propriedades e particularidades.

### Configura��o em mem�ria
Al�m da configura��o fixa por arquivo, tamb�m � poss�vel configurar a API atrav�s da classe *Benner.Messaging.MessagingConfig*, que � obtida atrav�s da classe *Benner.Messaging.MessagingConfigFactory*.
Por exemplo, vamos criar uma configura��o para utilizar 2 servi�os, ActiveMQ e RabbitMQ, usando RabbitMQ como default (***MENTIRA***)

	// Propriedades de configura��o do RabbitMQ
	var configRabbit = new Dictionary<string, string>() { { "Hostname", "nome-servidor" } };
	
	// Propriedades de configura��o do ActiveMQ
	var configActive = new Dictionary<string, string>() { { "Hostname", "nome-servidor" } };

	var config = MessagingConfigFactory.NewMessagingConfigFactory()
		// Adicionando o broker active e sua respectiva configura��o
		.WithActiveMQBroker(configActive)
		// Adicionando o broker rabbit e sua respectiva configura��o
		.WithRabbitMQBroker(configRabbit)
		// Adicionando algumas filas pr�-configuradas para utiliza��o
		.WithMappedQueue("fila-notas", "rabbit")
		.WithMappedQueue("fila-lancamentos", "rabbit")
		.WithMappedQueue("fila-financeiro", "active")
		// Criando o objeto MessagingConfig
		.Create();

### Propriedades do ActiveMQConfig
|Nome|Descri��o|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o cont�iner est� rodando.|
|Username|O nome de usu�rio para login no servi�o. Padr�o: admin.|
|Password|A senha para login no servi�o. Padr�o: admin.|
|Port|O n�mero da porta do host do servi�o. Padr�o: 61616.|

### Propriedades do AmazonSQSConfig
O servi�o da amazon possui uma particularidade quanto ao modo de estabelecer uma conex�o. Para mais informa��es: [Configurando credenciais AWS (em ingl�s)](https://docs.aws.amazon.com/sdk-for-net/v2/developer-guide/net-dg-config-creds.html)

|Nome|Descri��o|
|--|--|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficar� invis�vel para outros consumidores at� ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica vis�vel novamente na fila, dispon�vel para outros consumidores.|

### Propriedades do AzureMQConfig
|Nome|Descri��o|
|--|--|
|ConnectionString|A string de conex�o fornecida pelo servi�o para estabelecer a conex�o|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficar� invis�vel para outros consumidores at� ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica vis�vel novamente na fila, dispon�vel para outros consumidores.|

### Propriedades do RabbitMQConfig
|Nome|Descri��o|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o cont�iner est� rodando.|
|Username|O nome de usu�rio para login no servi�o. Padr�o: guest.|
|Password|A senha para login no servi�o. Padr�o: guest.|
|Port|O n�mero da porta do host do servi�o. Padr�o: 5672.|

## A classe *Messaging*
A principal classe para uso da API � a *Benner.Messaging.Messaging*. Ela possui os m�todos para envio e recebimento de mensagens, e pode ser usada como est�tica ou inst�ncia. 

### Como inst�ncia

|Construtor|Descri��o|
|--|--|
|Messaging()|Instancia um objeto *Messaging* utilizando configura��o de arquivo (padr�o de *FileMessagingConfig*).|
|Messaging(IMessagingConfig)|Recebe a configura��o a ser utilizada (arquivo ou mem�ria).|

|M�todo|Descri��o|
|--|--|
|EnqueueMessage(string, string)|Enfileira uma mensagem *string* na fila informada|
|EnqueueMessage(string, object)|Enfileira uma mensagem de um *object* (serializado) na fila informada|
|StartListening(string, Func<MessagingArgs, bool>)|Escuta a fila informada, recebendo as mensagens atrav�s de um m�todo (an�nimo ou n�o) que recebe as mensagens por um objeto *MessagingArgs*|

Quando utilizado o m�todo ***StartListening***, existem 3 simples cen�rios para o recebimento das mensagens. O m�todo informado no par�metro *Func* recebe um par�metro *MessagingArgs* que possui a mensagem em forma de *string* e *byte[]*, e tamb�m um m�todo para desserializar a mensagem para um objeto. 
O m�todo faz seus tratamentos com a mensagem e retorna *true*, *false* ou aciona uma exce��o:

 - **True**: a mensagem � considerada consumida com sucesso, portanto esta � deletada da fila.
 - **False**: a mensagem � considerada recebida mas com alguma falha de processamento. A mensagem volta para fila e fica dispon�vel para outros consumidores.
 - **Exce��o**: a mensagem � considerada perigosa, sendo retirada da fila e enfileirada em uma outra fila de erros, cujo nome � o nome da fila que est� sendo consumida com um sufixo '-error'. Por exemplo: se o nome for 'fila-teste', a fila de erros ser� 'fila-teste-error'. 

Deve-se utilizar esse m�todo com certo cuidado, uma vez que ele � um "escutador", recomenda-se deix�-lo parado, como algum tipo de espera ap�s ele, por exemplo:
			
	// Instanciando um novo Messaging de configura��o padr�o
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

	// Liberando o objeto das conex�es. Fortemente recomendado
	messaging.Dispose();

### Como est�tica

|M�todo|Descri��o|
|--|--|
|Dequeue(string)|Recebe a pr�xima mensagem da fila, retornando-a em *string*.|
|Dequeue(string, IMessagingConfig)|Recebe a pr�xima mensagem da fila, retornando-a em *string*.|
|Dequeue<T>(string)|Recebe a pr�xima mensagem da fila e a desserializa para um objeto do tipo ***T***. Caso ocorra um erro de desserializa��o a mensagem � perdida.|
|Dequeue<T>(string, IMessagingConfig)|Recebe a pr�xima mensagem da fila e a desserializa para um objeto do tipo ***T***. Caso ocorra um erro de desserializa��o a mensagem � perdida.|
|Enqueue(string, object)|Enfileira uma mensagem *string* na fila informada.|
|Enqueue(string, object, IMessagingConfig)|Enfileira uma mensagem *string* na fila informada.|
|Enqueue(string, string)|Enfileira uma mensagem de um *object* (serializado) na fila informada.|
|Enqueue(string, string, IMessagingConfig)|Enfileira uma mensagem de um *object* (serializado) na fila informada.|

Alguns destes m�todos n�o tem um par�metro de configura��o, pois utilizam a configura��o padr�o (construtor padr�o de *FileMessagingConfig*).
Dito isto, � recomendado usar as sobrecargas com o par�metro de configura��o.
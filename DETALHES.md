[<img align="right" src="https://emojipedia-us.s3.dualstack.us-west-1.amazonaws.com/thumbs/160/google/146/flag-for-united-states_1f1fa-1f1f8.png" width="35x"/>](https://github.com/benner-sistemas/messaging/blob/master/DETAILS.md)

# Configura��o da API

### Servi�os dispon�veis

At� o momento a API possibilita o uso de 4 servi�os diferentes, com as respectivas *classnames* a serem informadas na configura��o:

|Servi�o|Classname|
|--|--|
|**ActiveMQ**|ActiveMQConfig|
|**Amazon SQS**|AmazonSQSConfig|
|**Azure Queue**|AzureMQConfig|
|**RabbitMQ**|RabbitMQConfig|

O atributo "type" do broker deve ser um nome de assembly completo, por exemplo *Benner.Messaging.ActiveMQConfig, Benner.Messaging*.

### Restri��o para nome de filas

Existem algumas regras para a cria��o e utiliza��o de filas ao que diz respeito ao seu nome:

 1. Um nome de fila deve come�ar com uma letra ou n�mero, e pode conter apenas letras, n�meros e h�fen (-).
 2. A primeira e �ltima letras devem ser alfanum�ricos. O h�fen (-) n�o pode ser o primeiro nem �ltimo caractere. H�fens consecutivos tamb�m n�o s�o permitidos.
 3. Todas as letras no nome devem ser min�sculas.
 4. O nome deve possuir de 3 a 63 caracteres.

Estas regras s�o validadas na configura��o em mem�ria e por arquivo (a seguir).

## Configura��o por arquivo

O arquivo de configura��o para o servi�o de mensageria, conforme o arquivo messaging.config.model, tem a seguinte estrutura:

```xml
<MessagingConfigSection>
   <brokerList default="Name_do_broker_default">
      <broker name="nome_do_broker" type="Benner.Messaging.Classname, Benner.Messaging">
         <add key="Propriedade" value="Valor" />
      </broker>
   </brokerList>
   <queues>
      <queue name="nome_da_fila" broker="nome_do_broker" />
   </queues>
</MessagingConfigSection>
```

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

## Configura��o em mem�ria

Al�m da configura��o fixa por arquivo, tamb�m � poss�vel configurar a API atrav�s da classe *Benner.Messaging.MessagingConfig*, que � obtida atrav�s da classe *Benner.Messaging.MessagingConfigBuilder*.
Por exemplo, vamos criar uma configura��o para utilizar 2 servi�os, ActiveMQ e RabbitMQ, usando RabbitMQ como default

```csharp
// Propriedades de configura��o do RabbitMQ
var configRabbit = new Dictionary<string, string>() { { "Hostname", "nome-servidor" } };
	
// Instanciando o builder com broker default
var config = new MessagingConfigBuilder("rabbit", BrokerType.RabbitMQ, rabbitConfig)
	// Adicionando o broker active e sua respectiva configura��o
	.WithActiveMQBroker("active", "nome-servidor")
	// Adicionando algumas filas pr�-configuradas para utiliza��o
	.WithMappedQueue("fila-notas", "rabbit")
	.WithMappedQueue("fila-lancamentos", "rabbit")
	.WithMappedQueue("fila-financeiro", "active")
	// Criando o objeto MessagingConfig
	.Create();
```

## Propriedades dos servi�os

### ActiveMQConfig

|Nome|Descri��o|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o cont�iner est� rodando.|
|Username|O nome de usu�rio para login no servi�o. Padr�o: admin.|
|Password|A senha para login no servi�o. Padr�o: admin.|
|Port|O n�mero da porta do host do servi�o. Padr�o: 61616.|

### AmazonSQSConfig

� poss�vel estabelecer conex�o com os servidores da Amazon de duas maneiras simples.
 - Atrav�s de um arquivo de credenciais. Saiba mais em: [Configurando credenciais AWS (em ingl�s)](https://docs.aws.amazon.com/sdk-for-net/v2/developer-guide/net-dg-config-creds.html#creds-file).
 - Atrav�s de propriedades na configura��o da API.

Quando o arquivo for detectado, este � usado e as credenciais (configura��o de arquivo ou mem�ria da API) n�o s�o obrigat�rias.
Caso n�o seja encontrado o arquivo, tais propriedades s�o obrigat�rias. Caso n�o sejam preenchidas ou sejam inv�lidas, uma exce��o � lan�ada.
Existem algumas outras formas de estabelecer conex�o, mas aqui visamos as duas formas mais simples.

|Nome|Descri��o|
|--|--|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficar� invis�vel para outros consumidores at� ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica vis�vel novamente na fila, dispon�vel para outros consumidores.|
|AccessKeyId|O identificador da chave de acesso fornecida pela Amazon.|
|SecretAccessKey|A chave de acesso secreta fornecida pela Amazon.|

Quando usar as propriedades AccessKeyId e SecretAccessKey, � recomendado n�o utiliz�-las por configura��o em mem�ria.

### AzureMQConfig

|Nome|Descri��o|
|--|--|
|ConnectionString|A string de conex�o fornecida pelo servi�o para estabelecer a conex�o|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficar� invis�vel para outros consumidores at� ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica vis�vel novamente na fila, dispon�vel para outros consumidores.|

### RabbitMQConfig

|Nome|Descri��o|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o cont�iner est� rodando.|
|Username|O nome de usu�rio para login no servi�o. Padr�o: guest.|
|Password|A senha para login no servi�o. Padr�o: guest.|
|Port|O n�mero da porta do host do servi�o. Padr�o: 5672.|

# A classe *Messaging*

A principal classe para uso da API � a *Benner.Messaging.Messaging*. Ela possui os m�todos para envio e recebimento de mensagens, e pode ser usada como est�tica ou inst�ncia. 

## Como inst�ncia

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
			
```csharp
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
```

## Como est�tica

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
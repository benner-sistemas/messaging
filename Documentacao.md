# Introdu��o
Esta documenta��o visa ensinar como configurar, enviar e receber mensagens atrav�s da API de mensageria Benner. 
Esta API foi concebida para simplificar o m�ximo poss�vel a utiliza��o de diversos servi�os de mensageria de forma gen�rica em um �nico lugar, dando comportamentos semelhantes para todas elas.

# Configura��o da API

## Servi�os dispon�veis
At� o momento a API possibilita o uso de 4 servi�os diferentes, com as respectivas *classnames* a serem informadas na configura��o:

|Servi�o|Classname|
|--|--|
|**ActiveMQ**|ActiveMQConfig|
|**Amazon SQS**|AmazonSqsConfig|
|**Azure Queue**|AzureMQConfig|
|**RabbitMQ**|RabbitMQConfig|

Obs.: Refor�ando que o atributo "type" do broker deve ser um nome de assembly completo, por exemplo *Benner.Tecnologia.Messaging.ActiveMQConfig, Benner.Tecnologia.Messaging*.

### Nomes de filas
Existem algumas regras para a cria��o e utiliza��o de filas ao que diz respeito ao seu nome:

 1. Um nome de fila deve come�ar com uma letra ou n�mero, e pode conter apenas letras, n�meros e h�fen (-).
 2. A primeira e �ltima letras devem ser alfanum�ricos. O h�fen (-) n�o pode ser o primeiro nem �ltimo caractere. H�fens consecutivos tamb�m n�o s�o permitidos.
 3. Todas as letras no nome devem ser min�sculas.
 4. O nome deve possuir de 3 a 63 caracteres.

Estas regras s�o validadas na configura��o em mem�ria e por arquivo (a seguir).

## Configura��o por arquivo
O arquivo de configura��o para o servi�o de mensageria, conforme o arquivo messaging.config.modelo, tem a seguinte estrutura:

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

Para utilizar esta forma de configura��o na API, utiliza-se a classe *Benner.Tecnologia.Messaging.FileMessagingConfig*.

|Construtor|Descri��o|
|--|--|
|FileMessagingConfig()|Utiliza-se o arquivo *messaging.config* encontrado no mesmo diret�rio que a dll do seu projeto.|
|FileMessagingConfig(string)|Utiliza o arquivo *.config* informado. Deve-se informar o caminho completo do arquivo.|

Esta classe faz diversas valida��es na estrutura do arquivo, portanto se houver algum erro de estrutura, uma exce��o � acionada.

### Tags
 - **MessagingConfigSection**: Respons�vel por toda a configura��o de filas e servi�os da API.
	 - **queues**: Cont�m todas as filas pr�-configuradas.
		 - **queue**:Uma fila pr�-configurada. Cont�m o nome da fila que ser� criada ou usada (name) e qual configura��o de servi�o utilizar (broker). O broker equivale ao *name* da tag *broker* (ver abaixo).
	 - **brokerList**: Cont�m todas as configura��es de servi�os. O atributo *default* define a configura��o que ser� utilizada caso seja informada uma fila que n�o est� pr�-configurada. Equivale ao *name* da tag *broker*.
		 - **broker**: A tag de configura��o. O atributo *name* nomeia a configura��o, pois � poss�vel configurar o mesmo servi�o de v�rios modos diferentes, com diferentes nomes. O atributo *type* informa qual classe de configura��o � utilizada, ou seja, o servi�o em si. Deve-se alterar apenas *classname*.
			 - **add**: Tag de propriedades para os servi�os. Cada servi�o ter� suas pr�prias propriedades e particularidades.

## Configura��o em mem�ria
Al�m da configura��o fixa por arquivo, tamb�m � poss�vel configurar a API atrav�s da classe *Benner.Tecnologia.Messaging.MemoryMessagingConfig*, que � obtida atrav�s da classe builder *Benner.Tecnologia.Messaging.MemoryMessagingConfigBuilder*.
Por exemplo, vamos criar uma configura��o para utilizar 2 servi�os, ActiveMQ e RabbitMQ, usando RabbitMQ como default

	// Propriedades de configura��o do RabbitMQ
	var configRabbit = new Dictionary<string, string>() { { "Hostname", "bnu-vtec012" } };
	
	// Propriedades de configura��o do ActiveMQ
    var configActive = new Dictionary<string, string>() { { "Hostname", "bnu-vtec012" } };
    
    // Instanciando o Builder informando o nome do broker padr�o, seu tipo e configura��es
	var config = new MemoryMessagingConfigBuilder("rabbit", Broker.Rabbit, configRabbit)
		 //Adicionando o broker active e sua respectiva configura��o
         .WithBroker("active", Broker.ActiveMQ, configActive)
         // Adicionando algumas filas pr�-configuradas para utiliza��o
         .WithQueues(new Dictionary<string, string>
         {
             {"fila-notas", "rabbit"},
             {"fila-lancamentos", "rabbit"},
             {"fila-financeiro", "active"}
         })
         // Criando o objeto MemoryMessagingConfig
         .Create();

## Propriedades dos servi�os

### ActiveMQConfig
|Nome|Descri��o|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o cont�iner est� rodando.|
|Username|O nome de usu�rio para login no servi�o. Padr�o: admin.|
|Password|A senha para login no servi�o. Padr�o: admin.|
|Port|O n�mero da porta do host do servi�o. Padr�o: 61616.|

### AmazonSqsConfig
O servi�o da amazon possui uma particularidade quanto ao modo de estabelecer uma conex�o. Para mais informa��es: [Configurando credenciais AWS (em ingl�s)](https://docs.aws.amazon.com/sdk-for-net/v2/developer-guide/net-dg-config-creds.html)

|Nome|Descri��o|
|--|--|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficar� invis�vel para outros consumidores at� ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica vis�vel novamente na fila, dispon�vel para outros consumidores.|

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

# A classe *Client*
A principal classe para uso da API � a *Benner.Tecnologia.Messaging.Client*. Ela possui os m�todos para envio e recebimento de mensagens, e pode ser usada como est�tica ou inst�ncia. 


## Como inst�ncia

|Construtor|Descri��o|
|--|--|
|Client()|Instancia um objeto *Client* utilizando configura��o de arquivo (padr�o de *FileMessagingConfig*).|
|Client(IMessagingConfig)|Recebe a configura��o a ser utilizada (arquivo ou mem�ria).|

|M�todo|Descri��o|
|--|--|
|EnqueueMessage(string, string)|Enfileira uma mensagem *string* na fila informada|
|EnqueueMessage(string, object)|Enfileira uma mensagem de um *object* (serializado) na fila informada|
|StartListening(string, Func<MessagingArgs, bool>)|Escuta a fila informada, recebendo as mensagens atrav�s de um m�todo (an�nimo ou n�o) que recebe as mensagens por um objeto *MessagingArgs*|

Quando utilizado o m�todo ***StartListening***, existem 3 simples cen�rios  para o recebimento das mensagens. O m�todo informado no par�metro *Func* recebe um par�metro *MessagingArgs* que possui a mensagem em forma de *string* e *byte[]*, e tamb�m um m�todo para desserializar a mensagem para um objeto. 
O m�todo faz seus tratamentos com a mensagem e retorna *true*, *false* ou aciona uma exce��o:

 - **True**: a mensagem � considerada consumida com sucesso, portanto esta � deletada da fila.
 - **False**: a mensagem � considerada recebida mas com alguma falha de processamento. A mensagem volta para fila e fica dispon�vel para outros consumidores.
 - **Exce��o**: a mensagem � considerada perigosa, sendo retirada da fila e enfileirada em uma outra fila de erros, cujo nome � o nome da fila que est� sendo consumida com um sufixo '-error'. Por exemplo: se o nome  for 'fila-teste', a fila de erros ser� 'fila-teste-error'. 

Deve-se utilizar esse m�todo com certo cuidado, uma vez que ele � um "escutador", recomenda-se deix�-lo parado, como algum tipo de espera ap�s ele, por exemplo:
			
	// Instanciando um novo Client de configura��o padr�o
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

    // Liberando o objeto das conex�es. Fortemente recomendado
    client.Dispose();

## Como est�tica

|M�todo|Descri��o|
|--|--|
|DequeueSingleMessage(string, IMessagingConfig)|Recebe a pr�xima mensagem da fila, retornando-a em *string*.|
|DequeueSingleMessage<T>(string, IMessagingConfig)|Recebe a pr�xima mensagem da fila e a desserializa para um objeto do tipo ***T***. Caso ocorra um erro de desserializa��o a mensagem � perdida.|
|EnqueueSingleMessage(string, object, IMessagingConfig)|Enfileira uma mensagem *string* na fila informada|
|EnqueueSingleMessage(string, string, IMessagingConfig)|Enfileira uma mensagem de um *object* (serializado) na fila informada|

Em todos estes m�todos,  o par�metro de configura��o � opcional (default sendo o construtor padr�o de *FileMessagingConfig*), todavia � recomendado informar a configura��o.

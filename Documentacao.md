# Introdução
Esta documentação visa ensinar como configurar, enviar e receber mensagens através da API de mensageria Benner. 
Esta API foi concebida para simplificar o máximo possível a utilização de diversos serviços de mensageria de forma genérica em um único lugar, dando comportamentos semelhantes para todas elas.

# Configuração da API

## Serviços disponíveis
Até o momento a API possibilita o uso de 4 serviços diferentes, com as respectivas *classnames* a serem informadas na configuração:

|Serviço|Classname|
|--|--|
|**ActiveMQ**|ActiveMQConfig|
|**Amazon SQS**|AmazonSqsConfig|
|**Azure Queue**|AzureMQConfig|
|**RabbitMQ**|RabbitMQConfig|

Obs.: Reforçando que o atributo "type" do broker deve ser um nome de assembly completo, por exemplo *Benner.Tecnologia.Messaging.ActiveMQConfig, Benner.Tecnologia.Messaging*.

### Nomes de filas
Existem algumas regras para a criação e utilização de filas ao que diz respeito ao seu nome:

 1. Um nome de fila deve começar com uma letra ou número, e pode conter apenas letras, números e hífen (-).
 2. A primeira e última letras devem ser alfanuméricos. O hífen (-) não pode ser o primeiro nem último caractere. Hífens consecutivos também não são permitidos.
 3. Todas as letras no nome devem ser minúsculas.
 4. O nome deve possuir de 3 a 63 caracteres.

Estas regras são validadas na configuração em memória e por arquivo (a seguir).

## Configuração por arquivo
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

## Configuração em memória
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

## Propriedades dos serviços

### ActiveMQConfig
|Nome|Descrição|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o contêiner está rodando.|
|Username|O nome de usuário para login no serviço. Padrão: admin.|
|Password|A senha para login no serviço. Padrão: admin.|
|Port|O número da porta do host do serviço. Padrão: 61616.|

### AmazonSqsConfig
O serviço da amazon possui uma particularidade quanto ao modo de estabelecer uma conexão. Para mais informações: [Configurando credenciais AWS (em inglês)](https://docs.aws.amazon.com/sdk-for-net/v2/developer-guide/net-dg-config-creds.html)

|Nome|Descrição|
|--|--|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficará invisível para outros consumidores até ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica visível novamente na fila, disponível para outros consumidores.|

### AzureMQConfig
|Nome|Descrição|
|--|--|
|ConnectionString|A string de conexão fornecida pelo serviço para estabelecer a conexão|
|InvisibilityTime|O tempo em minutos que a mensagem recuperada ficará invisível para outros consumidores até ser consumida com sucesso e deletada. Caso o processamento demore mais para retornar do que o tempo de invisibilidade, a mensagem fica visível novamente na fila, disponível para outros consumidores.|

### RabbitMQConfig
|Nome|Descrição|
|--|--|
|Hostname|O nome do host, por exemplo o servidor que o contêiner está rodando.|
|Username|O nome de usuário para login no serviço. Padrão: guest.|
|Password|A senha para login no serviço. Padrão: guest.|
|Port|O número da porta do host do serviço. Padrão: 5672.|

# A classe *Client*
A principal classe para uso da API é a *Benner.Tecnologia.Messaging.Client*. Ela possui os métodos para envio e recebimento de mensagens, e pode ser usada como estática ou instância. 


## Como instância

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

## Como estática

|Método|Descrição|
|--|--|
|DequeueSingleMessage(string, IMessagingConfig)|Recebe a próxima mensagem da fila, retornando-a em *string*.|
|DequeueSingleMessage<T>(string, IMessagingConfig)|Recebe a próxima mensagem da fila e a desserializa para um objeto do tipo ***T***. Caso ocorra um erro de desserialização a mensagem é perdida.|
|EnqueueSingleMessage(string, object, IMessagingConfig)|Enfileira uma mensagem *string* na fila informada|
|EnqueueSingleMessage(string, string, IMessagingConfig)|Enfileira uma mensagem de um *object* (serializado) na fila informada|

Em todos estes métodos,  o parâmetro de configuração é opcional (default sendo o construtor padrão de *FileMessagingConfig*), todavia é recomendado informar a configuração.

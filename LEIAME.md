[<img align="right" src="https://emojipedia-us.s3.dualstack.us-west-1.amazonaws.com/thumbs/160/google/146/flag-for-united-states_1f1fa-1f1f8.png" width="35x"/>](https://github.com/benner-sistemas/messaging/blob/master/README.md)

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

```csharp
Messaging.Enqueue("nome-da-fila", "ol� mundo!");
var mensagem = Messaging.Dequeue("nome-da-fila");
```

Sim, voc� entendeu certo! Voc� programa apenas uma vez, de modo muito simples, e utiliza o c�digo em qualquer servi�o de mensageria. Sem amarras tecnol�gicas, sem refatora��o de c�digo.
Toda magia se baseia em configura��o (de arquivo ou em c�digo).

## O comportamento importa

� importante notar que a API nasceu para funcionar de um modo espec�fico:
* Primeiro a entrar, primeiro a sair
* Um produtor envia
* Apenas um consumidor recebe
* Se o consumo falhar no recebimento ou processamento, a mensagem retorna para a fila (Em progresso)
* A mensagem n�o ser� perdida (Em progresso)

As opera��es de enfileirar e desenfileirar foram desenhadas para assegurar que uma mensagem enviada pelo _produtor_ chegue no _receptor_. 
Isso significa que seguimos a abordagem "_Publicador confirma_" e "_Consumidor reconhece_" em todos os servi�os suportados. (***MENTIRA?***)

## Como come�ar

### Enviando mensagem

Crie um novo projeto:
```shell
dotnet new console -n producer
cd producer
```

Adicione Benner.Messaging e abra o projeto no VSCode:
```shell
dotnet add package benner.messaging
code .
```

Adicione _using_ e envie uma �nica mensagem para alguma fila:
```csharp
using Benner.Messaging;
Messaging.Enqueue("nome-da-fila", "ol� mundo!");
```

� isso! Execute `dotnet run` e voc� obter�:
```shell
	Messaging config not found
```

### Configura��o dos servi�os

Voc� vai precisar de um arquivo `messaging.config` como a seguir:
```xml
<MessagingConfigSection>
   <!-- Benner.Messaging � multi-broker, escolha o default -->
   <brokerList default="RabbitMQ">
      <broker name="AzureQueue" type="Benner.Messaging.AzureQueueConfig, Benner.Messaging">
         <add key="ConnectionString" value="DefaultEndpointsProtocol=https;AccountName=accountName;AccountKey=accountKey;EndpointSuffix=core.windows.net" />
         <add key="InvisibilityTime" value="15" />
      </broker>
      <broker name="RabbitMQ" type="Benner.Messaging.RabbitMQConfig, Benner.Messaging">
         <add key="Hostname" value="nome-servidor" />
         <add key="Username" value="nome-usuario" />
         <add key="Password" value="senha" />
         <add key="Port" value="porta" />
      </broker>
      <broker name="AmazonSQS" type="Benner.Messaging.AmazonSQSConfig, Benner.Messaging">
         <add key="InvisibilityTime" value="15" />
		 <add key="AccessKeyId" value="idChaveDeAcesso" />
         <add key="SecretAccessKey" value="chaveDeAcessoSecreta" />
      </broker>
      <broker name="ActiveMQ" type="Benner.Messaging.ActiveMQConfig, Benner.Messaging">
         <add key="Hostname" value="servername" />
         <add key="Username" value="nome-usuario" />
         <add key="Password" value="password" />
         <add key="Port" value="porta" />
      </broker>
   </brokerList>
   <!-- especifique apenas filas que usar�o algum broker que n�o o default -->
   <queues>
      <queue name="queue-name" broker="nome-do-broker" />
   </queues>
</MessagingConfigSection>
```

Voc� pode, como alternativa, injetar a configura��o a partir de c�digo:
```csharp
var config = new MessagingConfigBuilder()
    .WithRabbitMQBroker("RabbitMQ", "servername", setAsDefault: true)
    .Create();

Messaging.Enqueue("nome-da-fila", "ol� mundo!", config);
```

� isso! Execute `dotnet run` e voc� obter�:
```shell
	Unable to connect to RabbitMQ server
```

### Providenciando um servi�o

Voc� precisa, nesse caso, de um RabbitMQ em funcionamento.

Vamos usar um cont�iner Docker para nos ajudar. Podemos executar o seguinte comando:
```shell
docker run -d -v rabbitmq_data:/var/lib/rabbitmq -p 15672:15672 -p 15671:15671 -p 5672:5672 -p 5671:5671 rabbitmq:3.7-management
```

Tudo pronto! Execute `dotnet run` novamente. Sem erros? �timo, e agora?

### Acesse o gerenciador do seu RabbitMQ

Em seu navegador, acesse `http://servername:15672/#/queues` (usu�rio guest, senha guest) para gerenciar o servi�o. Aqui voc� pode ver suas filas com suas mensagens dentro.

### Recebendo uma mensagem
Crie um novo projeto:
```shell
dotnet new console -n consumer
cd consumer
```

Adicione Benner.Messaging e abra o projeto no VSCode:
```shell
dotnet add package benner.messaging
code .
```

Adicione _using_ e receba uma mensagem da fila, sem esquecer da configura��o:
```csharp
using Benner.Messaging;

var config = new MessagingConfigBuilder()
    .WithRabbitMQBroker("RabbitMQ", "servername", setAsDefault: true)
    .Create();

var mensagem = Messaging.Dequeue("nome-da-fila", config);
Console.Write(mensagem);
```

� isso! Execute `dotnet run` e voc� receber�:
```shell
ol� mundo! 
```

### Agora vamos fazer isso insanamente

Mude o c�digo do consumidor para:
```csharp
using Benner.Messaging;

var config = new MessagingConfigBuilder()
    .WithRabbitMQBroker("RabbitMQ", "servername", setAsDefault: true)
    .Create();

using (var client = new Messaging(config))
{
    client.StartListening("queue-name", (e) =>
    {
        // Print the message
        Console.WriteLine(e.AsString);
        return true;
    });
    // Stand-by the application so it can keep listening
    Console.ReadKey();
}
```

Execute `dotnet run` em 3 terminais diferentes e voc� ter� 3 consumidores simult�neos.

Mude o c�digo do produtor para:
```csharp
using Benner.Messaging;

var config = new MessagingConfigBuilder()
    .WithRabbitMQBroker("RabbitMQ", "servername", setAsDefault: true)
    .Create();

// Create new instance of messaging
using (var client = new Messaging(config))
{
    // Sending 1000 messages
    for (int i = 1; i <= 1000; i++)
        client.EnqueueMessage("queue-name", "hello world #" + i);
}
```

Execute `dotnet run` e voc� ver� o que acontece.

### Pr�ximos passos

Veja os exemplos dos passos iniciais [aqui](samples).

Ous, veja mais detalhes [aqui](DETALHES.md).
# Benner.Producer

Documentação referente a produção de um container linux para o Benner.Consumer.Core

## O que você precisa?

- Uma máquina com o Docker hub rodando para containers Linux ou uma máquina Linux com Docker rodando.
- Um container ou servidor rodando o RabbitMQ

# Passo a passo:

1 - Realizar o Publish do Benner.Consumer.Core

2 - Abrir a pasta onde os arquivos do Publish foram gerados

4 - Criar um Dockerfile NA MESMA PASTA que se encontram os arquivos gerados do Publish com o seguinte conteúdo:

```csharp
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app

COPY . /app/out
COPY . /app
COPY . /app/bin/Release/netcoreapp2.2

ENTRYPOINT ["dotnet", "Benner.Consumer.Core.dll"]
```

5 - Rodar o seguinte comando para buildar a imagem Docker:
```shell
docker build -t consumercore .
```

6 - Após o térimo do Build, rodar o seguinte comando para iniciar o docker, passando por parametro os dados utilizados:
```shell

docker run consumercore listen broker -h hostname --port porta -u user -p password

```
Exemplo: 
```shell
docker run consumercore listen rabbit -h tec-rabbit --port 5672 -u guest -p guest -n Benner.Consumer.Core.ContabilizacaoConsumer

```

Pronto! Para testar se funcionou, envie uma mensagem para a uma fila e configure o cunsumer para consumir as mensagem desta mesma fila e verifique se a mensagem realmente foi consumida.
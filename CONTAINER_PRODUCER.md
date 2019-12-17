# Benner.Producer

Documentação referente a produção de um container linux para o Benner.Producer

## O que você precisa?

Uma máquina com o Docker hub rodando para containers Linux ou uma máquina Linux com Docker rodando.

# Passo a passo:

1 - Realizar o Publish do Benner.Producer

2 - Abrir a pasta onde os arquivos do Publish foram gerados

3 - Altere o messaging.config gerado conforme está configurado o seu rabbitMq (ou qualquer outro meio de mensageria)
Obs: Se o rabbitMQ estiver em outro container na mesma máquina, utilizar o nome do container do RabbitMQ como Hostname.

4 - Criar um Dockerfile NA MESMA PASTA que se encontram os arquivos gerados do Publish com o seguinte conteúdo:

```csharp
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app

COPY . /app/out
COPY . /app
COPY . /app/bin/Release/netcoreapp2.2

ENTRYPOINT ["dotnet", "Benner.Producer.dll"]
```

5 - Rodar o seguinte comando para buildar a imagem Docker:
```shell
docker build -t producercore .
```

6 - Após o térimo do Build, rodar o seguinte comando para inicioar o docker: (Se a porta 5004 já estiver sendo utilizada, trocar por outra qualquer.)
```shell
docker run -p 5004:80 producercore
```

Pronto! Agora é só testar. Vá até o seu navegador e digite a seguinte URL, onde maquina é a máquina que subiu o container do producer:

maquina:5004

Se tudo deu certo, o swagger irá abrir.

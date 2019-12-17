# Benner.Producer

Documenta��o referente a produ��o de um container linux para o Benner.Producer

## O que voc� precisa?

Uma m�quina com o Docker hub rodando para containers Linux ou uma m�quina Linux com Docker rodando.

## Passo a passo:

1 - Realizar o Publish do Benner.Producer
2 - Abrir a pasta onde os arquivos do Publish foram gerados
3 - Criar um Dockerfile NA MESMA PASTA que se encontram os arquivos gerados do Publish com o seguinte conte�do:

```csharp
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app

COPY . /app/out
COPY . /app
COPY . /app/bin/Release/netcoreapp2.2

ENTRYPOINT ["dotnet", "Benner.Producer.dll"]
```

4 - Rodar o seguinte comando para buildar a imagem Docker:
```shell
docker build -t producercore .
```

5 - Ap�s o t�rimo do Build, rodar o seguinte comando para inicioar o docker: (Se a porta 5004 j� estiver sendo utilizada, trocar por outra qualquer.)
```shell
docker run -p 5004:80 producercore
```

Pronto! Agora � s� testar. V� at� o seu navegador e digite a seguinte URL, onde maquina � a m�quina que subiu o container do producer:

maquina:5004

Se tudo deu certo, o swagger ir� abrir.
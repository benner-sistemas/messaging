# Benner.Producer

Documentação referente à construção de um contêiner linux para o Benner.Producer.

## O que você precisa?

- Uma máquina com Docker rodando contêineres Linux.
- Um contêiner ou servidor de mensageria (exemplo: RabbitMQ).

# Passo a passo:

1. Realizar o Publish do Benner.Producer para uma pasta.
   1. Navegue até a pasta do `Benner.Producer.csproj` via linha de comando, e execute `dotnet publish`.
2. Na pasta publicada, altere o arquivo `messaging.config` para [configurar seus brokers de mensageria.](https://github.com/benner-sistemas/messaging/blob/master/LEIAME.md#configura%C3%A7%C3%A3o-dos-servi%C3%A7os) **É fortemente recomendado** utilizar o IP do servidor nas configurações de hostname.

3. Criar um arquivo de texto com nome "dockerfile" dentro da pasta `publish` gerada, com o seguinte conteúdo:

```dockerfile
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY . /app
ENTRYPOINT ["dotnet", "Benner.Producer.dll"]
```

4. Para construir a imagem Docker navegue via linha de comando até a pasta "publish" e execute:
```shell
docker build -t producercore .
```

5. Para rodar o contêiner a partir da imagem construída, publicando uma porta local, por exemplo 5004, para o contêiner, execute:
```shell
docker run -p 5004:80 producercore
```

Pronto! Agora é só testar. Vá até o seu navegador e acesse `maquina:porta`, onde `maquina` é o nome da máquina que rodou o contêiner, e `porta` a publicada, como por exemplo `localhost:5004`.

Se tudo estiver correto, você verá a tela do swagger.

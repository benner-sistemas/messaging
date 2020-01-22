
# Benner.Producer
Documentação referente à construção de um contêiner linux para o Benner.Producer.

## O que você precisa?
- Uma máquina com Docker rodando contêineres Linux.
- Um contêiner ou servidor de mensageria (exemplo: RabbitMQ).

# Passo a passo:
 1. Primeiro é necessário criar arquivos para a configuração do producer. Em um novo diretório, crie os seguintes arquivos, já os configurando:
    - [**messaging.config**](LEIAME.md#configuração-dos-serviços)
    - [**producer.json**](CONFIGURATION_FILES.md#producerjson)
   2. Copie suas dlls de controllers para o diretório, com suas respectivas dependências.
   3. Crie um arquivo de texto com nome "dockerfile" com o seguinte conteúdo:
```dockerfile
FROM bennersistemas/producer:1.0.0
COPY . /app
```
 4. Para construir a imagem Docker navegue via linha de comando até seu diretório e execute:
```shell
docker build -t [nome da imagem] .
```
 5. Para rodar o contêiner a partir da imagem construída, publicando uma porta local, por exemplo 5004, para o contêiner, execute:
```shell
docker run -p 5004:80 [nome da imagem]
```

Pronto! Agora é só testar. Vá até o seu navegador e acesse `maquina:porta`, onde `maquina` é o nome da máquina que rodou o contêiner, e `porta` a publicada, como por exemplo `localhost:5004`.

Se tudo estiver correto, você verá a tela do swagger.

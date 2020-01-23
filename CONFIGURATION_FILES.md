# Configurações de Producer e Consumer
Existem 3 arquivos totais obrigatórios para configurar o Benner.Producer e Benner.Consumer.Core, sendo eles:
 - messaging.config
 - producer.json
 - consumer.json
 
 O arquivo messaging.config deve estar presente e configurado no diretório do producer e do consumer. Já os arquivos json devem estar presentes apenas em seus respectivos diretórios.

### Messaging.config
Este arquivo serve para as configurações de broker para o qual o Benner.Producer enviará as mensagens e de onde Benner.Consumer.Core escutará. [Clique aqui](LEIAME.md#configuração-dos-serviços) para detalhes de como configurá-lo.

## Benner.Producer

###  Producer.json
Este arquivo serve para configurar a lista de controllers que devem ser carregados e os *endpoints* do servidor de autenticação Open Id Connect, responsável pela segurança do envio de requisições para o servidor de mensageria.
Sua estrutura é básica:
```json
{
  "Controllers": [
    "Controller1.dll",
    "Controller2.dll",
    "Controller3.dll"
  ],
  "Oidc": {
    "ClientId": "client_id",
    "ClientSecret": "client_secret",
    "TokenEndpoint": "http://dominio.com.br/auth/realms/master/protocol/openid-connect/token",
    "UserInfoEndpoint" : "http://dominio.com.br/auth/realms/master/protocol/openid-connect/userinfo"
  }
}
```

#### Controllers
Uma lista de nomes dos assemblies que devem ser carregados dinamicamente pelo Benner.Producer. 
Pode ser vazia ou conter N nomes. 
 - Caso seja vazia, serão procurados e carregados todos os assemblies com sufixo "*.Producer.dll" no diretório de trabalho.
 - Caso possua itens, estes serão procurados no diretório de trabalho e carregados.

#### Oidc
 - ClientId: Identificador do aplicativo, deve ser cadastrado no servidor de identidade.
 - ClientSecret: Chave de acesso do aplicativo, deve ser gerada ao cadastrar o aplicativo no servidor de identidade.
 - TokenEndpoint: Url do serviço de token (access_token).
 - UserInfoEndpoint: Url do serviço de informação de usuários (id_token).

## Benner.Consumer.Core

### Consumer.json
Este arquivo serve para configurar qual a classe responsável pelo consumo de mensagens recebidas, e tem a estrutura ainda mais simples:

```json
{
  "Consumer": "ERP.Consumer.PessoaConsumer"
}
```
O nome da classe informada para `Consumer` deve ser um nome de classe completo, com ou sem assembly. Os dois formatos são válidos.
 - Nome completo: `Namespace1.Namespace2.NomeDaClasse`.
 - Nome com assembly: `Namespace1.Namespace2.NomeDaClasse, Namespace1.Namespace2`.

 Se o arquivo não for encontrado ou o item Consumer for vazio, Benner.Consumer.Core informará um erro e não iniciará.

# Configuração de logs
A API de log tem por padrão o log configurado para console, sendo possível também enviar os logs para o ElasticSearch.
Basta configurar o arquivo `elasticsearch.json` no diretório do producer ou consumer, que possui a seguinte estrutura:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Elasticsearch",
        "Args": {
          "nodeUris": "http://maquina.benner.com.br:9200",
          "autoRegisterTemplate": true,
          "autoRegisterTemplateVersion": "ESv6"
        }
      }
    ]
  }
}
```
Em "nodeUris" é possível configurar um ou mais endereços de servidores, separados por ponto-e-vírgula (`;`), sem espaços.
É possível também adicionar mais configurações ao ElasticSearch na [documentação oficial da API](https://github.com/serilog/serilog-sinks-elasticsearch#json-appsettingsjson-configuration).
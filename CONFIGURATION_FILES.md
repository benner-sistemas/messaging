# Configura��o do Benner.Producer
Existem 3 arquivos totais necess�rios para configurar o producer e o consumer, sendo eles:
 - messaging.config
 - producer.json
 - consumer.json
 
 O arquivo messaging.config deve estar presente e configurado no diret�rio do producer e do consumer. J� os arquivos json devem estar presentes apenas em seus respectivos diret�rios.
 
## Messaging.config
Este arquivo serve para as configura��es de broker para o qual o producer enviar� as mensagens. [Clique aqui](LEIAME.md#configura��o-dos-servi�os) para detalhes de como configur�-lo.

##  Producer.json
Este arquivo serve para configurar a lista de controllers que devem ser carregados e os *endpoints* do servidor de autentica��o Open Id Connect, respons�vel pela seguran�a do envio de requisi��es para o servidor de mensageria.
Sua estrutura � b�sica:
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

### Controllers
Uma lista de nomes dos assemblies que devem ser carregados dinamicamente pelo Benner.Producer. 
Pode ser vazia ou conter N nomes. 
 - Caso seja vazia, ser�o procurados e carregados todos os assemblies com sufixo "*.Producer.dll" no diret�rio de trabalho.
 - Caso possua itens, estes ser�o procurados no diret�rio de trabalho e carregados.

### Oidc
 - ClientId: Identificador do aplicativo, deve ser cadastrado no servidor de identidade.
 - ClientSecret: Chave de acesso do aplicativo, deve ser gerada ao cadastrar o aplicativo no servidor de identidade.
 - TokenEndpoint: Url do servi�o de token (access_token).
 - UserInfoEndpoint: Url do servi�o de informa��o de usu�rios (id_token).

## Consumer.json
Este arquivo serve para configurar qual a classe respons�vel pelo consumo de mensagens recebidas, e tem a estrutura ainda mais simples:

```json
{
  "Consumer": "ERP.Consumer.PessoaConsumer"
}
```
O nome da classe informada para `Consumer` deve ser um nome de classe completo, com ou sem assembly. Os dois formatos s�o v�lidos.
 - Nome completo: `Namespace1.Namespace2.NomeDaClasse`.
 - Nome com assembly: `Namespace1.Namespace2.NomeDaClasse, Namespace1.Namespace2`.

 Se o arquivo n�o for encontrado ou o item Consumer for vazio, Benner.Consumer.Core informar� um erro e n�o iniciar�.
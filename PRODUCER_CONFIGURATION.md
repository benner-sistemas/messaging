# Configura��o do Benner.Producer
Existem 2 arquivos necess�rios para configurar um producer:
 - messaging.config
 - producer.json
 
 Ambos devem estar presentes no mesmo diret�rio que o Benner.Producer a ser executado.
 
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
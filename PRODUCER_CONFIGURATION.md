# Configuração do Benner.Producer
Existem 2 arquivos necessários para configurar um producer:
 - messaging.config
 - producer.json
 
 Ambos devem estar presentes no mesmo diretório que o Benner.Producer a ser executado.
 
## Messaging.config
Este arquivo serve para as configurações de broker para o qual o producer enviará as mensagens. [Clique aqui](LEIAME.md#configuração-dos-serviços) para detalhes de como configurá-lo.

##  Producer.json
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

### Controllers

Uma lista de nomes dos assemblies que devem ser carregados dinamicamente pelo Benner.Producer. 
Pode ser vazia ou conter N nomes. 
 - Caso seja vazia, serão procurados e carregados todos os assemblies com sufixo "*.Producer.dll" no diretório de trabalho.
 - Caso possua itens, estes serão procurados no diretório de trabalho e carregados.

### Oidc
 - ClientId: Identificador do aplicativo, deve ser cadastrado no servidor de identidade.
 - ClientSecret: Chave de acesso do aplicativo, deve ser gerada ao cadastrar o aplicativo no servidor de identidade.
 - TokenEndpoint: Url do serviço de token (access_token).
 - UserInfoEndpoint: Url do serviço de informação de usuários (id_token).
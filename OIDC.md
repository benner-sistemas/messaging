# Benner.Producer

Documentação referente a autenticação do Producer com OIDC

## O que você precisa?

- 
- 

# Passo a passo:

Acesse em seu navegador o gerenciador do KeyCloak. Entre em Administration Console para acessar a tela de login, utilizando os dados previamente configurados na instalação  do keycloak. Você será redirecionado para o Realm Master.

No menu lateral esquerdo, acesse Clients, e então no canto direito clique em `Create` para criar um novo client a ser usado na autenticação do Producer. Preencha o Client ID, por exemplo `producer-api`, e então salve. Você será redirecionado para a página completa de configuração.

Nesta página precisaremos seguir alguns passos:

Altere o Access Type para confidential.
Habilite as opções:

Standard Flow Enabled;
Implicit Flow Enabled;
Direct Access Grants Enabled;
Service Accounts Enabled;
Authorization Enabled;

Salve as configurações.


No menu lateral esquerdo, acesse Roles, e então no canto direito clique em `Add Role` para criar uma nova role. Preencha o Role Name, por exemplo `acesso-producer`, e então salve. Você será redirecionado para a página completa de configuração, porém nenhuma configuração adicional é necessária.

No menu lateral esquerdo, acesse Users, e então no canto direito clique em `Add user` para criar um novo usuário. Preencha o Username com o nome do usuário, por exemplo `usuario-fulano`, e então salve. Você será redirecionado para a página completa de configuração, onde deve seguir os seguintes passos:

Na aba `Credentials`, adicione uma senha e a confiramção da mesma nos campos `Password` e `Password Confirmation`, respectivamente;
Na aba `Role Mappings`, selecione a rola criada anteriormente (`acesso-producer`) dentro do quadro Available Roles, e clique em `Add selectec >`

Estas são as configurações necessárias no Keycloak, agora você está pronto para realizar autenticação no Producer! 





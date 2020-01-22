# Benner.Producer

Documentação referente a autenticação do Producer com OIDC

# Passo a passo da configuração do Keycloak:

Acesse em seu navegador o gerenciador do KeyCloak. Entre em Administration Console para acessar a tela de login, utilizando os dados previamente configurados na instalação  do keycloak. Você será redirecionado para o Realm Master.

No menu lateral esquerdo, acesse Clients, e então no canto direito clique em `Create` para criar um novo client a ser usado na autenticação do Producer. Preencha o Client ID, por exemplo `producer-api`, e então salve. Você será redirecionado para a página completa de configuração.

Nesta página precisaremos seguir alguns passos:

Altere o Access Type para confidential.
Habilite as opções:
```
Standard Flow Enabled;
Implicit Flow Enabled;
Direct Access Grants Enabled;
Service Accounts Enabled;
Authorization Enabled;
```
Salve as configurações.


No menu lateral esquerdo, acesse Roles, e então no canto direito clique em `Add Role` para criar uma nova role. Preencha o Role Name, por exemplo `acesso-producer`, e então salve. Você será redirecionado para a página completa de configuração, porém nenhuma configuração adicional é necessária.

No menu lateral esquerdo, acesse Users, e então no canto direito clique em `Add user` para criar um novo usuário. Preencha o Username com o nome do usuário, por exemplo `usuario-fulano`, e então salve. Você será redirecionado para a página completa de configuração, onde deve seguir os seguintes passos:

Na aba `Credentials`, adicione uma senha e a confiramção da mesma nos campos `Password` e `Password Confirmation`, respectivamente;
Na aba `Role Mappings`, selecione a rola criada anteriormente (`acesso-producer`) dentro do quadro Available Roles, e clique em `Add selectec >` 

# Passo a passo da configuração no código:

Obs: Para demonstração, utilizaremos uma classe de teste e uma API de pessoas previamente criada.

Dentro da classe de testes, é necessário instanciar um Client para realizar a requisição do token de autenticação no Keycloak e um Client também para realizar requisições Http.
```csharp
        private readonly HttpClient _client;
        private readonly HttpClient _authClient = new HttpClient();
```
Também criaremos uma classe estática chamada `ContentHelper`, que nos ajudará a converter objetos anônimos. 
```csharp
 public static class ContentHelper
    {
        public static StringContent GetStringContent(object obj)
            => new StringContent(JsonConvert.SerializeObject(obj), Encoding.Default, "application/json");
    }
```

No construtor da classe (chamada PessoasAPITest), temos o seguinte código para inicializar o client de requições http:

```csharp
public PessoasAPITest()
        {
            var server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>());
            _client = server.CreateClient();
        }
```

Para realizar o teste de requisição do token, temos o seguinte código, onde criamos um request passando a Url da API e um Body contendo os dados requisitados na API de pessoas, e temos também as informações previamente configuradas no Keycloak. Estas informações podem ser obtidas no painel de aministração do Keycloak. No menu lateral esquerdo, selecione a opção `Realm Settings`, e na aba `General`, clique em cima do texto `OpenID Endpoint Configuration`. Será aberto um uma nova aba um arquivo Json, onde podemos encontrar o Token Endpoint. Precisamos das credenciais do usuário que foi adicionado anteriormente nas configurações do keycloak, onde username será o `usuario-fulano` e a senha será a que você definiu. P ClientId também foi definido previamente, e neste caso seria `producer-api`. ClientSecret pode ser encontrado no painel de administração do Keycloak também. No menu esquerdo, selecione `Clients` e depois abra a aba `Credentials`. Selecione o texto que está na caixa ao lado de `Secret`.  Abaixo segue um modelo de como serão usadas estas informações no código:

```csharp
[Theory]
        [InlineData("POST")]
        public async Task PessoasPostTestAsyncComTokenValido(string method)
        {
            var request = new
            {
                Url = "/api/pessoas",
                Body = new
                {
                    RequestID = Guid.NewGuid(),
                    CPF = "123.567.901-34",
                    Nome = "Nome da Pessoa da Silva",
                    Nascimento = new DateTime(1983, 3, 30),
                    Endereco = new
                    {
                        Logradouro = "Rua Itajaí",
                        Numero = 881,
                        CEP = "12345-789",
                        Bairro = "Centro",
                        Municipio = "Blumenau",
                        Estado = "Santa Catarina",
                    },
                },
            };
            // Arrange
            var username = "usuario-fulano";
            var password = "suasenha";
            var tokenEndPoint = "http://seu-dominio:sua-porta/auth/realms/master/protocol/openid-connect/token";
            var clientId = "producer-api";
            var clientSecret = "54883694-162b3-9377-a1bc-a5b3cafe223d";

            var passwordRequest = new PasswordTokenRequest
            {
                Address = tokenEndPoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                UserName = username,
                Password = password,
                Scope = "openid profile email updated_at groups",
            };


            // recuperar o token soliticado
            var passwordResponse = _authClient.RequestPasswordTokenAsync(passwordRequest).Result;

            Assert.NotNull(passwordResponse);
            Assert.False(passwordResponse.IsError);
            Assert.False(string.IsNullOrEmpty(passwordResponse.AccessToken));

            _client.SetBearerToken(passwordResponse.AccessToken);

            var httpResponse = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.True(httpResponse.IsSuccessStatusCode);
        }
```
Neste teste, o `_authClient` é responsável por recuperar o token do keycloak, que futuramente é utilizado na requisição http. Dentro do producer ele faz a validação deste token recuperado.

Estas são as configurações necessárias, agora você está pronto para utlizar o Producer com autenticação OIDC!

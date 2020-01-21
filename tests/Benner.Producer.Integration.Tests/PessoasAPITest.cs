using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Text;
using IdentityModel.Client;
using System.Net.Http.Headers;

namespace Benner.Producer.Integration.Tests
{
    public static class ContentHelper
    {
        public static StringContent GetStringContent(object obj)
            => new StringContent(JsonConvert.SerializeObject(obj), Encoding.Default, "application/json");
    }

    public class PessoasAPITest
    {
        private readonly HttpClient _client;
        private readonly HttpClient _authClient = new HttpClient();

        public PessoasAPITest()
        {
            var server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>());
            _client = server.CreateClient();
        }

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
            var username = "usuario.123";
            var password = "benner";
            var tokenAddress = "http://bnu-vtec012:7600/auth/realms/master/protocol/openid-connect/token";
            var clientId = "producer-api";
            var clientSecret = "54835680-02b3-4477-a5bc-a5b3cafe223d";

            var passwordRequest = new PasswordTokenRequest
            {
                Address = tokenAddress,
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


        [Theory]
        [InlineData("POST")]
        public async Task PessoasPostTestAsyncComTokenInvalido(string method)
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

            _client.SetBearerToken("token invalido");

            var httpResponse = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.False(httpResponse.IsSuccessStatusCode);
            Assert.True(httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
            Assert.Equal(
                "{\"success\":false,\"message\":\"Unauthorized\"}",
                httpResponse.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData("POST")]
        public async Task PessoasPostTestAsyncComBasicAuthentication(string method)
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

            

            _client.SetBasicAuthentication("frida","fritz");

            var httpResponse = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.False(httpResponse.IsSuccessStatusCode);
            Assert.True(httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
            Assert.Equal(
                "{\"success\":false,\"message\":\"Authorization header with Bearer scheme not found\"}",
                httpResponse.Content.ReadAsStringAsync().Result);
        }

        [Theory]
        [InlineData("POST")]
        public async Task PessoasPostTestAsyncSemToken(string method)
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

            var httpResponse = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.False(httpResponse.IsSuccessStatusCode);
            Assert.True(httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
            Assert.Equal(
                "{\"success\":false,\"message\":\"Authorization header not found\"}",
                httpResponse.Content.ReadAsStringAsync().Result);
        }
    }
}
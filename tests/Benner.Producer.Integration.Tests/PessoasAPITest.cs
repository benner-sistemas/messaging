using Benner.ERP.API;
using Benner.Messaging.Configuration;
using Benner.Messaging.Logger;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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
        private readonly ProducerJson config = JsonConfiguration.LoadConfiguration<ProducerJson>();

        public PessoasAPITest()
        {
            var server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>());
            _client = server.CreateClient();
            Log.ConfigureLog();
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


            var passwordRequest = new PasswordTokenRequest
            {
                Address = config.Oidc.TokenEndpoint,
                ClientId = config.Oidc.ClientId,
                ClientSecret = config.Oidc.ClientSecret,
                UserName = config.Oidc.Username,
                Password = config.Oidc.Password,
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



            _client.SetBasicAuthentication("frida", "fritz");

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
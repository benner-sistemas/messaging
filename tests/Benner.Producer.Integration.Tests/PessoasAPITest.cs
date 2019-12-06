using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Text;

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
        public PessoasAPITest()
        {
            var server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>());
            _client = server.CreateClient();
        }

        [Theory]
        [InlineData("POST")]
        public async Task PessoasPostTestAsync(string method)
        {
            // Arrange
            var request = new
            {
                Url = "/api/pessoas",
                Body = new
                {
                    RequestID = Guid.NewGuid(),
                    CPF = "123.567.901-34",
                    Nome = "Nome da Pessoa da Silva",
                    Nascimento = new DateTime(1983,3,30),
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

            // Act
            var httpResponse = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            // Assert
            httpResponse.EnsureSuccessStatusCode();
            Assert.True(httpResponse.StatusCode == System.Net.HttpStatusCode.OK);
            var value = await httpResponse.Content.ReadAsStringAsync();
            dynamic response = JsonConvert.DeserializeObject<dynamic>(value);

            Assert.Equal(request.Body.RequestID, new Guid(response.messageID.ToString()));
            Assert.Equal("pessoas", response.queueName.ToString());


            var createdAt = Convert.ToDateTime(response.createdAt);
            TimeSpan diff = DateTime.Now.Subtract(createdAt);
            Assert.True(diff.TotalSeconds < 20);
        }
    }
}

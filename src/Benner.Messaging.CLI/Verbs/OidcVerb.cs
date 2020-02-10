using Benner.Messaging.Configuration;
using CommandLine;
using System;

namespace Benner.Messaging.CLI.Verbs
{
    [Verb("oidc", HelpText = "Configura o OIDC do 'producer.json'")]
    public class OidcVerb : IVerb
    {
        [Option("clientId", HelpText = "O Client ID do servidor OIDC.")]
        public string ClientId { get; set; }

        [Option("clientSecret", HelpText = "O Client Secret do client do servidor OIDC.")]
        public string ClientSecret { get; set; }

        [Option("tokenEndpoint", HelpText = "A url do token do servidor OIDC.")]
        public string TokenEndpoint { get; set; }

        [Option("authorizationEndpoint", HelpText = "A url de autenticação do servidor OIDC.")]
        public string AuthorizationEndpoint { get; set; }

        [Option("userInfoEndpoint", HelpText = "A url de informações de usuário do servidor OIDC.")]
        public string UserInfoEndpoint { get; set; }

        [Option("username", HelpText = "O usuário para autenticar no servidor OIDC.")]
        public string Username { get; set; }

        [Option("password", HelpText = "A senha para autenticar no servidor OIDC.")]
        public string Password { get; set; }

        public void Configure()
        {
            var producerJson = JsonConfiguration.LoadConfiguration<ProducerJson>() ?? new ProducerJson();

            if (producerJson.Oidc == null)
                producerJson.Oidc = new OidcSettings();

            if (!string.IsNullOrWhiteSpace(this.ClientId))
                producerJson.Oidc.ClientId = this.ClientId;

            if (!string.IsNullOrWhiteSpace(this.ClientSecret))
                producerJson.Oidc.ClientSecret = this.ClientSecret;

            if (!string.IsNullOrWhiteSpace(this.AuthorizationEndpoint))
                producerJson.Oidc.AuthorizationEndpoint = this.AuthorizationEndpoint;

            if (!string.IsNullOrWhiteSpace(this.TokenEndpoint))
                producerJson.Oidc.TokenEndpoint = this.TokenEndpoint;

            if (!string.IsNullOrWhiteSpace(this.UserInfoEndpoint))
                producerJson.Oidc.UserInfoEndpoint = this.UserInfoEndpoint;

            if (!string.IsNullOrWhiteSpace(this.Username))
                producerJson.Oidc.Username = this.Username;

            if (!string.IsNullOrWhiteSpace(this.Password))
                producerJson.Oidc.Password = this.Password;

            producerJson.SaveConfigurationToFile();
        }

        public bool HasNoInformedParams()
        {
            return ClientId == null && ClientSecret == null && TokenEndpoint == null && AuthorizationEndpoint == null
                && UserInfoEndpoint == null && Username == null && Password == null;
        }
    }
}

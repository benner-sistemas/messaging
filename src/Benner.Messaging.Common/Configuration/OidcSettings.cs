using System;

namespace Benner.Messaging.Configuration
{
    public class OidcSettings 
    {
        public string TokenEndpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorizationEndpoint { get; set; }
        public string UserInfoEndpoint { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public static void Validate(OidcSettings oidc)
        {
            if (oidc == null)
                throw new Exception("A configuração de Oidc deve ser informada.");

            string msg = "{0} deve ser informado.";

            if (string.IsNullOrWhiteSpace(oidc.TokenEndpoint))
                throw new Exception(string.Format(msg, nameof(oidc.TokenEndpoint)));

            if (string.IsNullOrWhiteSpace(oidc.UserInfoEndpoint))
                throw new Exception(string.Format(msg, nameof(oidc.UserInfoEndpoint)));

            if (string.IsNullOrWhiteSpace(oidc.AuthorizationEndpoint))
                throw new Exception(string.Format(msg, nameof(oidc.AuthorizationEndpoint)));
        }
    }
}

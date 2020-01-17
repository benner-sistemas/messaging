namespace Benner.Producer.Configuration
{
    internal class OidcConfig : Configuration
    {
        protected override string FileName => "oidc.json";

        public string TokenEndpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}

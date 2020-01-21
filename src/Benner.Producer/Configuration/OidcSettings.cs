namespace Benner.Producer.Configuration
{
    public class OidcSettings 
    {
        public string TokenEndpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string UserInfoEndpoint { get; set; }
    }
}

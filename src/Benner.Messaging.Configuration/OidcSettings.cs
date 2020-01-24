namespace Benner.Messaging.Configuration
{
    public class OidcSettings 
    {
        public string TokenEndpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string UserInfoEndpoint { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

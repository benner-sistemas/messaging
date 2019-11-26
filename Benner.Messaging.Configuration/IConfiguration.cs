using Benner.Messaging;

namespace Benner.Messaging.Configuration
{
    public interface IConfiguration
    {
        string Validation();

        MessagingConfig Configuration();
    }
}

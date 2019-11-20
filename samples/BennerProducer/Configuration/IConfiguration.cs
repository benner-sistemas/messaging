using Benner.Messaging;

namespace BennerProducer.Configuration
{
    public interface IConfiguration
    {
        string Validation();

        MessagingConfig Configuration();
    }
}

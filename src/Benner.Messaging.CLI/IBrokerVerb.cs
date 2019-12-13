using Benner.Messaging.Interfaces;

namespace Benner.Messaging.CLI
{
    public interface IBrokerVerb
    {
        string BrokerName { get; }
        IMessagingConfig GetConfiguration();
    }
}

namespace Benner.Messaging
{
    internal class ErrorMessages
    {
        public const string EnqueueFailed = "Failed to enqueue message.";
        public const string InvalidInformed = "Informed {0} is not valid.";
        public const string MustBeInformed = "{0} must be informed.";
        public const string AlreadyListening = "There is already a listener being used in this context.";
        public const string UnableToConnect = "Unable to connect to {0} server.";
        public const string DefaultBrokerNotFound = "MessagingConfig requires a broker as default.";
    }
}

namespace Benner.Messaging
{
    public class QueueName
    {
        public QueueName(string queueName)
        {
            Default = DefaultQueueName(queueName);
            Invalid = InvalidQueueName(queueName);
            Dead = DeadQueueName(queueName);
            Retry = RetryQueueName(queueName);
        }

        public string Dead { get; private set; }
        public string Default { get; private set; }
        public string Invalid { get; private set; }
        public string Retry { get; private set; }

        public static string DeadQueueName(string queueName)
        {
            return $"{queueName}-dead".ToLower();
        }
        public static string DefaultQueueName(string queueName)
        {
            return $"{queueName}".ToLower();
        }
        public static string InvalidQueueName(string queueName)
        {
            return $"{queueName}-invalid".ToLower();
        }
        public static string RetryQueueName(string queueName)
        {
            return $"{queueName}-retry".ToLower();
        }
    }
}
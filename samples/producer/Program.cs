using System;
using Benner.Messaging;

namespace producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new MessagingConfigBuilder()
                .WithRabbitMQBroker("RabbitMQ", "servername", setAsDefault: true)
                .Create();

            // sample 01: enqueue single message
            // Messaging.Enqueue("queue-name", "hello world!", config);

            // sample 02: enqueue many messages
            using (var client = new Messaging(config))
            {
                for (int i = 1; i <= 1000; i++)
                    client.EnqueueMessage("my-queue", "hello world #" + i);
            }
        }
    }
}

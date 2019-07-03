using System;
using Benner.Messaging;

namespace producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = MessagingConfigFactory
                .NewMessagingConfigFactory()
                .WithRabbitMQBroker("hostname", 5672, "user", "password")
                .Create();

            Messaging.Enqueue("queue-name", "hello world!", config);

            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }
    }
}

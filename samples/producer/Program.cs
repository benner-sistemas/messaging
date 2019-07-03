using System;
using Benner.Messaging;

namespace producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new MessagingConfig()
                .AddRabbitMQBroker("hostname", 5672, "user", "password");

            Messaging.Enqueue("queue-name", "hello world!", config);
            
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }
    }
}

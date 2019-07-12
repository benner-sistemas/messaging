using System;
using Benner.Messaging;

namespace consumer
{
    class Program
    {
        static void Main(string[] args)
        {

            var config = new MessagingConfigBuilder()
                .WithRabbitMQBroker("RabbitMQ", "bnu-vtec011", setAsDefault: true)
                .Create();

            // sample 01: dequeue single message
            // var message = Messaging.Dequeue("queue-name", config);
            // Console.Write(message);


            // sample 02: keep listening
            using (var client = new Messaging(config))
            {
                client.StartListening("my-queue", (e) =>
                {
                    // Print the message
                    Console.WriteLine(e.AsString);
                    return true;
                });
                // Stand-by the application so it can keep listening
                Console.ReadKey();
            }
        }
    }
}

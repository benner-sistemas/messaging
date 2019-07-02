using System;
using System.Collections.Generic;

namespace Benner.Messaging.Tests
{
    public static class QueueMock
    {
        public static Dictionary<string, Queue<string>> Queues { get; }

        static QueueMock()
        {
            /// Comportamento de comparação replicado de <see cref="Client.Client(IMessagingConfig)"/>
            Queues = new Dictionary<string, Queue<string>>(StringComparer.OrdinalIgnoreCase);
        }

        public static void EnqueueMessage(string queueName, string message)
        {
            if (Queues.ContainsKey(queueName))
                Queues[queueName].Enqueue(message);
            else
            {
                Queues[queueName] = new Queue<string>();
                Queues[queueName].Enqueue(message);
            }
        }

        public static void ClearAllQueues()
        {
            Queues.Clear();
        }
    }
}

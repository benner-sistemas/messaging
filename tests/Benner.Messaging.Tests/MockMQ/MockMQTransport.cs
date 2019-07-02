using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Benner.Messaging.Interfaces;

namespace Benner.Messaging.Tests
{
    public class MockMQTransport : BrokerTransport
    {
        //fila, lista de actions
        static Dictionary<string, List<Func<MessagingArgs, bool>>> _consumers = new Dictionary<string, List<Func<MessagingArgs, bool>>>();
        //fila, ordenação
        private static Dictionary<string, int> _consumerIndexer = new Dictionary<string, int>();
        private static object _notifyLocker = new object();

        public MockMQTransport(MockMQConfig config)
        { }

        public override void EnqueueMessage(string queueName, string message)
        {
            QueueMock.EnqueueMessage(queueName, message);
            NotifyConsumers();
        }

        public override void StartListening(string queueName, Func<MessagingArgs, bool> func)
        {
            if (!_consumers.ContainsKey(queueName))
                _consumers[queueName] = new List<Func<MessagingArgs, bool>>() { func };
            else
                _consumers[queueName].Add(func);

            NotifyConsumers();
        }
        public override string DequeueSingleMessage(string queueName)
        {
            if (!QueueMock.Queues.ContainsKey(queueName))
                return null;

            if (QueueMock.Queues[queueName].Count == 0)
                return null;

            var messageValue = QueueMock.Queues[queueName].Dequeue();

            return messageValue;
        }

        private void NotifyConsumers()
        {
            lock (_notifyLocker)
            {
                int queuesLength = QueueMock.Queues.Count;
                for (int i = 0; i < queuesLength; i++)
                {
                    KeyValuePair<string, Queue<string>> queue = QueueMock.Queues.ElementAt(i);
                    int queueLength = queue.Value.Count;
                    for (int j = 0; j < queueLength; j++)
                    {
                        if (NotifyCurrentConsumer(queue.Key, queue.Value.ElementAt(j)))
                        {
                            queue.Value.Dequeue();
                            queueLength--;
                        }
                        else
                            break;
                    }
                }
            }
        }

        private bool NotifyCurrentConsumer(string queue, string message)
        {
            if (!_consumerIndexer.ContainsKey(queue))
                _consumerIndexer.Add(queue, 0);

            var currentConsumerIndex = _consumerIndexer[queue];

            if (_consumers.ContainsKey(queue))
            {
                if (_consumers[queue].Count > currentConsumerIndex)
                {
                    _consumers[queue][currentConsumerIndex](new MessagingArgs(Encoding.UTF8.GetBytes(message)));
                    _consumerIndexer[queue]++;
                    return true;
                }
                else if (_consumers[queue].Count == currentConsumerIndex)
                {
                    _consumers[queue][currentConsumerIndex - 1](new MessagingArgs(Encoding.UTF8.GetBytes(message)));
                    _consumerIndexer[queue] = 0;
                    return true;
                }
            }
            return false;
        }

        public override void Dispose()
        { }

        ~MockMQTransport()
        {
            Dispose();
        }
    }
}
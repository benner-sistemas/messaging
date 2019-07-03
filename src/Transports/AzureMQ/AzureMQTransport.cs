using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Benner.Messaging
{
    internal class AzureMQTransport : BrokerTransport
    {
        private readonly AzureMQConfig _config;
        private readonly CloudQueueClient _queueClient;
        private readonly Dictionary<string, CloudQueue> _declaredQueues;
        private Task _listeningTask;
        private bool _keepListenerAlive;

        public AzureMQTransport(AzureMQConfig config)
        {
            _config = config;
            _queueClient = _config.StorageAccount.CreateCloudQueueClient();
            _declaredQueues = new Dictionary<string, CloudQueue>();
            _keepListenerAlive = true;
        }

        public override void EnqueueMessage(string queueName, string message)
        {
            CloudQueue queue = GetQueue(queueName);
            var messageToSend = new CloudQueueMessage(message);
            queue.AddMessage(messageToSend);
        }

        private CloudQueue GetQueue(string queueName)
        {
            if (_declaredQueues.ContainsKey(queueName))
                return _declaredQueues[queueName];

            _declaredQueues[queueName] = _queueClient.GetQueueReference(queueName);
            try
            {
                _declaredQueues[queueName].CreateIfNotExists();
                return _declaredQueues[queueName];
            }
            catch (StorageException e)
            {
                throw new InvalidOperationException("Não foi possível se conectar ao servidor Azure.", e);
            }
        }

        public override void StartListening(string queueName, Func<MessagingArgs, bool> func)
        {
            if (_listeningTask != null)
                throw new InvalidOperationException("Escuta de fila neste contexto já está em uso");

            CloudQueue queue = GetQueue(queueName);
            _listeningTask = new Task(() => Listener(queue, func), TaskCreationOptions.LongRunning);
            _listeningTask.Start();
        }

        private void Listener(CloudQueue queue, Func<MessagingArgs, bool> func)
        {
            while (_keepListenerAlive)
            {
                if (queue.PeekMessage() != null)
                {
                    var message = queue.GetMessage(TimeSpan.FromMinutes(_config.InvisibilityTimeInMinutes));
                    if (message == null)
                        continue;

                    var acknowledge = true;
                    try
                    {
                        acknowledge = func(new MessagingArgs(message.AsBytes));
                    }
                    catch (Exception exception)
                    {
                        EnqueueMessage($"{queue.Name}-error", string.Concat(exception.Message, "\r\n", message.AsString));
                    }
                    finally
                    {
                        if (acknowledge)
                            queue.DeleteMessage(message);
                        else
                            queue.UpdateMessage(message, TimeSpan.Zero, MessageUpdateFields.Visibility);
                    }
                }
                else
                    Thread.Sleep(1000);
            }
        }

        public override void Dispose()
        {
            _keepListenerAlive = false;
            _listeningTask?.Wait();
        }

        public override string DequeueSingleMessage(string queueName)
        {
            var queue = GetQueue(queueName);

            var message = queue.GetMessage(TimeSpan.FromMinutes(_config.InvisibilityTimeInMinutes));
            if (message == null)
                return null;

            queue.DeleteMessage(message);
            return message.AsString;
        }

        ~AzureMQTransport()
        {
            Dispose();
        }
    }
}

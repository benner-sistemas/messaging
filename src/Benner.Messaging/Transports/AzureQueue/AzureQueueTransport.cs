using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Benner.Messaging
{
    internal class AzureQueueTransport : BrokerTransport
    {
        private readonly AzureQueueConfig _config;
        private readonly CloudQueueClient _queueClient;
        private readonly Dictionary<string, CloudQueue> _declaredQueues;
        private Task _listeningTask;
        private bool _keepListenerAlive;

        public AzureQueueTransport(AzureQueueConfig config)
        {
            _config = config;
            _queueClient = _config.StorageAccount.CreateCloudQueueClient();
            _declaredQueues = new Dictionary<string, CloudQueue>();
            _keepListenerAlive = true;
        }

        public override void EnqueueMessage(string queueName, string message)
        {
            CloudQueue queue = GetQueue(queueName);
            var msgToSend = new CloudQueueMessage(message);
            try
            {
                queue.AddMessage(msgToSend);

                //The CloudQueueMessage message passed in will be populated with the pop receipt, message ID, and the insertion/expiration time.
                if (msgToSend.Id == null || msgToSend.InsertionTime == null || msgToSend.ExpirationTime == null || msgToSend.PopReceipt == null)
                    throw new InvalidOperationException(ErrorMessages.EnqueueFailed);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(ErrorMessages.EnqueueFailed, e);
            }
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
                throw new InvalidOperationException(string.Format(ErrorMessages.UnableToConnect, "AzureQueue"), e);
            }
        }

        public override void StartListening(string queueName, Func<MessagingArgs, bool> func)
        {
            if (_listeningTask != null)
                throw new InvalidOperationException(ErrorMessages.AlreadyListening);

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

        public override void DequeueSingleMessage(string queueName, Func<string, bool> func)
        {
            var queue = GetQueue(queueName);

            var message = queue.GetMessage(TimeSpan.FromMinutes(_config.InvisibilityTimeInMinutes));
            if (message == null)
            {
                func(null);
                return;
            }

            bool succeeded = func(message.AsString);

            if (succeeded)
                queue.DeleteMessage(message);
            else
                queue.UpdateMessage(message, TimeSpan.Zero, MessageUpdateFields.Visibility);
        }

        ~AzureQueueTransport()
        {
            Dispose();
        }
    }
}

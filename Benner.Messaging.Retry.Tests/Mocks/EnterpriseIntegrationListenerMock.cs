using Benner.Listener;
using Benner.Messaging;
using Benner.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Benner.Retry.Tests.MockMQ
{
    public class EnterpriseIntegrationListenerMock : IDisposable
    {
        private int _countRetry;
        private readonly IEnterpriseIntegrationConsumer _consumer;
        private readonly Messaging.Messaging _receiver;
        private readonly Messaging.Messaging _sender;
        private string DefaultQueueName => _consumer.Settings.QueueName;
        private string InvalidQueueName => $"{_consumer.Settings.QueueName}-invalid";
        private string DeadQueueName => $"{_consumer.Settings.QueueName}-dead";
        private string RetryQueueName => $"{_consumer.Settings.QueueName}-retry";
        public EnterpriseIntegrationListenerMock(IMessagingConfig config, IEnterpriseIntegrationConsumer consumer)
        {
            _consumer = consumer;
            _receiver = new Messaging.Messaging(config);
            _sender = new Messaging.Messaging(config);
        }
        public void Dispose()
        {
            if (_receiver != null)
                _receiver.Dispose();

            if (_sender != null)
                _sender.Dispose();
        }
        public void Start()
        {
            _receiver.StartListening(DefaultQueueName, ProcessDefaultQueue);
            _receiver.StartListening(RetryQueueName, ProcessRetryQueue);
        }
        private bool ProcessDefaultQueue(MessagingArgs arg)
        {
            var integrationMessage = arg.GetMessage<EnterpriseIntegrationMessage>();
            return ProcessMessage(integrationMessage);
        }
        private bool ProcessRetryQueue(MessagingArgs arg)
        {
            _countRetry++;
            var integrationMessage = arg.GetMessage<EnterpriseIntegrationMessage>();
            var waitUntil = integrationMessage.WaitUntil.Subtract(DateTime.Now).TotalSeconds;
            if (waitUntil > 20)
            {
                Thread.Sleep(10);
                return false;
            }
            if (waitUntil > 0)
            {
                Thread.Sleep((int)waitUntil * 10);
            }

            return ProcessMessage(integrationMessage);
        }

        public void SetCountRetry(int v)
        {
            _countRetry = v;
        }

        private static object _processorLocker = new object();
        private bool ProcessMessage(EnterpriseIntegrationMessage integrationMessage)
        {
            lock (_processorLocker)
            {
                try
                {
                    return CallConsumeMessage(integrationMessage);
                }
                catch (InvalidMessageException invalidMessageException)
                {
                    return CallInvalidMessage(integrationMessage, invalidMessageException);
                }
                catch (Exception exception)
                {
                    return CallRetryOrDeadMessage(integrationMessage, exception);
                }
            }
        }
        private bool CallConsumeMessage(EnterpriseIntegrationMessage integrationMessage)
        {
            _consumer.OnMessage(integrationMessage.Body);
            return true;
        }
        private bool CallInvalidMessage(EnterpriseIntegrationMessage integrationMessage, InvalidMessageException invalidMessageException)
        {
            _countRetry++;
            integrationMessage.ExceptionList.Add(invalidMessageException);
            _sender.EnqueueMessage(InvalidQueueName, integrationMessage);
            Task.Run(() => { try { _consumer.OnInvalidMessage(integrationMessage.Body); } catch { /*silent!*/ } });
            return true;
        }
        private bool CallRetryOrDeadMessage(EnterpriseIntegrationMessage integrationMessage, Exception exception)
        {
            _countRetry++;
            integrationMessage.ExceptionList.Add(exception);
            integrationMessage.RetryCount = integrationMessage.RetryCount + 1;
            if (integrationMessage.RetryCount < _consumer.Settings.RetryLimit)
            {
                integrationMessage.WaitUntil = DateTime.Now.AddMilliseconds(_consumer.Settings.RetryIntervalInMilliseconds);
                _sender.EnqueueMessage(RetryQueueName, integrationMessage);
            }
            else
            {
                _sender.EnqueueMessage(DeadQueueName, integrationMessage);
                Task.Run(() => { try { _consumer.OnDeadMessage(integrationMessage.Body); } catch { /*silent!*/ } });
            }
            return true;
        }

        public int GetCountRetry()
        {
            return _countRetry;
        }

    }
}

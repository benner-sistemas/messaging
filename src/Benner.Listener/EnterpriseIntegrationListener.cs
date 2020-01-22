using Benner.Messaging;
using Benner.Messaging.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Benner.Listener
{
    public class EnterpriseIntegrationListener : IDisposable
    {
        private readonly IEnterpriseIntegrationConsumer _consumer;
        private readonly Messaging.Messaging _receiver;
        private readonly Messaging.Messaging _sender;
        private readonly QueueName _queueName;
        
        public EnterpriseIntegrationListener(IMessagingConfig config, IEnterpriseIntegrationConsumer consumer)
        {
            _consumer = consumer;
            _queueName = new QueueName(consumer.Settings.QueueName);
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
            _receiver.StartListening(_queueName.Default, ProcessDefaultQueue);
            _receiver.StartListening(_queueName.Retry, ProcessRetryQueue);
        }
        private bool ProcessDefaultQueue(MessagingArgs arg)
        {
            var integrationMessage = arg.GetMessage<EnterpriseIntegrationMessage>();
            return ProcessMessage(integrationMessage);
        }
        private bool ProcessRetryQueue(MessagingArgs arg)
        {
            var integrationMessage = arg.GetMessage<EnterpriseIntegrationMessage>();
            var waitUntil = integrationMessage.WaitUntil.Subtract(DateTime.Now).TotalSeconds;
            if (waitUntil > 20)
            {
                Thread.Sleep(2000);
                return false;
            }
            if (waitUntil > 0)
            {
                Thread.Sleep((int)waitUntil * 1000);
            }

            return ProcessMessage(integrationMessage);
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
            integrationMessage.ExceptionList.Add(invalidMessageException);
            _sender.EnqueueMessage(_queueName.Invalid, integrationMessage);
            Task.Run(() => { try { _consumer.OnInvalidMessage(integrationMessage.Body, invalidMessageException); } catch { /*silent!*/ } });
            return true;
        }
        private bool CallRetryOrDeadMessage(EnterpriseIntegrationMessage integrationMessage, Exception exception)
        {
            integrationMessage.ExceptionList.Add(exception);
            integrationMessage.RetryCount = integrationMessage.RetryCount + 1;
            if (integrationMessage.RetryCount < _consumer.Settings.RetryLimit)
            {
                integrationMessage.WaitUntil = DateTime.Now.AddMilliseconds(_consumer.Settings.RetryIntervalInMilliseconds);
                _sender.EnqueueMessage(_queueName.Retry, integrationMessage);
            }
            else
            {
                _sender.EnqueueMessage(_queueName.Dead, integrationMessage);
                Task.Run(() => { try { _consumer.OnDeadMessage(integrationMessage.Body, exception); } catch { /*silent!*/ } });
            }
            return true;
        }
    }
}
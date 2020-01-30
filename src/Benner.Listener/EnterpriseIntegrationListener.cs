using Benner.Messaging;
using Benner.Messaging.Interfaces;
using Benner.Messaging.Logger;
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
            Log.Information("Escutando fila default {default}", _queueName.Default);

            _receiver.StartListening(_queueName.Retry, ProcessRetryQueue);
            Log.Information("Escutando fila retry {retry}", _queueName.Retry);
        }
        private bool ProcessDefaultQueue(MessagingArgs arg)
        {
            var integrationMessage = arg.GetMessage<EnterpriseIntegrationMessage>();
            return ProcessMessage(integrationMessage);
        }
        private bool ProcessRetryQueue(MessagingArgs arg)
        {
            var integrationMessage = arg.GetMessage<EnterpriseIntegrationMessage>();
            var waitUntil = integrationMessage.WaitUntil.Subtract(DateTime.Now).TotalMilliseconds;
            if (waitUntil > 20_000)
            {
                Thread.Sleep(15_000);
                return false;
            }
            if (waitUntil > 0)
            {
                Thread.Sleep((int)waitUntil);
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
            var attempt = integrationMessage.RetryCount == 0 ? "default attempt" : $"retry {integrationMessage.RetryCount}";
            try
            {
                Log.Information("{id} EnterpriseIntegrationListener.OnMessage() {attempt} - begin", integrationMessage.MessageID, attempt);
                _consumer.OnMessage(integrationMessage.Body);
                return true;
            }
            finally
            {
                Log.Information("{id} EnterpriseIntegrationListener.OnMessage() {attempt} - end", integrationMessage.MessageID, attempt);
            }
        }
        private bool CallInvalidMessage(EnterpriseIntegrationMessage integrationMessage, InvalidMessageException invalidMessageException)
        {
            integrationMessage.ExceptionList.Add(invalidMessageException);
            _sender.EnqueueMessage(_queueName.Invalid, integrationMessage);
            Log.Information("{id} @ '{queueName}' mensagem inválida e foi arquivada", integrationMessage.MessageID, _queueName.Invalid);
            Task.Run(() =>
            {
                try
                {
                    Log.Information("{id} EnterpriseIntegrationListener.OnInvalidMessage() - begin", integrationMessage.MessageID);
                    _consumer.OnInvalidMessage(integrationMessage.Body, invalidMessageException);
                }
                catch (Exception e)
                {
                    Log.Error(e, e.Message);
                }
                finally
                {
                    Log.Information("{id} EnterpriseIntegrationListener.OnInvalidMessage() - end", integrationMessage.MessageID);
                }
            });
            return true;
        }
        private bool CallRetryOrDeadMessage(EnterpriseIntegrationMessage integrationMessage, Exception exception)
        {
            integrationMessage.ExceptionList.Add(exception);
            if (integrationMessage.RetryCount < _consumer.Settings.RetryLimit)
            {
                integrationMessage.RetryCount = integrationMessage.RetryCount + 1;
                integrationMessage.WaitUntil = DateTime.Now.AddMilliseconds(_consumer.Settings.RetryIntervalInMilliseconds);
                _sender.EnqueueMessage(_queueName.Retry, integrationMessage);
                Log.Information("{id} @{queueName} mensagem falhou e está em espera", integrationMessage.MessageID, _queueName.Retry);
            }
            else
            {
                _sender.EnqueueMessage(_queueName.Dead, integrationMessage);
                Log.Information("{id} @{queueName} mensagem morta e foi arquivada", integrationMessage.MessageID, _queueName.Dead);
                Task.Run(() =>
                {
                    try
                    {
                        Log.Information("{id} EnterpriseIntegrationListener.OnDeadMessage() - begin", integrationMessage.MessageID);
                        _consumer.OnDeadMessage(integrationMessage.Body, exception);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, e.Message);
                    }
                    finally
                    {
                        Log.Information("{id} EnterpriseIntegrationListener.OnDeadMessage() - end", integrationMessage.MessageID);
                    }
                });
            }
            return true;
        }
    }
}
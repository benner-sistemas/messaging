using Benner.Listener;
using Benner.Retry.Tests.MockMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Benner.Messaging.Retry.Tests
{
    public abstract class RetryTestsBase
    {
        private static QueueName _queueName = new QueueName($"{Environment.MachineName}-retryteste01");

        private static MessagingConfig _config;
        public RetryTestsBase(MessagingConfig config)
        {
            _config = config;
        }

        private static IEnterpriseIntegrationSettings _settings = new EnterpriseIntegrationSettings
        {
            QueueName = _queueName.Default,
            RetryIntervalInMilliseconds = 10,
            RetryLimit = 2,
        };

        private static EnterpriseIntegrationConsumerMock _consumer = new EnterpriseIntegrationConsumerMock(_settings);

        protected void EnterpriseIntegrationListener_deve_disparar_fluxo_de_retentativas()
        {
            CreateQueue(_queueName.Default);
            CreateQueue(_queueName.Dead);
            CreateQueue(_queueName.Retry);
            CreateQueue(_queueName.Invalid);

            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);
            PurgeQueue(_queueName.Retry);
            PurgeQueue(_queueName.Invalid);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));

            _consumer.OnMessageCount = 0;
            _consumer.OnInvalidMessageCount = 0;
            _consumer.OnDeadMessageCount = 0;
            using (var producer = new Messaging(_config))
            {
                for (int i = 0; i < 2; i++)
                {
                    producer.EnqueueMessage(_queueName.Default, new EnterpriseIntegrationMessage()
                    {
                        Body = "emitir-excecao",
                        MessageID = Guid.NewGuid().ToString()
                    });
                }
            }

            Assert.AreEqual(2, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));

            using (var listener = new EnterpriseIntegrationListener(_config, _consumer))
            {
                listener.Start();

                for (int index = 0; index < 100 && _consumer.OnDeadMessageCount < 4; ++index)
                    Thread.Sleep(10);
            }

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(2, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));

            Assert.AreEqual(4, _consumer.OnMessageCount);
            Assert.AreEqual(2, _consumer.OnDeadMessageCount);
            Assert.AreEqual(0, _consumer.OnInvalidMessageCount);

            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);
            PurgeQueue(_queueName.Retry);
            PurgeQueue(_queueName.Invalid);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));
        }

        protected void EnterpriseIntegrationListener_deve_direcionar_para_fila_de_imagens_invalidas()
        {
            CreateQueue(_queueName.Default);
            CreateQueue(_queueName.Dead);
            CreateQueue(_queueName.Retry);
            CreateQueue(_queueName.Invalid);

            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);
            PurgeQueue(_queueName.Retry);
            PurgeQueue(_queueName.Invalid);

            _consumer.OnMessageCount = 0;
            _consumer.OnInvalidMessageCount = 0;
            _consumer.OnDeadMessageCount = 0;

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));

            using (var producer = new Messaging(_config))
            {
                for (int i = 0; i < 2; i++)
                {
                    producer.EnqueueMessage(_queueName.Default, new EnterpriseIntegrationMessage()
                    {
                        Body = "emitir-excecao-mensagem-invalida",
                        MessageID = Guid.NewGuid().ToString()
                    });
                }
            }

            Assert.AreEqual(2, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));

            using (var listener = new EnterpriseIntegrationListener(_config, _consumer))
            {
                listener.Start();

                for (int index = 0; index < 100 && _consumer.OnInvalidMessageCount < 2; ++index)
                    Thread.Sleep(10);
            }

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(2, GetQueueSize(_queueName.Invalid));

            Assert.AreEqual(2, _consumer.OnMessageCount);
            Assert.AreEqual(0, _consumer.OnDeadMessageCount);
            Assert.AreEqual(2, _consumer.OnInvalidMessageCount);

            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);
            PurgeQueue(_queueName.Retry);
            PurgeQueue(_queueName.Invalid);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));
        }

        protected abstract void PurgeQueue(string queueName);

        protected abstract void CreateQueue(string queueName);

        protected abstract int GetQueueSize(string queueName);
    }
}

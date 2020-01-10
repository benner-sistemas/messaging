using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Benner.Messaging.Tests.Acknowledge
{

    public abstract class AcknowledgeTestsBase
    {
        public AcknowledgeTestsBase(MessagingConfig config)
        {
            _config = config;
        }
        private readonly QueueName _queueName = new QueueName($"{Environment.MachineName}-ackteste01");
        private readonly MessagingConfig _config;

        protected void Testa_garantia_de_recebimento_da_fila()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(_queueName.Default, message, _config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            var consumerFired = false;
            var exceptionFired = false;
            try
            {
                using (var client = new Messaging(_config))
                {
                    client.StartListening(_queueName.Default, (e) =>
                    {
                        consumerFired = true;
                        Assert.AreEqual(message, e.AsString);
                        throw new Exception(message);
                    });

                    for (int index = 0; index < 200 && !consumerFired; ++index)
                        Thread.Sleep(50);

                    Thread.Sleep(100);
                }
            }
            catch (Exception exception)
            {
                exceptionFired = true;
                Assert.AreEqual(message, exception.Message);
            }

            Assert.IsFalse(exceptionFired);
            Assert.IsTrue(consumerFired);
            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(1, GetQueueSize(_queueName.Dead));
        }

        protected void Testa_o_acknowledge_no_metodo_de_recebimento_da_fila()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(_queueName.Default, message, _config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            var consumerFired = false;
            using (var client = new Messaging(_config))
            {
                client.StartListening(_queueName.Default, (e) =>
                {
                    consumerFired = true;
                    Assert.AreEqual(message.ToString(), e.AsString);
                    return true;
                });
                for (int index = 0; index < 200 && !consumerFired; ++index)
                    Thread.Sleep(50);
            }
            Assert.IsTrue(consumerFired);
            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        protected void Testa_o_not_acknowledge_no_metodo_de_recebimento_sem_executar_o_consume_da_fila()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(_queueName.Default, message, _config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            var consumerFired = false;
            using (var client = new Messaging(_config))
            {
                try
                {
                    client.StartListening(_queueName.Default, null);
                }
                catch { /**/ }
            }

            Assert.IsFalse(consumerFired);
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        protected void Testa_o_not_acknowledge_no_metodo_de_recebimento_executando_o_consume_porem_disposeando_o_client_da_fila()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);
        
            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        
            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(_queueName.Default, message, _config);
        
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        
            var consumerFired = false;
            using (var client = new Messaging(_config))
            {
                client.StartListening(_queueName.Default, (e) =>
                {
                    Thread.Sleep(10);
                    consumerFired = true;
                    return true;
                });
            }
        
            Assert.IsFalse(consumerFired);
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        protected void Testa_o_not_acknowledge_no_metodo_de_recebimento_com_duas_mensagens_sem_executar_o_consume_da_fila()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            Messaging.Enqueue(_queueName.Default, "Message_A", _config);
            Messaging.Enqueue(_queueName.Default, "Message_B", _config);

            Assert.AreEqual(2, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));


            var client01 = new Messaging(_config);
            var client02 = new Messaging(_config);

            client01.StartListening(_queueName.Default, ProcessQueueAorB);
            client02.StartListening(_queueName.Default, ProcessQueueAorB);

            for (int index = 0; index < 200 && !(_consumerFired_A && _consumerFired_B); ++index)
                Thread.Sleep(50);

            client01.Dispose();
            client02.Dispose();

            Assert.IsTrue(_consumerFired_A);
            Assert.IsTrue(_consumerFired_B);
            Assert.AreEqual(2, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        protected void Dois_clients_simultaneos_ouvindo_filas_distintas()
        {
            var queueName_A = new QueueName(_queueName.Default + "A");
            var queueName_B = new QueueName(_queueName.Default + "B");

            PurgeQueue(queueName_A.Default);
            PurgeQueue(queueName_A.Dead);

            PurgeQueue(queueName_B.Default);
            PurgeQueue(queueName_B.Dead);

            Messaging.Enqueue(queueName_A.Default, "Message_A", _config);
            Messaging.Enqueue(queueName_B.Default, "Message_B", _config);

            Assert.AreEqual(1, GetQueueSize(queueName_A.Default));
            Assert.AreEqual(0, GetQueueSize(queueName_A.Dead));

            Assert.AreEqual(1, GetQueueSize(queueName_B.Default));
            Assert.AreEqual(0, GetQueueSize(queueName_B.Dead));

            var client01 = new Messaging(_config);
            var client02 = new Messaging(_config);

            client01.StartListening(queueName_A.Default, ProcessQueueAorB);
            client02.StartListening(queueName_B.Default, ProcessQueueAorB);

            for (int index = 0; index < 200 && !(_consumerFired_A && _consumerFired_B); ++index)
                Thread.Sleep(50);

            client01.Dispose();
            client02.Dispose();

            Assert.IsTrue(_consumerFired_A);
            Assert.IsTrue(_consumerFired_B);

            Assert.AreEqual(1, GetQueueSize(queueName_A.Default));
            Assert.AreEqual(0, GetQueueSize(queueName_A.Dead));

            Assert.AreEqual(1, GetQueueSize(queueName_B.Default));
            Assert.AreEqual(0, GetQueueSize(queueName_B.Dead));

            PurgeQueue(queueName_A.Default);
            PurgeQueue(queueName_A.Dead);

            PurgeQueue(queueName_B.Default);
            PurgeQueue(queueName_B.Dead);

            Assert.AreEqual(0, GetQueueSize(queueName_A.Default));
            Assert.AreEqual(0, GetQueueSize(queueName_A.Dead));

            Assert.AreEqual(0, GetQueueSize(queueName_B.Default));
            Assert.AreEqual(0, GetQueueSize(queueName_B.Dead));
        }

        private bool _consumerFired_A = false;
        private bool _consumerFired_B = false;

        private bool ProcessQueueAorB(MessagingArgs arg)
        {
            if (arg.AsString == "Message_A")
                _consumerFired_A = true;

            if (arg.AsString == "Message_B")
                _consumerFired_B = true;

            return false;
        }

        protected void Testa_o_not_acknowledge_no_metodo_de_recebimento_executando_o_consume_da_fila()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(_queueName.Default, message, _config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            var consumerFired = false;
            using (var client = new Messaging(_config))
            {
                client.StartListening(_queueName.Default, (e) =>
                {
                    consumerFired = true;

                    Assert.IsTrue(e.AsString == message);

                    return false;
                });
                for (int index = 0; index < 200 && !consumerFired; ++index)
                    Thread.Sleep(50);
            }

            Assert.IsTrue(consumerFired);
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        protected void Testa_garantia_de_recebimento_da_fila_sem_using()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var message = Guid.NewGuid().ToString();


            Messaging.Enqueue(_queueName.Default, message, _config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            try
            {
                var receivedMessage = Messaging.Dequeue(_queueName.Default, _config);
                Assert.IsNotNull(receivedMessage);
            }
            catch { }
            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        protected void Testa_garantia_de_envio_para_fila()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var message = Guid.NewGuid().ToString();

            try
            {
                using (var client = new Messaging(_config))
                {
                    bool exceptionWasThrow = false;
                    try
                    {
                        client.EnqueueMessage(_queueName.Default, message);
                    }
                    catch
                    {
                        exceptionWasThrow = true;
                    }
                    Assert.IsFalse(exceptionWasThrow);
                    Assert.AreEqual(1, GetQueueSize(_queueName.Default));
                    throw new Exception(message);
                }
            }
            catch (Exception exception)
            {
                Assert.AreEqual(message, exception.Message);
            }
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            var received = Messaging.Dequeue(_queueName.Default, _config);
            Assert.IsTrue(received.Contains(message));
        }

        protected void Testa_garantia_de_envio_para_fila_sem_using()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var guid = Guid.NewGuid().ToString();
            try
            {
                bool exceptionWasThrow = false;
                try
                {
                    Messaging.Enqueue(_queueName.Default, new { id = guid }, _config);
                }
                catch
                {
                    exceptionWasThrow = true;
                }
                Assert.IsFalse(exceptionWasThrow);
                Assert.AreEqual(1, GetQueueSize(_queueName.Default));
                throw new Exception(guid);
            }
            catch (Exception exception)
            {
                Assert.AreEqual(guid, exception.Message);
            }
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            var received = Messaging.Dequeue(_queueName.Default, _config);
            Assert.IsTrue(received.Contains(guid));
        }

        protected void StartListening_deve_acusar_erro_com_metodo_nulo()
        {
            var queueName = "bla";
            try
            {
                using (var client = new Messaging(_config))
                {
                    client.StartListening(queueName, null);
                }
            }
            finally
            {
                PurgeQueue(queueName);
            }
        }

        protected void Dequeue_de_fila_vazia_deve_retornar_null()
        {
            var queueName = "bla";
            try
            {
                var result = Messaging.Dequeue(queueName, _config);
                Assert.IsNull(result);

                result = Messaging.Dequeue<string>(queueName, _config);
                Assert.IsNull(result);
            }
            finally
            {
                PurgeQueue(queueName);
            }
        }
        

        protected void Mensagem_não_deve_ser_perdida_por_conta_do_dispose()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            Messaging.Enqueue(_queueName.Default, "Message_X", _config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            var client01 = new Messaging(_config);

            client01.StartListening(_queueName.Default, ProccessWithoutConfirmation);

            for (int index = 0; index < 200 && !_proccessWithoutConfirmationFired; ++index)
                Thread.Sleep(10);

            client01.Dispose();

            Assert.IsTrue(_proccessWithoutConfirmationFired);
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        protected void Client_nao_deve_permitir_consumo_simultaneo_01()
        {
            try
            {
                using (var client = new Messaging(_config))
                {
                    client.StartListening(_queueName.Default, ProcessQueueAorB);
                    client.StartListening(_queueName.Default, ProcessQueueAorB);
                }
            }
            catch(Exception exception)
            {
                Assert.AreEqual("There is already a listener being used in this context.", exception.Message);
                throw;
            }
        }

        private bool _proccessWithoutConfirmationFired = false;

        private bool ProccessWithoutConfirmation(MessagingArgs arg)
        {
            _proccessWithoutConfirmationFired = true;

            return false;
        }

        protected abstract int GetQueueSize(string fila);

        protected abstract void PurgeQueue(string queueName);
    }
}
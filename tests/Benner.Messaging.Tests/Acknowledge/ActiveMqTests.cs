using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Benner.Messaging.Tests.Acknowledge
{

    [TestClass]
    public class ActiveMqTests : AcknowledgeTestsBase
    {
        private readonly ConnectionFactory _factory = new ConnectionFactory("tcp://bnu-vtec001:61616");
        public ActiveMqTests() :
            base(new MessagingConfigBuilder("ActiveMQ", BrokerType.ActiveMQ, new Dictionary<string, string>()
            {   {"UserName", "admin"},
                {"Password", "admin"},
                {"Hostname", "bnu-vtec001" }
            }).Create())
        { }

        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_activeMq()
        {
            base.Testa_garantia_de_recebimento_da_fila();
        }

        [TestMethod]
        public void Testa_o_acknowledge_no_metodo_de_recebimento_da_fila_no_activeMq()
        {
            base.Testa_o_acknowledge_no_metodo_de_recebimento_da_fila();
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_sem_executar_o_consume_da_fila_no_activeMq()
        {
            base.Testa_o_not_acknowledge_no_metodo_de_recebimento_sem_executar_o_consume_da_fila();
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_executando_o_consume_porem_disposeando_o_client_da_fila_no_activeMq()
        {
            base.Testa_o_not_acknowledge_no_metodo_de_recebimento_executando_o_consume_porem_disposeando_o_client_da_fila();
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_com_duas_mensagens_sem_executar_o_consume_da_fila_no_activeMq()
        {
            base.Testa_o_not_acknowledge_no_metodo_de_recebimento_com_duas_mensagens_sem_executar_o_consume_da_fila();
        }

        [TestMethod]
        public void Dois_clients_simultaneos_ouvindo_filas_distintas_activeMq()
        {
            base.Dois_clients_simultaneos_ouvindo_filas_distintas();
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_executando_o_consume_da_fila_no_activeMq()
        {
            base.Testa_o_not_acknowledge_no_metodo_de_recebimento_executando_o_consume_da_fila();
        }

        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_activeMq_sem_using()
        {
            base.Testa_garantia_de_recebimento_da_fila_sem_using();
        }

        [TestMethod]
        public void Testa_garantia_de_envio_para_fila_no_activeMq()
        {
            base.Testa_garantia_de_envio_para_fila();
        }

        [TestMethod]
        public void Testa_garantia_de_envio_para_fila_no_activeMq_sem_using()
        {
            base.Testa_garantia_de_envio_para_fila_sem_using();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartListening_deve_acusar_erro_com_metodo_nulo_no_activeMq()
        {
            base.StartListening_deve_acusar_erro_com_metodo_nulo();
        }

        [TestMethod]
        public void Dequeue_de_fila_vazia_deve_retornar_null_activeMq()
        {
            base.Dequeue_de_fila_vazia_deve_retornar_null();
        }

        [TestMethod]
        public void Mensagem_não_deve_ser_perdida_por_conta_do_dispose_activeMq()
        {
            base.Mensagem_não_deve_ser_perdida_por_conta_do_dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Client_nao_deve_permitir_consumo_simultaneo_01_activeMq()
        {
            base.Client_nao_deve_permitir_consumo_simultaneo_01();
        }

        protected override int GetQueueSize(string fila)
        {
            int count = 0;
            using (Connection conn = _factory.CreateConnection() as Connection)
            {
                conn.Start();
                using (ISession session = conn.CreateSession())
                {
                    IQueue queue = session.GetQueue(fila);
                    using (IQueueBrowser queueBrowser = session.CreateBrowser(queue))
                    {
                        IEnumerator messages = queueBrowser.GetEnumerator();
                        while (messages.MoveNext())
                        {
                            IMessage message = (IMessage)messages.Current;
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        protected override void PurgeQueue(string queueName)
        {
            using (Connection conn = _factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.DeleteQueue(queueName);
                }
            }
        }
    }
}
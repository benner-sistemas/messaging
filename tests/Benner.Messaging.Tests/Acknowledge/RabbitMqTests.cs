using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Benner.Messaging.Tests.Acknowledge
{

    [TestClass]
    public class RabbitMqTests : AcknowledgeTestsBase
    {
        private readonly ConnectionFactory factory = new ConnectionFactory
        {
            HostName = "bnu-vtec012",
            UserName = "guest",
            Port = 5672
        };
        public RabbitMqTests() :
            base(new MessagingConfigBuilder("RabbitMQ", BrokerType.RabbitMQ, new Dictionary<string, string>()
                {
                    {"UserName", "guest"},
                    {"Password", "guest"},
                    {"HostName", "bnu-vtec012"}
                })
                .Create())
        { }

        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_rabbitMq()
        {
            base.Testa_garantia_de_recebimento_da_fila();
        }

        [TestMethod]
        public void Testa_o_acknowledge_no_metodo_de_recebimento_da_fila_no_rabbitMq()
        {
            base.Testa_o_acknowledge_no_metodo_de_recebimento_da_fila();
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_sem_executar_o_consume_da_fila_no_rabbitMq()
        {
            base.Testa_o_not_acknowledge_no_metodo_de_recebimento_sem_executar_o_consume_da_fila();
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_executando_o_consume_porem_disposeando_o_client_da_fila_no_rabbitMq()
        {
            base.Testa_o_not_acknowledge_no_metodo_de_recebimento_executando_o_consume_porem_disposeando_o_client_da_fila();
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_com_duas_mensagens_sem_executar_o_consume_da_fila_no_rabbitMq()
        {
            base.Testa_o_not_acknowledge_no_metodo_de_recebimento_com_duas_mensagens_sem_executar_o_consume_da_fila();
        }

        [TestMethod]
        public void Dois_clients_simultaneos_ouvindo_filas_distintas_rabbitMq()
        {
            base.Dois_clients_simultaneos_ouvindo_filas_distintas();
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_executando_o_consume_da_fila_no_rabbitMq()
        {
            base.Testa_o_not_acknowledge_no_metodo_de_recebimento_executando_o_consume_da_fila();
        }

        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_rabbitMq_sem_using()
        {
            base.Testa_garantia_de_recebimento_da_fila_sem_using();
        }

        [TestMethod]
        public void Testa_garantia_de_envio_para_fila_no_rabbitMq()
        {
            base.Testa_garantia_de_envio_para_fila();
        }

        [TestMethod]
        public void Testa_garantia_de_envio_para_fila_no_rabbitMq_sem_using()
        {
            base.Testa_garantia_de_envio_para_fila_sem_using();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartListening_deve_acusar_erro_com_metodo_nulo_no_rabbitMq()
        {
            base.StartListening_deve_acusar_erro_com_metodo_nulo();
        }

        [TestMethod]
        public void Dequeue_de_fila_vazia_deve_retornar_null_rabbitMq()
        {
            base.Dequeue_de_fila_vazia_deve_retornar_null();
        }

        [TestMethod]
        public void Mensagem_não_deve_ser_perdida_por_conta_do_dispose_rabbitMq()
        {
            base.Mensagem_não_deve_ser_perdida_por_conta_do_dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Client_nao_deve_permitir_consumo_simultaneo_01_rabbitMq()
        {
            base.Client_nao_deve_permitir_consumo_simultaneo_01();
        }

        protected override int GetQueueSize(string fila)
        {
            uint result = 0;
            try
            {
                using (var conn = factory.CreateConnection())
                {
                    using (var channel = conn.CreateModel())
                    {
                        result = channel.MessageCount(fila);
                    }
                }
            }
            catch
            { }
            return Convert.ToInt32(result);
        }

        protected override void PurgeQueue(string queueName)
        {
            using (var conn = factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    try
                    {
                        channel.QueuePurge(queueName);
                    }
                    catch
                    { }
                }
            }
        }
    }
}
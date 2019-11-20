using Benner.Listener;
using Benner.Messaging;

namespace Benner.Consumer.Core
{
    public class Program
    {
        /// <summary>
        /// dotnet run -broker:RabbitMQ -host:bnu-vtec012 -port:6287 -fila:contabil
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public int Main(string[] args)
        {
            //com base no identificador da fila, tenho que conseguir injetar de alguma 
            // a implementação do 'ContabilizacaoConsumer' com D.I.
            // ver exemplo do BTL, por exemplo.

            // instanciar o consumer
            // invocar o IOC do cara
            var consumer = new ContabilizacaoConsumer();

            // instanciar uma configuração
            var config = new MessagingConfigBuilder()
                .WithRabbitMQBroker("RabbitMQ", "servername", setAsDefault: true)
                .Create();

            // instanciar um listener injetando consumer e a configuração
            var listener = new EnterpriseIntegrationListener(config, consumer);
            listener.Start();

            return 0;
        }
    }
}

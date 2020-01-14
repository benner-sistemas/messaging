using System;
using Benner.ERP.Models;
using Benner.Listener;
using Benner.Messaging;

namespace ERP.Consumer
{
    public class PessoaConsumer : IEnterpriseIntegrationConsumer
    {
        public IEnterpriseIntegrationSettings Settings => new EnterpriseIntegrationSettings()
        {
            QueueName = "fila-pessoa-consumer",
            RetryIntervalInMilliseconds = 1000,
            RetryLimit = 3
        };

        public void OnMessage(object message)
        {
            try
            {
                var request = Utils.DeserializeObject<PessoaRequest>((string)message);

                if (request == null)
                    throw new ArgumentNullException($"Request não é '{nameof(PessoaRequest)}'");

                if (string.IsNullOrWhiteSpace(request.CPF) || request.CPF.Length != 11)
                    throw new InvalidMessageException("CPF deve ser informado e conter 11 caracteres");

                if (!request.Nascimento.HasValue)
                    throw new InvalidMessageException("Data de nascimento deve ser informada");

                if (string.IsNullOrWhiteSpace(request.Nome))
                    throw new InvalidMessageException("Nome deve ser informado");

                if (request?.RequestID == null || !request.RequestID.HasValue)
                    throw new InvalidMessageException("Request ID deve ser informado");

                if (request.Endereco == null)
                    throw new InvalidMessageException("Endereço deve ser preenchido");

                Console.WriteLine("\r\nPessoaConsumer.OnMessage:" + request + "\r\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("OnMessage ERROR: " + e.Message + "\r\n");
            }
        }

        public void OnInvalidMessage(object message)
        {
            var request = Utils.DeserializeObject<PessoaRequest>((string)message);
            //var request = message as PessoaRequest ?? throw new ArgumentNullException($"Request não é do tipo '{nameof(PessoaRequest)}'");

            // fazer algo com a request
            Console.WriteLine("PessoaConsumer.OnInvalidMessage:" + request + "\r\n");
        }

        public void OnDeadMessage(object message)
        {
            var request = Utils.DeserializeObject<PessoaRequest>((string)message);
            //var request = message as PessoaRequest ?? throw new ArgumentNullException($"Request não é do tipo '{nameof(PessoaRequest)}'");

            // fazer algo com a request
            Console.WriteLine("PessoaConsumer.OnDeadMessage:" + request + "\r\n");
        }
    }
}

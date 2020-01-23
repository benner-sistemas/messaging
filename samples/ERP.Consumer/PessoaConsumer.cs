using Benner.ERP.Models;
using Benner.Listener;
using System;

namespace ERP.Consumer
{
    public class PessoaConsumer : EnterpriseIntegrationConsumerBase
    {
        public override IEnterpriseIntegrationSettings Settings => new EnterpriseIntegrationSettings()
        {
            QueueName = "fila-pessoa-consumer",
            RetryIntervalInMilliseconds = 1000,
            RetryLimit = 3,
        };

        public override void OnMessage(string message)
        {
            PessoaRequest request = null;
            try
            {
                request = DeserializeMessage<PessoaRequest>(message);

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

                LogInformation("PessoaConsumer.OnMessage {requestId}:", request.RequestID);
            }
            catch (Exception e)
            {
                LogError(e, "Erro OnMessage: {requestId}:", request?.RequestID);
                throw;
            }
        }

        public override void OnInvalidMessage(string message)
        {
            var request = DeserializeMessage<PessoaRequest>(message);
            if (request == null)
                throw new ArgumentNullException($"Request não é '{nameof(PessoaRequest)}'");

            // fazer algo com a request
            LogInformation("PessoaConsumer.OnInvalidMessage {requestId}:", request.RequestID);
        }

        public override void OnDeadMessage(string message)
        {
            var request = DeserializeMessage<PessoaRequest>(message);
            if (request == null)
                throw new ArgumentNullException($"Request não é '{nameof(PessoaRequest)}'");

            // fazer algo com a request
            LogInformation("PessoaConsumer.OnDeadMessage {requestId}:", request.RequestID);
        }
    }
}
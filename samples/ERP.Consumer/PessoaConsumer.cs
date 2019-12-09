using System;
using Benner.ERP.API.Models;
using Benner.Listener;

namespace ERP.Consumer
{
    public class PessoaConsumer : IEnterpriseIntegrationConsumer
    {
        public IEnterpriseIntegrationSettings Settings => throw new NotImplementedException();

        public void OnMessage(object message)
        {
            var request = message as PessoaRequest;

            if (request == null)
                throw new ArgumentNullException($"Request não é '{nameof(PessoaRequest)}'");

            if (string.IsNullOrWhiteSpace(request.CPF) || request.CPF.Length != 11)
                throw new InvalidMessageException("CPF deve ser informado e conter 11 caracteres");

            if (!request.Nascimento.HasValue)
                throw new InvalidMessageException("Data de nascimento deve ser informada");

            if (string.IsNullOrWhiteSpace(request.Nome))
                throw new InvalidMessageException("Nome deve ser informado");

            if (request.Endereco == null)
                throw new InvalidMessageException("Endereço deve ser preenchido");

            // fazer algo com a request
        }

        public void OnInvalidMessage(object message)
        {
            var request = message as PessoaRequest;

            if (request == null)
                throw new ArgumentNullException($"Request não é '{nameof(PessoaRequest)}'");

            // fazer algo com a request
        }

        public void OnDeadMessage(object message)
        {
            var request = message as PessoaRequest;

            if (request == null)
                throw new ArgumentNullException($"Request não é '{nameof(PessoaRequest)}'");

            // fazer algo com a request
        }
    }
}

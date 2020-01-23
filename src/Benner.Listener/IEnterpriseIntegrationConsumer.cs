using System;

namespace Benner.Listener
{
    public interface IEnterpriseIntegrationConsumer
    {
        IEnterpriseIntegrationSettings Settings { get; }
        /// <summary>
        /// Método disparado ao receber uma mensagem, local onde a mensagem deve ser processada
        /// </summary>
        /// <param name="message">A mensagem recebida</param>
        void OnMessage(string message);
        
        /// <summary>
        /// Método disparado após a ocorrência de uma <see cref="InvalidMessageException"/> durante o <see cref="OnMessage(Message)"/>, 
        /// ou seja, quando uma mensagem é considerada inválida ou inconsistente, o que impossibilita o processamento da mensagem.
        /// Esse método pode ser utilizado para disparar alguma ação de compensação, ou algum tipo de notificação ao produtor da mensagem.
        /// </summary>
        /// <param name="message"></param>
        void OnInvalidMessage(string message, InvalidMessageException exception);
        
        /// <summary>
        /// Método disparado após as retentativas de processamento terem sido esgotadas.
        /// </summary>
        /// <param name="message"></param>
        void OnDeadMessage(string message, Exception exception);
    }
}
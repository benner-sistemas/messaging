using System;

namespace Benner.Messaging.Interfaces
{
    /// <summary>
    /// Interface base para as classes dos transporters.
    /// </summary>
    public interface IBrokerTransport : IDisposable
    {
        /// <summary>
        /// Envia uma mensagem para a fila informada.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="message">A mensagem.</param>
        void EnqueueMessage(string queueName, string message);

        /// <summary>
        /// Inicia um listener/consumidor para a fila informada.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="func">A Func que receberá e processará as mensagens.</param>
        void StartListening(string queueName, Func<MessagingArgs, bool> func);

        /// <summary>
        /// Desenfileira uma única mensagem.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <returns>A mensagem desenfileirada. 
        /// Caso não existam mensagens na fila retorna <see cref="null"/>.
        /// Cada transporter espera um tempo configurado ou pré-determinado antes de retornar null.
        /// </returns>
        void DequeueSingleMessage(string queueName, Func<string, bool> func);
    }
}

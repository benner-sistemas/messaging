using System;
using Benner.Messaging.Interfaces;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe base para os transporters.
    /// </summary>
    public abstract class BrokerTransport : IBrokerTransport
    {
        public abstract void Dispose();

        /// <summary>
        /// Envia uma mensagem para a fila informada.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="message">A mensagem.</param>
        public abstract void EnqueueMessage(string queueName, string message);

        /// <summary>
        /// Inicia um listener/consumidor para a fila informada.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="func">A Func que receberá e processará as mensagens.</param>
        public abstract void StartListening(string queueName, Func<MessagingArgs, bool> func);

        /// <summary>
        /// Desenfileira uma única mensagem.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <returns>A mensagem desenfileirada. 
        /// Caso não existam mensagens na fila retorna <see cref="null"/>.
        /// Cada transporter espera um tempo configurado ou pré-determinado antes de retornar null.
        /// </returns>
        public abstract void DequeueSingleMessage(string queueName, Func<string, bool> func);

        ~BrokerTransport()
        {
            Dispose();
        }
    }
}

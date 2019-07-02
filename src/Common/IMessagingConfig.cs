namespace Benner.Messaging.Interfaces
{
    /// <summary>
    /// Interface para as classes de diferentes modos de configuração de filas.
    /// </summary>
    public interface IMessagingConfig
    {
        /// <summary>
        /// Obtém a instância de um <see cref="IBrokerConfig"/> relativo ao nome da fila. 
        /// Caso a fila não exista na configuração, o broker definido como default em 'brokerList' é utilizado.
        /// </summary>
        /// <param name="queueName">O nome da fila</param>
        /// <returns>A instância de um <see cref="IBrokerConfig"/></returns>
        IBrokerConfig GetConfigForQueue(string queueName);
    }
}

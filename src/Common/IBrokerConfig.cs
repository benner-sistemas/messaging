namespace Benner.Messaging.Interfaces
{
    /// <summary>
    /// Interface base para as classes de configuração específicas de cada transporter.
    /// </summary>
    public interface IBrokerConfig
    {
        /// <summary>
        /// Cria a instância para o transporter relativo à esta configuração.
        /// </summary>
        /// <returns>A instância do transporter.</returns>
        BrokerTransport CreateTransporterInstance();
    }

    /// <summary>
    /// Interface interna para as classes de configuração.
    /// </summary>
    internal interface IInternalBrokerConfig : IBrokerConfig
    { }
}
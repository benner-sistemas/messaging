using Newtonsoft.Json;
using System;
using System.Text;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe base para armazenar e desserializar mensagens recebidas por um listener de um <see cref="Messaging"/>.
    /// A criptografia padrão para conversão de mensagens é <see cref="Encoding.UTF8"/>.
    /// </summary>
    public class MessagingArgs
    {
        /// <summary>
        /// A mensagem em um <see cref="byte[]"/>.
        /// </summary>
        public byte[] AsBytes { get; set; }

        /// <summary>
        /// A mensagem em <see cref="string"/>.
        /// </summary>
        public string AsString { get; set; }

        private MessagingArgs(byte[] rawMessage, string stringMessage)
        {
            AsBytes = rawMessage;
            AsString = stringMessage;
        }

        /// <summary>
        /// </summary>
        /// <param name="rawMessage">A mensagem em formato de <see cref="byte[]"/>.</param>
        public MessagingArgs(byte[] rawMessage) : this(rawMessage, Encoding.UTF8.GetString(rawMessage))
        { }

        /// <summary>
        /// </summary>
        /// <param name="stringMessage">A mensagem em <see cref="string"/>.</param>
        public MessagingArgs(string stringMessage) : this(Encoding.UTF8.GetBytes(stringMessage), stringMessage)
        { }

        /// <summary>
        /// Desserializa a mensagem para o objeto do tipo informado.
        /// </summary>
        /// <typeparam name="T">O tipo do objeto.</typeparam>
        /// <returns>Uma nova instancia do objeto desserializado.</returns>
        /// <exception cref="InvalidCastException">Ocorre quando há algum tipo de erro na desserialização.</exception>
        public T GetMessage<T>()
        {
            return Utils.DeserializeObject<T>(AsString);
        }
    }
}
using Newtonsoft.Json;
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
        public byte[] AsBytes
        {
            get
            {
                if (_bytesMessage == null)
                    _bytesMessage = Encoding.UTF8.GetBytes(_stringMessage);
                return _bytesMessage;
            }
        }

        /// <summary>
        /// A mensagem em <see cref="string"/>.
        /// </summary>
        public string AsString
        {
            get
            {
                if (_stringMessage == null)
                    _stringMessage = Encoding.UTF8.GetString(_bytesMessage);
                return _stringMessage;
            }
        }

        private byte[] _bytesMessage;
        private string _stringMessage;

        /// <summary>
        /// </summary>
        /// <param name="rawMessage">A mensagem em formato de <see cref="byte[]"/>.</param>
        public MessagingArgs(byte[] rawMessage) => _bytesMessage = rawMessage;

        /// <summary>
        /// </summary>
        /// <param name="rawMessage">A mensagem em <see cref="string"/>.</param>
        public MessagingArgs(string rawMessage) => _stringMessage = rawMessage;

        /// <summary>
        /// Desserializa a mensagem para o objeto do tipo informado.
        /// </summary>
        /// <typeparam name="T">O tipo do objeto.</typeparam>
        /// <returns>Uma nova instancia do objeto desserializado.</returns>
        public T GetMessage<T>()
        {
            return JsonConvert.DeserializeObject<T>(AsString, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            });
        }
    }
}
using Benner.Messaging.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe responsável pelo envio e recebimento de mensagens.
    /// </summary>
    public class Client : IDisposable
    {
        private readonly IMessagingConfig _config;
        private readonly Dictionary<string, IBrokerTransport> _brokers;

        /// <summary>
        /// Instancia um <see cref="Client"/> com configuração padrão de <see cref="FileMessagingConfig"/>.
        /// </summary>
        public Client() : this(new FileMessagingConfig())
        { }

        /// <summary>
        /// Instancia um <see cref="Client"/> com a configuração informada.
        /// </summary>
        /// <param name="config"></param>
        public Client(IMessagingConfig config)
        {
            _config = config;
            _brokers = new Dictionary<string, IBrokerTransport>(StringComparer.OrdinalIgnoreCase);
        }

        ~Client()
        {
            Dispose();
        }

        /// <summary>
        /// Faz o dispose de todos os brokers em cache.
        /// </summary>
        public void Dispose()
        {
            foreach (var transport in _brokers.Values)
                transport.Dispose();
        }

        /// <summary>
        /// Encapsulamento seguro para receber a próxima mensagem da fila.
        /// Obtém a próxima mensagem da fila informada e desserializa para um objeto. 
        /// Uma vez que a mensagem é recuperada da fila, esta é removida da fila. 
        /// Caso ocorra erro de desserialização por serem objetos de tipos diferentes, a mensagem é perdida.
        /// Dependendo do serviço utilizado, este método pode levar alguns segundos esperando por novas mensagens até retornar.
        /// Caso não existam mensagens, retorna <see cref="null"/>. 
        /// </summary>
        /// <typeparam name="T">O tipo para qual a mensagem será desserializada.</typeparam>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="config">A configuração de filas.</param>
        /// <returns>O objeto desserializado da mensagem.</returns>
        /// <exception cref="InvalidOperationException">Ocorre quando a conexão com o servidor de filas falha.</exception>
        public static T DequeueSingleMessage<T>(string queueName, IMessagingConfig config = default(FileMessagingConfig))
        {
            var rawMessage = DequeueSingleMessage(queueName, config);
            var jsSettings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            if (rawMessage == null)
                return default(T);

            return JsonConvert.DeserializeObject<T>(rawMessage, jsSettings);
        }

        /// <summary>
        /// Encapsulamento seguro para receber a próxima mensagem da fila.
        /// Obtém a próxima mensagem da fila informada.
        /// Uma vez que a mensagem é recuperada da fila, esta é removida da fila. 
        /// Dependendo do serviço utilizado, este método pode levar alguns segundos esperando por novas mensagens até retornar.
        /// Caso não existam mensagens, retorna <see cref="null"/>. 
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="config">A configuração de filas.</param>
        /// <returns>A mensagem recebida.</returns>
        /// <exception cref="InvalidOperationException">Ocorre quando a conexão com o servidor de filas falha.</exception>
        public static string DequeueSingleMessage(string queueName, IMessagingConfig config = default(FileMessagingConfig))
        {
            queueName = queueName.ToLower();
            Utils.ValidateQueueName(queueName, true);

            using (var client = new Client(config))
            {
                var transporter = client.GetTransporter(queueName);
                return transporter.DequeueSingleMessage(queueName);
            }
        }

        /// <summary>
        /// Encapsulamento seguro para receber a próxima mensagem da fila.
        /// Envia um objeto para o serviço configurado.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="objMessage">O objeto a ser serializado e enviado como mensagem.</param>
        /// <param name="config">A configuração de filas.</param>
        /// <exception cref="InvalidOperationException">Ocorre quando a conexão com o servidor de filas falha.</exception>
        public static void EnqueueSingleMessage(string queueName, object objMessage, IMessagingConfig config = default(FileMessagingConfig))
        {
            var messageString = JsonConvert.SerializeObject(objMessage);
            EnqueueSingleMessage(queueName, messageString, config);
        }

        /// <summary>
        /// Encapsulamento seguro para receber a próxima mensagem da fila.
        /// Envia uma mensagem para o serviço configurado.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="objMessage">O objeto a ser serializado e enviado como mensagem.</param>
        /// <param name="config">A configuração de filas.</param>
        /// <exception cref="InvalidOperationException">Ocorre quando a conexão com o servidor de filas falha.</exception>
        public static void EnqueueSingleMessage(string queueName, string message, IMessagingConfig config = default(FileMessagingConfig))
        {
            using (var client = new Client(config))
                client.EnqueueMessage(queueName, message);
        }

        /// <summary>
        /// Envia um objeto para o serviço configurado.
        /// É extremamente recomendado fazer o <see cref="Dispose"/> na instância após todos os objetos enviados.
        /// Este método é recomendado quando há muitos objetos a serem enviados de uma vez. 
        /// Caso queira enviar apenas um, utilize <see cref="Client.EnqueueMessage(string, object)"/>.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="objMessage">O objeto a ser serializado e enviado como mensagem.</param>
        /// <exception cref="InvalidOperationException">Ocorre quando a conexão com o servidor de filas falha.</exception>
        public void EnqueueMessage(string queueName, object objMessage)
        {
            var messageString = JsonConvert.SerializeObject(objMessage);
            EnqueueMessage(queueName, messageString);
        }

        /// <summary>
        /// Envia uma mensagem para o serviço configurado.
        /// É extremamente recomendado fazer o <see cref="Dispose"/> na instância após todas as mensagens enviadas.
        /// Este método é recomendado quando há muitas mensagens a serem enviadas de uma vez. 
        /// Caso queira enviar apenas uma utilize <see cref="Client.EnqueueMessage(string, string)"/>.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="message">O objeto a ser serializado e enviado como mensagem.</param>
        /// <exception cref="InvalidOperationException">Ocorre quando a conexão com o servidor de filas falha.</exception>
        public void EnqueueMessage(string queueName, string message)
        {
            queueName = queueName.ToLower();
            Utils.ValidateQueueName(queueName, true);
            var transporter = GetTransporter(queueName);
            transporter.EnqueueMessage(queueName, message);
        }

        /// <summary>
        /// Inicia um listener para a fila.
        /// </summary>
        /// <param name="queueName">Nome da fila.</param>
        /// <param name="func">
        /// A função que irá receber as mensagens e processá-las. 
        /// Caso retorne <see cref="true"/> o processamento é considerado completo com sucesso. A mensagem é retirada da fila.
        /// Caso retorne <see cref="false"/> o processamento é considerado completo mas com falha. 
        /// A mensagem volta para a fila e fica disponível para outros consumidores.
        /// Caso lance uma exceção a mensagem é considerada perigosa, retirada da fila e enfileirada em uma outra fila de erros. 
        /// Esta fila é o nome da fila que está sendo consumida com um sufixo '-error'.
        /// Se o nome de <paramref name="queueName"/> for 'fila-teste', a fila de erros será 'fila-teste-error'. 
        /// </param>
        /// <exception cref="InvalidOperationException">Ocorre quando a conexão com o servidor de filas falha.</exception>
        public void StartListening(string queueName, Func<MessagingArgs, bool> func)
        {
            queueName = queueName.ToLower();
            Utils.ValidateQueueName(queueName, true);
            var transporter = GetTransporter(queueName);
            transporter.StartListening(queueName, func);
        }

        private IBrokerTransport GetTransporter(string queue)
        {
            if (_brokers.TryGetValue(queue, out IBrokerTransport transport))
                return transport;

            IBrokerConfig config = _config.GetConfigForQueue(queue);
            transport = config.CreateTransporterInstance();
            _brokers.Add(queue, transport);

            return transport;
        }
    }
}
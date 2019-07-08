using Benner.Messaging.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Benner.Messaging
{
    /// <summary>
    /// The responsible class for message delivery and receipt.
    /// </summary>
    public class Messaging : IDisposable
    {
        private readonly IMessagingConfig _config;
        private readonly Dictionary<string, IBrokerTransport> _brokers;

        /// <summary>
        /// Instantiates a new <see cref="Messaging"/> object with <see cref="FileMessagingConfig"/>'s default constructor.
        /// </summary>
        public Messaging() : this(new FileMessagingConfig())
        { }

        public Messaging(IMessagingConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _brokers = new Dictionary<string, IBrokerTransport>(StringComparer.OrdinalIgnoreCase);
        }

        ~Messaging()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose all cached brokers.
        /// </summary>
        public void Dispose()
        {
            foreach (var transport in _brokers.Values)
                transport.Dispose();
        }

        /// <summary>
        /// Gets the next informed queue's message and deserializes it to an object.
        /// Once the message is dequeued, it is deleted from the queue.
        /// In case of deserialization errors (e.g. different types objects), the message is lost.
        /// Depending on the service, this method may take a few seconds to return, waiting for new messages before completion.
        /// If no message is found, returns null.
        /// Uses <see cref="FileMessagingConfig"/>'s default constructor.
        /// </summary>
        /// <typeparam name="T">The type to which the message will be deserialized to.</typeparam>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
        public static T Dequeue<T>(string queueName)
        {
            return Dequeue<T>(queueName, new FileMessagingConfig());
        }

        /// <summary>
        /// Gets the next informed queue's message and deserializes it to an object.
        /// Once the message is dequeued, it is deleted from the queue.
        /// In case of deserialization errors (e.g. different types objects), the message is lost.
        /// Depending on the service, this method may take a few seconds to return, waiting for new messages before completion.
        /// If no message is found, returns null.
        /// </summary>
        /// <typeparam name="T">The type to which the message will be deserialized to.</typeparam>
        /// <param name="config">The queue configuration.</param>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
        public static T Dequeue<T>(string queueName, IMessagingConfig config)
        {
            var rawMessage = Dequeue(queueName, config);
            var jsSettings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            if (rawMessage == null)
                return default(T);

            return JsonConvert.DeserializeObject<T>(rawMessage, jsSettings);
        }

        /// <summary>
        /// Gets the next informed queue's message.
        /// Once the message is dequeued, it is deleted from the queue.
        /// Depending on the service, this method may take a few seconds to return, waiting for new messages before completion.
        /// If no message is found, returns null.
        /// Uses <see cref="FileMessagingConfig"/>'s default constructor.
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
        public static string Dequeue(string queueName)
        {
            return Dequeue(queueName, new FileMessagingConfig());
        }

        /// <summary>
        /// Gets the next informed queue's message.
        /// Once the message is dequeued, it is deleted from the queue.
        /// Depending on the service, this method may take a few seconds to return, waiting for new messages before completion.
        /// If no message is found, returns null.
        /// </summary>
        /// <param name="config">The queue configuration.</param>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
        public static string Dequeue(string queueName, IMessagingConfig config)
        {
            queueName = queueName.ToLower();
            Utils.ValidateQueueName(queueName, true);

            using (var client = new Messaging(config))
            {
                var transporter = client.GetTransporter(queueName);
                return transporter.DequeueSingleMessage(queueName);
            }
        }

        /// <summary>
        /// Sends an object to the configured service.
        /// Uses <see cref="FileMessagingConfig"/>'s default constructor.
        /// </summary>
        /// <param name="objMessage">The object to be serialized and sent as message.</param>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
        public static void Enqueue(string queueName, object objMessage)
        {
            Enqueue(queueName, objMessage, new FileMessagingConfig());
        }

        /// <summary>
        /// Sends an object to the configured service.
        /// </summary>
        /// <param name="objMessage">The object to be serialized and sent as message.</param>
        /// <param name="config">The queue configuration.</param>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
        public static void Enqueue(string queueName, object objMessage, IMessagingConfig config)
        {
            Enqueue(queueName, JsonConvert.SerializeObject(objMessage), config);
        }

        /// <summary>
        /// Sends an object to the configured service.
        /// Uses <see cref="FileMessagingConfig"/>'s default constructor.
        /// </summary>
        /// <param name="objMessage">The object to be serialized and sent as message.</param>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
        public static void Enqueue(string queueName, string message)
        {
            Enqueue(queueName, message, new FileMessagingConfig());
        }

        /// <summary>
        /// Sends an object to the configured service.
        /// </summary>
        /// <param name="objMessage">The object to be serialized and sent as message.</param>
        /// <param name="config">The queue configuration.</param>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
        public static void Enqueue(string queueName, string message, IMessagingConfig config)
        {
            using (var client = new Messaging(config))
                client.EnqueueMessage(queueName, message);
        }

        /// <summary>
        /// Sends an object to the configured service.
        /// It is strongly recommended to Dispose the object after everything is complete.
        /// This method is recommended when there are lots of objects to be sent.
        /// If only one message has to be sent, it is advised to use static <see cref="Messaging.EnqueueMessage(string, object)"/>.
        /// </summary>
        /// <param name="objMessage">The object to be serialized and sent as message.</param>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
        public void EnqueueMessage(string queueName, object objMessage)
        {
            var messageString = JsonConvert.SerializeObject(objMessage);
            EnqueueMessage(queueName, messageString);
        }

        /// <summary>
        /// Sends an object to the configured service.
        /// It is strongly recommended to Dispose the object after everything is complete.
        /// This method is recommended when there are lots of objects to be sent.
        /// If only one message has to be sent, it is advised to use static <see cref="Messaging.EnqueueMessage(string, object)"/>.
        /// </summary>
        /// <param name="objMessage">The object to be serialized and sent as message.</param>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
        public void EnqueueMessage(string queueName, string message)
        {
            queueName = queueName.ToLower();
            Utils.ValidateQueueName(queueName, true);
            var transporter = GetTransporter(queueName);
            transporter.EnqueueMessage(queueName, message);
        }

        /// <summary>
        /// Starts a listener for the queue.
        /// </summary>
        /// <param name="func">
        /// The method that will receive and process the messages.
        /// Returns true: the processing is considered as succeeded. The message is deleted from queue.
        /// Returns false: the processing is considered complete with flaws. The message returns to the queue.
        /// Throws exception: the message is considered dangerous, removed from the queue and enqueued to another error queue.
        /// This error queue is another queue with the same name followed by "-error", e.g. if <paramref name="queueName"/> equals "test-queue",
        /// the error queue will be "test-queue-error".
        /// </param>
        /// <exception cref="InvalidOperationException">Occurs when the connection to the server fails.</exception>
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
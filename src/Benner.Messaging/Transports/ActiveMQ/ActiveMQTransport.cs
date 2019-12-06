using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Benner.Messaging
{
    internal class ActiveMQTransport : BrokerTransport
    {
        private readonly ActiveMQConfig _config;
        private readonly Uri _connectUri;
        private IConnection _connection;
        private IConnectionFactory _factory;
        private ISession _session;
        private readonly Dictionary<string, IDestination> _destinations;
        private bool _keepListenerAlive;
        private Task _listeningTask;

        public ActiveMQTransport(ActiveMQConfig config)
        {
            _config = config;
            _connectUri = new Uri($"activemq:tcp://{config.Hostname}:{config.Port}");
            _destinations = new Dictionary<string, IDestination>();
            _keepListenerAlive = true;
        }

        public override void EnqueueMessage(string queueName, string message)
        {
            ISession session = GetSession();
            IDestination destination = GetDestinationForQueue(queueName);

            using (IMessageProducer producer = session.CreateProducer(destination))
            {
                producer.DeliveryMode = MsgDeliveryMode.Persistent;
                var request = session.CreateTextMessage(message);

                try
                {
                    producer.Send(request);
                    var requestObj = request as ActiveMQMessage;
                    if (requestObj.ProducerId == null && requestObj.Destination == null)
                        throw new InvalidOperationException(ErrorMessages.EnqueueFailed);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(ErrorMessages.EnqueueFailed, e);
                }
            }
        }

        public override void StartListening(string queueName, Func<MessagingArgs, bool> func)
        {
            if (_listeningTask != null)
                throw new InvalidOperationException(ErrorMessages.AlreadyListening);

            var destination = GetDestinationForQueue(queueName);
            var consumer = GetSession().CreateConsumer(destination);
            _listeningTask = new Task(() => Listener(func, consumer, queueName), TaskCreationOptions.LongRunning);
            _listeningTask.Start();
        }

        private void Listener(Func<MessagingArgs, bool> func, IMessageConsumer consumer, string queueName)
        {
            while (_keepListenerAlive)
            {
                lock (_session)
                {
                    var message = consumer.Receive(TimeSpan.FromSeconds(10)) as ITextMessage;
                    if (message == null)
                        continue;

                    var acknowledge = true;
                    try
                    {
                        acknowledge = func(new MessagingArgs(message.Text));
                    }
                    catch (Exception exception)
                    {
                        var errorMessage = exception.Message;
                        EnqueueMessage($"{queueName}-error", string.Concat(errorMessage, "\r\n", message.Text));
                    }
                    finally
                    {
                        if (acknowledge)
                            message.Acknowledge();
                        //else
                        //    _session.Recover();
                    }
                }
            }
            consumer.Dispose();
        }

        private IConnectionFactory GetConnectionFactory()
        {
            if (_factory != null)
                return _factory;

            _factory = new NMSConnectionFactory(_connectUri);
            return _factory;
        }

        private IConnection GetConnection()
        {
            if (_connection != null)
                return _connection;

            try
            {
                _connection = GetConnectionFactory().CreateConnection(_config.Username, _config.Password);
                _connection.Start();
                return _connection;
            }
            catch (NMSConnectionException e)
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.UnableToConnect, "ActiveMQ"), e);
            }
        }

        private ISession GetSession()
        {
            if (_session != null)
                return _session;

            _session = GetConnection().CreateSession(AcknowledgementMode.IndividualAcknowledge);
            return _session;
        }

        private IDestination GetDestinationForQueue(string queueName)
        {
            if (_destinations.ContainsKey(queueName))
                return _destinations[queueName];

            _destinations[queueName] = GetSession().GetQueue(queueName);
            return _destinations[queueName];
        }

        public override void Dispose()
        {
            _keepListenerAlive = false;
            _listeningTask?.Wait();
            _connection?.Dispose();
            _session?.Dispose();
            foreach (var destino in _destinations.Values)
                destino?.Dispose();
        }

        public override void DequeueSingleMessage(string queueName, Func<string, bool> func)
        {
            var destination = GetDestinationForQueue(queueName);

            using (var consumer = GetSession().CreateConsumer(destination))
            {
                var message = consumer.Receive(TimeSpan.FromSeconds(1)) as ITextMessage;
                if (message == null)
                {
                    func(null);
                    return;
                }

                bool succeeded = func(message.Text);

                if (succeeded)
                    message.Acknowledge();
                else
                    _session.Recover();
            }
        }

        ~ActiveMQTransport()
        {
            Dispose();
        }
    }
}
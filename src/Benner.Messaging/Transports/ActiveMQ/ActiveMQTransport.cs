using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Benner.Messaging.Logger;
using System;
using System.Threading.Tasks;

namespace Benner.Messaging
{
    internal class ActiveMQTransport : BrokerTransport
    {
        private const long ReceiveTimeOutInMilliseconds = 200;
        private readonly ActiveMQConfig _config;
        private IConnection _connection;
        private ISession _producerSession;
        private ISession _consumerSession;
        private IMessageProducer _producer;
        private Task _listeningTask;
        private bool _keepListening;
        ~ActiveMQTransport()
        {
            Dispose();
        }
        public override void Dispose()
        {
            _keepListening = false;
            _listeningTask?.Wait();
            _listeningTask = null;

            _producer?.Close();
            _producer?.Dispose();
            _producer = null;

            _consumerSession?.Close();
            _consumerSession?.Dispose();
            _consumerSession = null;

            _producerSession?.Close();
            _producerSession?.Dispose();
            _producerSession = null;

            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }
        public ActiveMQTransport(ActiveMQConfig config)
        {
            _config = config;
        }
        private void EnsureConnection()
        {
            if (_connection == null)
                _connection = new ConnectionFactory(new Uri($"activemq:tcp://{_config.Hostname}:{_config.Port}"))
                    .CreateConnection(_config.Username, _config.Password);
        }
        private void EnsureProducer(string queueName)
        {
            EnsureConnection();

            if (_producerSession == null)
                _producerSession = _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

            if (!_connection.IsStarted)
                _connection.Start();

            if (_producer == null)
            {
                _producer = _producerSession.CreateProducer(_producerSession.GetDestination(queueName));
                _producer.DeliveryMode = MsgDeliveryMode.Persistent;
            }
        }

        private void EnsureConsumerSession()
        {
            EnsureConnection();

            if (_consumerSession == null)
                _consumerSession = _connection.CreateSession(AcknowledgementMode.IndividualAcknowledge);

            if (!_connection.IsStarted)
                _connection.Start();
        }

        public override void EnqueueMessage(string queueName, string message)
        {
            try
            {
                EnsureProducer(queueName);

                var request = _producerSession.CreateTextMessage(message);
                _producer.Send(request);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(ErrorMessages.EnqueueFailed, e);
            }
        }

        public override void DequeueSingleMessage(string queueName, Func<MessagingArgs, bool> func)
        {
            if (func == null)
                throw new InvalidOperationException(ErrorMessages.ConsumerFunctionMustBeInformed);

            if (_listeningTask != null)
                throw new InvalidOperationException(ErrorMessages.AlreadyListening);

            EnsureConsumerSession();
            try
            {
                ProcessMessage(func, queueName);
            }
            finally
            {
                _consumerSession.Close();
            }
        }

        public override void StartListening(string queueName, Func<MessagingArgs, bool> func)
        {
            if (func == null)
                throw new InvalidOperationException(ErrorMessages.ConsumerFunctionMustBeInformed);

            if (_listeningTask != null)
                throw new InvalidOperationException(ErrorMessages.AlreadyListening);

            _listeningTask = new Task(() => Listener(func, queueName), TaskCreationOptions.LongRunning);
            _keepListening = true;
            _listeningTask.Start();
        }

        private void Listener(Func<MessagingArgs, bool> func, string queueName)
        {
            EnsureConsumerSession();
            try
            {
                while (_keepListening)
                    ProcessMessage(func, queueName);
            }
            finally
            {
                _consumerSession.Close();
            }
        }

        private void ProcessMessage(Func<MessagingArgs, bool> func, string queueName)
        {
            using (var dedicatedConsumer = _consumerSession.CreateConsumer(_consumerSession.GetDestination(queueName)))
            {
                var rawMessage = dedicatedConsumer.Receive(TimeSpan.FromMilliseconds(ReceiveTimeOutInMilliseconds));
                if (rawMessage == null)
                    return;

                var textMessage = rawMessage as ITextMessage;

                var acknowledge = true;
                try
                {
                    acknowledge = func(new MessagingArgs(textMessage?.Text));
                }
                catch (Exception exception)
                {
                    Log.Error(exception, exception.Message);
                    EnqueueMessage(QueueName.DeadQueueName(queueName), string.Concat(exception.Message, "\r\n", textMessage?.Text));
                }
                finally
                {
                    if (acknowledge)
                        rawMessage.Acknowledge();
                }
                dedicatedConsumer.Close();
            }
        }
    }
}
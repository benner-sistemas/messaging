using Benner.Messaging.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Benner.Messaging
{
    internal class RabbitMQTransport : BrokerTransport
    {
        private enum ConectionType
        {
            Publish, Consume
        }

        private IConnection _publishConnection;
        private IConnection _consumeConnection;
        private IModel _publishChannel;
        private IModel _consumeChannel;
        private bool _isListening;
        private readonly RabbitMQConfig _config;
        private ConnectionFactory _factory;
        private readonly List<string> _declaredQueues;

        public RabbitMQTransport(RabbitMQConfig config)
        {
            _config = config;
            _factory = null;
            _publishConnection = _consumeConnection = null;
            _publishChannel = _consumeChannel = null;
            _declaredQueues = new List<string>();
        }

        private ConnectionFactory GetConnectionFactory()
        {
            if (_factory != null)
                return _factory;

            _factory = new ConnectionFactory() { UseBackgroundThreadsForIO = true };
            Type facType = _factory.GetType();
            var configProps = _config.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (PropertyInfo item in configProps)
            {
                string configPropValue = item.GetValue(_config) as string;
                if (!string.IsNullOrWhiteSpace(configPropValue))
                    facType.GetProperty(item.Name).SetValue(_factory, configPropValue);
            }

            return _factory;
        }

        private IConnection GetConnection(ConectionType type)
        {
            switch (type)
            {
                case ConectionType.Publish:
                    if (_publishConnection != null && _publishConnection.IsOpen)
                        return _publishConnection;
                    try
                    {
                        _publishConnection = GetConnectionFactory().CreateConnection();
                        return _publishConnection;
                    }
                    catch (BrokerUnreachableException e)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.UnableToConnect, "RabbitMQ"), e);
                    }
                case ConectionType.Consume:
                    if (_consumeConnection != null && _consumeConnection.IsOpen)
                        return _consumeConnection;
                    try
                    {
                        _consumeConnection = GetConnectionFactory().CreateConnection();
                        return _consumeConnection;
                    }
                    catch (BrokerUnreachableException e)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.UnableToConnect, "RabbitMQ"), e);
                    }
                default:
                    throw new ArgumentException(string.Format(ErrorMessages.MustBeInformed, "The connection type"));
            }
        }

        private IModel GetChannel(ConectionType type)
        {
            switch (type)
            {
                case ConectionType.Publish:
                    if (_publishChannel != null && _publishChannel.IsOpen)
                        return _publishChannel;
                    _publishChannel = GetConnection(type).CreateModel();
                    _publishChannel.ConfirmSelect();
                    return _publishChannel;
                case ConectionType.Consume:
                    if (_consumeChannel != null && _consumeChannel.IsOpen)
                        return _consumeChannel;
                    _consumeChannel = GetConnection(type).CreateModel();
                    _consumeChannel.BasicQos(0, 1, false);
                    return _consumeChannel;
                default:
                    throw new ArgumentException(string.Format(ErrorMessages.MustBeInformed, "The connection type"));
            }
        }

        public override void EnqueueMessage(string queueName, string message)
        {
            var channel = GetChannel(ConectionType.Publish);
            EnsureQueueDeclared(channel, queueName);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            try
            {
                channel.BasicPublish("", queueName, properties, Encoding.UTF8.GetBytes(message));
                channel.WaitForConfirmsOrDie();
            }
            catch (AlreadyClosedException e)
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.UnableToConnect, "RabbitMQ") + "The connection to server closed in the process of sending the message.", e);
            }
            catch (OperationInterruptedException e)
            {
                throw new InvalidOperationException(ErrorMessages.EnqueueFailed, e);
            }
        }

        public override void StartListening(string queueName, Func<MessagingArgs, bool> func)
        {
            if (_isListening)
                throw new InvalidOperationException(ErrorMessages.AlreadyListening);

            var channel = GetChannel(ConectionType.Consume);
            EnsureQueueDeclared(channel, queueName);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var acknowledge = true;
                try
                {
                    acknowledge = func(new MessagingArgs(ea.Body));
                }
                catch (Exception exception)
                {
                    EnqueueMessage($"{queueName}-error", string.Concat(exception.Message, "\r\n", Encoding.UTF8.GetString(ea.Body)));
                }
                finally
                {
                    if (acknowledge)
                        channel.BasicAck(ea.DeliveryTag, false);
                    else
                        channel.BasicReject(ea.DeliveryTag, true);
                }
            };

            channel.BasicConsume(queueName, false, consumer);
            _isListening = true;
        }

        private void EnsureQueueDeclared(IModel channel, string queue)
        {
            if (_declaredQueues.Contains(queue))
                return;

            channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _declaredQueues.Add(queue);
        }

        public override void Dispose()
        {
            _publishConnection?.Dispose();
            _consumeConnection?.Dispose();
        }

        public override void DequeueSingleMessage(string queueName, Func<string, bool> func)
        {
            var channel = GetChannel(ConectionType.Consume);
            EnsureQueueDeclared(channel, queueName);

            var result = channel.BasicGet(queueName, false);
            if (result == null)
            {
                func(null);
                return;
            }

            bool succeeded = func(Encoding.UTF8.GetString(result.Body));

            if (succeeded)
                channel.BasicAck(result.DeliveryTag, false);
            else
                channel.BasicNack(result.DeliveryTag, false, true);
        }

        ~RabbitMQTransport()
        {
            Dispose();
        }
    }
}
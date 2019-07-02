using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benner.Messaging
{
    internal class AmazonSqsTransport : BrokerTransport
    {
        private readonly AmazonSqsConfig _config;
        private IAmazonSQS _client;
        private readonly Dictionary<string, string> _declaredQueues;
        private bool _keepListenerAlive;
        private Task _listeningTask;

        public AmazonSqsTransport(AmazonSqsConfig config)
        {
            _config = config;
            _client = null;
            _declaredQueues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _keepListenerAlive = true;
        }

        private IAmazonSQS GetClient()
        {
            if (_client != null)
                return _client;

            _client = new AmazonSQSClient(RegionEndpoint.SAEast1);
            return _client;
        }

        public override void EnqueueMessage(string queueName, string message)
        {
            IAmazonSQS client = GetClient();
            string queueUrl = GetQueueUrl(queueName, client);
            var sqsMessageRequest = new SendMessageRequest(queueUrl, message);
            client.SendMessageAsync(sqsMessageRequest).Wait();
        }

        /// <summary>
        /// Obtém a url da queue, já criando-a caso não exista.
        /// </summary>
        /// <param name="queueName">Nome da queue</param>
        /// <param name="client">Client a ser usado</param>
        /// <returns>Url da queue</returns>
        private string GetQueueUrl(string queueName, IAmazonSQS client)
        {
            if (_declaredQueues.ContainsKey(queueName))
                return _declaredQueues[queueName];

            var sqsRequest = new CreateQueueRequest(queueName)
            {
                Attributes = new Dictionary<string, string>()
                {
                    {QueueAttributeName.ReceiveMessageWaitTimeSeconds, "20" }
                }
            };
            try
            {
                CreateQueueResponse createQueueResponse = client.CreateQueueAsync(sqsRequest).Result;
                string queueUrl = createQueueResponse.QueueUrl;
                _declaredQueues.Add(queueName, queueUrl);
                return queueUrl;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Não foi possível se conectar ao servidor Amazon.", e);
            }
        }

        public override void StartListening(string queueName, Func<MessagingArgs, bool> func)
        {
            if (_listeningTask != null)
                throw new InvalidOperationException("Escuta de fila neste contexto já está em uso");

            string queueUrl = GetQueueUrl(queueName, GetClient());
            ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest(queueUrl)
            {
                VisibilityTimeout = (int)TimeSpan.FromMinutes(_config.InvisibilityTimeInMinutes).TotalSeconds,
                WaitTimeSeconds = 20
            };

            _listeningTask = new Task(() => Listener(receiveMessageRequest, queueUrl, func, queueName), TaskCreationOptions.LongRunning);
            _listeningTask.Start();
        }

        private void Listener(ReceiveMessageRequest request, string queueUrl, Func<MessagingArgs, bool> func, string queueName)
        {
            IAmazonSQS client = GetClient();
            while (_keepListenerAlive)
            {
                ReceiveMessageResponse receiveMessageResponse = client.ReceiveMessageAsync(request).Result;
                var receivedMessage = receiveMessageResponse?.Messages?.FirstOrDefault();
                if (receivedMessage != null)
                {
                    var acknowledge = true;
                    try
                    {
                        acknowledge = func(new MessagingArgs(receivedMessage.Body));
                    }
                    catch (Exception exception)
                    {
                        EnqueueMessage($"{queueName}-error", string.Concat(exception.Message, "\r\n", receivedMessage.Body));
                    }
                    finally
                    {
                        if (acknowledge)
                            DeleteMessage(queueUrl, client, receivedMessage.ReceiptHandle);
                        else
                            client.ChangeMessageVisibilityAsync(queueUrl, receivedMessage.ReceiptHandle, 0).Wait();
                    }
                }
            }
        }

        private DeleteMessageResponse DeleteMessage(string queueUrl, IAmazonSQS client, string receiptHandle)
        {
            DeleteMessageRequest deleteRequest = new DeleteMessageRequest(queueUrl, receiptHandle);
            return client.DeleteMessageAsync(deleteRequest).Result;
        }

        public override string DequeueSingleMessage(string queueName)
        {
            var client = GetClient();
            var queueUrl = GetQueueUrl(queueName, client);
            var receiveMessageRequest = new ReceiveMessageRequest(queueUrl)
            {
                VisibilityTimeout = (int)TimeSpan.FromMinutes(_config.InvisibilityTimeInMinutes).TotalSeconds,
                WaitTimeSeconds = 1
            };
            var receivedMsgResponse = client.ReceiveMessageAsync(receiveMessageRequest)?.Result;
            if (receivedMsgResponse == null)
                return null;

            var receivedMessage = receivedMsgResponse.Messages.FirstOrDefault();
            if (receivedMessage == null)
                return null;

            var deleteResponse = DeleteMessage(queueUrl, client, receivedMessage.ReceiptHandle);
            if (deleteResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                throw new InvalidOperationException("Falha ao remover mensagem da fila");

            return receivedMessage.Body;
        }

        public override void Dispose()
        {
            _keepListenerAlive = false;
            _listeningTask?.Wait();
        }

        ~AmazonSqsTransport()
        {
            Dispose();
        }
    }
}
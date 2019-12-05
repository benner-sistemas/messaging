using Benner.Messaging.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Benner.Messaging.Core
{
    public class MessagingController : ControllerBase
    {
        private readonly IMessagingConfig _messagingConfig;

        public MessagingController()
        {
            // carregar da linha de comando
            // senão, carregar do arquivo
            // senão, deixar nulo
            _messagingConfig = new FileMessagingConfig();
        }

        protected virtual string QueueName
        {
            get { return this.ControllerContext.ActionDescriptor.ControllerName.ToLower(); }
        }

        protected virtual IMessagingConfig MessagingConfig
        {
            get { return _messagingConfig; }
        }

        protected ActionResult<IEnterpriseIntegrationResponse> Enqueue(IEnterpriseIntegrationResquest request)
        {
            var message = EnterpriseIntegrationMessage.Create(request);

            Messaging.Enqueue(
                QueueName,
                message,
                MessagingConfig);

            var response = new EnterpriseIntegrationResponse
            {
                MessageID = new Guid(message.MessageID),
                QueueName = QueueName,
                CreatedAt = DateTime.Now,
            };

            return new OkObjectResult(response);
        }
    }
}
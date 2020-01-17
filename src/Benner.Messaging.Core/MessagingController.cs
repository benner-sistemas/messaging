using Benner.Messaging.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Benner.Messaging.Core
{
    public abstract class MessagingController : ControllerBase
    {
        private readonly IMessagingConfig _messagingConfig;

        protected MessagingController()
        {
            _messagingConfig = new FileMessagingConfig();
        }

        protected abstract string QueueName { get; }

        protected virtual IMessagingConfig MessagingConfig
        {
            get { return _messagingConfig; }
        }

        protected ActionResult<IEnterpriseIntegrationResponse> Enqueue(IEnterpriseIntegrationResquest request)
        {
            /*
            this.Authenticate(token);
            var json = OpenIDConnectService.GetRawJson(accessToken);
            var userInfo = OidcUserInfo.GetOidcUserInfo(json);
            */
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
using Benner.Messaging.Interfaces;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;

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
            ValidateAuthentication();

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

        private void ValidateAuthentication()
        {
            //Microsoft.Extensions.Primitives.StringValues tokenValue;
            //if (!Request.Headers.TryGetValue("Authorization", out tokenValue))
            //    throw new UnauthorizedAccessException("Token é nulo ou não é do tipo Bearer");
            //
            //string token = tokenValue[0]
            //
            //if (token.StartsWith("Bearer"))
            //    token = token.Remove(0, 7);
            //else
            //    throw new UnauthorizedAccessException("Token é nulo ou não é do tipo Bearer");
            //
            //var userAddress = "http://bnu-vtec012:7600/auth/realms/master/protocol/openid-connect/userinfo";
            //var tokenRequest = new UserInfoRequest
            //{
            //    Address = userAddress,
            //    Token = token,
            //};
            //var _authClient = new HttpClient();
            //var tokenResponse = _authClient.GetUserInfoAsync(tokenRequest).Result;
            //
            //if (tokenResponse.IsError)
            //    throw new UnauthorizedAccessException();
        }
    }
}
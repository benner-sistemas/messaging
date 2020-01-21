using Benner.Messaging.Common;
using Benner.Messaging.Interfaces;
using Benner.Producer.Configuration;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;

namespace Benner.Messaging.Core
{
    public abstract class MessagingController : ControllerBase
    {
        private readonly IMessagingConfig _messagingConfig;
        private static readonly HttpClient _httpClient = new HttpClient();

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
            var authenticationResult = ValidateAuthentication();
            if (!authenticationResult.Success)
                return Unauthorized(authenticationResult);

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

        private dynamic ValidateAuthentication()
        {
            Microsoft.Extensions.Primitives.StringValues tokenValue;
            if (!Request.Headers.TryGetValue("Authorization", out tokenValue))
                return new { Success = false, Message = "Authorization header not found" };

            string token = tokenValue[0];

            if (token.StartsWith("Bearer"))
                token = token.Remove(0, 7);
            else
                return new { Success = false, Message = "Authorization header with Bearer scheme not found" };

            var configuration = JsonConfiguration.LoadConfiguration<ProducerJson>();
            
     
            var tokenRequest = new UserInfoRequest
            {
                Address = configuration.Oidc.UserInfoEndpoint,
                Token = token,
            };
            var tokenResponse = _httpClient.GetUserInfoAsync(tokenRequest).Result;

            if (tokenResponse.IsError)
                return new { Success = false, Message = tokenResponse.Error };

            return new { Success = true };
        }
    }
}
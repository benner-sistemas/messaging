using Benner.Messaging.Configuration;
using Benner.Messaging.Interfaces;
using Benner.Messaging.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
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

        protected ActionResult<IEnterpriseIntegrationResponse> Enqueue(IEnterpriseIntegrationRequest request)
        {
            //var authenticationResult = ValidateAuthentication();
            //if (!authenticationResult.Success)
            //    return Unauthorized(authenticationResult);
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
            Log.Information("Request com id {@request} enviado para fila {queueName}", request.RequestID, QueueName);
            return new OkObjectResult(response);
        }

        private dynamic ValidateAuthentication()
        {
            if (!Request.Headers.TryGetValue("Authorization", out StringValues tokenValue))
                return new { Success = false, Message = "Authorization header not found" };

            string token = tokenValue[0];

            if (token.StartsWith("Bearer"))
                token = token.Remove(0, 7);
            else
                return new { Success = false, Message = "Authorization header with Bearer scheme not found" };

            var configuration = JsonConfiguration.LoadConfiguration<ProducerJson>();


            //var tokenRequest = new UserInfoRequest
            //{
            //    Address = configuration.Oidc.UserInfoEndpoint,
            //    Token = token,
            //};
            //var tokenResponse = _httpClient.GetUserInfoAsync(tokenRequest).Result;

            //if (tokenResponse.IsError)
            //    return new { Success = false, Message = tokenResponse.Error };

            return new { Success = true };
        }
    }
}
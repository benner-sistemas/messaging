using Benner.Messaging;
using Benner.Messaging.Configuration;
using Benner.Messaging.Interfaces;
using Benner.Messaging.Logger;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Net.Http;

namespace Benner.Enterprise.Integration.Messaging
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
            var authenticationResult = ValidateBearerToken();
            if (!authenticationResult.Success)
                return Unauthorized(authenticationResult);

            var message = EnterpriseIntegrationMessage.Create(request);
            Benner.Messaging.Messaging.Enqueue(
                QueueName,
                message,
                MessagingConfig);

            var response = new EnterpriseIntegrationResponse
            {
                MessageID = new Guid(message.MessageID),
                QueueName = QueueName,
                CreatedAt = DateTime.Now,
            };
            Log.Information("{id} @{queueName}", request.RequestID, QueueName);
            return new ObjectResult(response) { StatusCode = 201 };
        }

        private dynamic ValidateBearerToken()
        {
            if (!Request.Headers.TryGetValue("Authorization", out StringValues tokenValue))
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
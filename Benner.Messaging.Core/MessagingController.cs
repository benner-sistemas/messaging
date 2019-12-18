using Benner.Messaging.CLI;
using Benner.Messaging.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Benner.Messaging.Core
{
    public class MessagingController : ControllerBase
    {
        private readonly IMessagingConfig _messagingConfig;

        public MessagingController()
        {
            // quando tem linha de comando, o primeiro argumento é um path
            try
            {
                var args = Environment.GetCommandLineArgs().ToList();
                if (args.Count > 1)
                {
                    if (System.IO.File.Exists(args[0]))
                        args.RemoveAt(0);

                    var parser = CliParserFactory.CreateForProducer(args.ToArray());
                    parser.Parse();

                    _messagingConfig = parser.Configuration;
                }
                else
                    _messagingConfig = new FileMessagingConfig();
            }
            catch (Exception)
            {
                _messagingConfig = null;
            }
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
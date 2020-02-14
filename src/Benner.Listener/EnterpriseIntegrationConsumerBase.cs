using Benner.Messaging.Configuration;
using Benner.Messaging.Logger;
using System;

namespace Benner.Enterprise.Integration.Messaging
{
    public abstract class EnterpriseIntegrationConsumerBase : IEnterpriseIntegrationConsumer
    {
        public abstract IEnterpriseIntegrationSettings Settings { get; }

        public abstract void OnDeadMessage(string message, Exception exception);

        public abstract void OnInvalidMessage(string message, InvalidMessageException exception);

        public abstract void OnMessage(string message);

        protected virtual T DeserializeMessage<T>(string message)
        {
            return JsonParser.Deserialize<T>(message);
        }

        protected void LogInformation(string message)
        {
            Log.Information(message);
        }

        protected void LogInformation(string message, params object[] propertyValues)
        {
            Log.Information(message, propertyValues);
        }

        protected void LogError(Exception exception, string message)
        {
            Log.Error(exception, message);
        }

        protected void LogError(Exception exception, string message, params object[] propertyValues)
        {
            Log.Error(exception, message, propertyValues);
        }
    }
}
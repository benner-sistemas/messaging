using Benner.Listener;
using System;
using System.Collections.Generic;
using System.Text;

namespace Benner.Consumer.Core
{
    // dar uma relida aqui:
    // http://wiki.benner.com.br/wiki/index.php?title=Inje%C3%A7%C3%A3o_de_depend%C3%AAncia_no_BEF
    // http://www.ninject.org/
    //public class IOC
    //{
    //    public void RegisterMessagingModules(IKernel kernel, string queueName)
    //    {
    //        //kernel
    //        kernel.Bind<IEnterpriseIntegrationConsumer>(ContabilizacaoConsumer)
    //            .When(queueName == "contabil");
    //        kernel.Bind<IEnterpriseIntegrationConsumer>(DccumentoFinancConsumer)
    //            .When(queueName == "financ");
    //        kernel.Bind<IEnterpriseIntegrationConsumer>(BlahConsumer)
    //            .When(queueName == "blah");
    //        kernel.Bind<IEnterpriseIntegrationConsumer>(ContabilizacaoConsumer)
    //            .When(queueName == "contabil");
    //    }
    //}
    //[QueueNameAttribute("contabil")]
    public class ContabilizacaoConsumer : IEnterpriseIntegrationConsumer
    {
        public IEnterpriseIntegrationSettings Settings => new EnterpriseIntegrationSettings()
        {
            QueueName = "contabilizacao",
            RetryIntervalInMilliseconds = 1000 * 60,
            RetryLimit = 10,
        };

        public void OnDeadMessage(object message)
        {
            throw new NotImplementedException();
        }

        public void OnInvalidMessage(object message)
        {
            throw new NotImplementedException();
        }

        public void OnMessage(object message)
        {
            throw new NotImplementedException();
        }
    }
}

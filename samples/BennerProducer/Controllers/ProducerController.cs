using Benner.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace BennerProducer.Controllers
{
    [Route("producer")]
    [ApiController]
    public class ProducerController : ControllerBase
    {
        // POST: producer/adicionarMensagemFerias
        [HttpPost]
        [AcceptVerbs("POST")]
        [Route("adicionarMensagemFerias")]
        public void AdicionarMensagemFerias(string mensagem)
        {
            var teste = EnterpriseIntegrationMessage.Create(null);

            Messaging.Enqueue("mensagemFerias", teste, Program.BrokerConnection);
        }
    }
}

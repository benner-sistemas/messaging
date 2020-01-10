using Benner.ERP.API.Models;
using Benner.Messaging;
using Benner.Messaging.Core;
using Microsoft.AspNetCore.Mvc;

namespace Benner.ERP.API.Controllers
{
    [Route("producer")]
    [ApiController]
    public class FeriasController : MessagingController
    {
        protected override string QueueName { get => "ferias-producer"; }

        // POST: producer/adicionarMensagemFerias
        [HttpPost]
        [AcceptVerbs("POST")]
        [Route("adicionarMensagemFerias")]
        public ActionResult<IEnterpriseIntegrationResponse> AdicionarMensagemFerias([FromBody] FeriasRequest request)
        {
            return base.Enqueue(request as IEnterpriseIntegrationResquest);
        }
    }
}

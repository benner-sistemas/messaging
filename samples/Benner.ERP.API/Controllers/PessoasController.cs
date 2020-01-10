using Benner.ERP.Models;
using Benner.Messaging;
using Benner.Messaging.Core;
using Microsoft.AspNetCore.Mvc;

namespace Benner.ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoasController : MessagingController
    {
        protected override string QueueName { get => "fila-pessoa-consumer"; }

        // POST api/pessoas
        [HttpPost]
        public ActionResult<IEnterpriseIntegrationResponse> Post([FromBody] PessoaRequest request)
        {
            return base.Enqueue(request as IEnterpriseIntegrationResquest);
        }
    }
}
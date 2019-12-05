using Benner.ERP.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace Benner.ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoasController : ControllerBase
    {
        // POST api/pessoas
        [HttpPost]
        public void Post([FromBody] PessoaRequest value)
        {
            Debugger.Launch();
            Debugger.Break();
            Console.WriteLine(value);
        }
    }
}
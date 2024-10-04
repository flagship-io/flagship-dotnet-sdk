using Flagship;
using Flagship.Model.Config;
using Flagship.Model.Decision;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace QAApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VisitorController : ControllerBase
    {
        private static Model.Visitor currentVisitor = new Model.Visitor();
        public static IFlagshipVisitor Visitor;

        public VisitorController()
        {
        }

        [HttpGet]
        public Model.Visitor Get()
        {
            return currentVisitor;
        }

        private object ToObject(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.ToString();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();
                case JsonValueKind.Number:
                    return element.GetDouble();
                default:
                    return element.ToString();
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(Model.Visitor newVisitor)
        {
            if (EnvController.Client == null)
            {
                return BadRequest(new
                {
                    error = "SDK Client not initialized"
                });
            }
            currentVisitor = newVisitor;
            var context = new Dictionary<string, object>();
            foreach (var item in newVisitor.Context)
            {
                context.Add(item.Key, ToObject(item.Value));
            }

            Visitor = EnvController.Client.NewVisitor(newVisitor.Id, context);
            await Visitor.SynchronizeModifications().ConfigureAwait(false);

            var modifs = Visitor.GetAllModifications();
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(modifs), "application/json");
        }
    }
}

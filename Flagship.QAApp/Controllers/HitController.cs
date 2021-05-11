using Flagship.Model.Hits;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace QAApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HitController : ControllerBase
    {
        public HitController()
        {
        }
        private T GetObject<T>(JsonElement dict)
        {
            var raw = dict.GetRawText();
            return JsonConvert.DeserializeObject<T>(raw);
        }

        [HttpPost]
        public async Task<IActionResult> Send(
            JsonElement hit)
        {
            BaseHit hitObj = null;
            try
            {
                var enumType = (HitType)Enum.Parse(typeof(HitType), hit.GetProperty("t").GetString());
                switch (enumType)
                {
                    case HitType.PAGEVIEW:
                        hitObj = GetObject<Pageview>(hit);
                        break;
                    case HitType.SCREENVIEW:
                        hitObj = GetObject<Screenview>(hit);
                        break;
                    case HitType.EVENT:
                        hitObj = GetObject<Event>(hit);
                        break;
                    case HitType.TRANSACTION:
                        hitObj = GetObject<Transaction>(hit);
                        break;
                    case HitType.ITEM:
                        hitObj = GetObject<Item>(hit);
                        break;
                }

                await VisitorController.Visitor.SendHit(enumType, hitObj);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    error = e.Message,
                });
            }
        }

    }
}

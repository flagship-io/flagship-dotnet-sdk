using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace QAApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlagController : ControllerBase
    {
        private readonly ILogger<FlagController> _logger;

        public FlagController(ILogger<FlagController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("{name}")]
        public IActionResult Get(
            string name,
            [FromQuery(Name = "type")] string type,
            [FromQuery(Name = "activate")] bool activate,
            [FromQuery(Name = "defaultValue")] string defaultValue)
        {
            object flag = null;
            var error = "";
            switch (type)
            {
                case "string":
                    flag = VisitorController.Visitor.GetModification(name, defaultValue, activate);
                    break;
                case "bool":
                    bool boolVal;
                    if (bool.TryParse(defaultValue, out boolVal))
                    {
                        flag = VisitorController.Visitor.GetModification(name, boolVal, activate);
                    }
                    else
                    {
                        error = $"Default value {defaultValue} is not of type {type}";
                    }
                    break;
                case "number":
                    double doubleVal;
                    if (double.TryParse(defaultValue, out doubleVal))
                    {
                        flag = VisitorController.Visitor.GetModification(name, doubleVal, activate);
                    }
                    else
                    {
                        error = $"Default value {defaultValue} is not of type {type}";
                    }
                    break;
                case "array":
                    try
                    {
                        var arrayVal = JsonConvert.DeserializeObject<List<object>>(defaultValue);
                        flag = VisitorController.Visitor.GetModification(name, arrayVal, activate);
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                    }
                    break;
                case "object":
                    try
                    {
                        var objectVal = JsonConvert.DeserializeObject<object>(defaultValue);
                        flag = VisitorController.Visitor.GetModification(name, objectVal, activate);
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                    }
                    break;
                default:
                    return BadRequest(new
                    {
                        error = $"Type {type} not handled",
                    });
            }

            if (error != "")
            {
                return BadRequest(new
                {
                    error = error,
                });
            }

            return Content(JsonConvert.SerializeObject(new
            {
                value = flag
            }), "application/json");
        }

        [HttpGet]
        [Route("{name}/info")]
        public IActionResult GetInfos(string name)
        {
            var infos = VisitorController.Visitor.GetModificationInfo(name);
            return Ok(new
            {
                value = infos,
                error = infos == null ? "Flag key not found" : null
            });
        }
    }
}

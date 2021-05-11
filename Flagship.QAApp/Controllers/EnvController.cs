using Flagship;
using Flagship.Model.Config;
using Flagship.Model.Decision;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.Logger;
using Microsoft.AspNetCore.Mvc;
using QAApp.Model;

namespace QAApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnvController : ControllerBase
    {
        private static Environment currentEnv = new Environment();
        public static IFlagshipClient Client;

        public EnvController()
        {
        }

        [HttpGet]
        public Environment Get()
        {
            return currentEnv;
        }

        [HttpPut]
        public IActionResult Put(Environment newEnv)
        {
            currentEnv = newEnv;
            var logger = new DefaultLogger();
            var exceptionHandler = new DefaultExceptionHandler(logger, true);
            var builder = new FlagshipOptions.Builder()
                .WithErrorHandler(exceptionHandler)
                .WithDecisionMode(newEnv.Bucketing ? Mode.Bucketing : Mode.API)
                .WithBucketingOptions(System.TimeSpan.FromMilliseconds(newEnv.PollingInterval));
            if (newEnv.Timeout > 0)
            {
                builder.WithAPIOptions(System.TimeSpan.FromSeconds(newEnv.Timeout));
            }
            Client = Flagship.FlagshipBuilder.Start(
                newEnv.Id,
                newEnv.ApiKey,
                builder.Build()
                );

            return Ok(currentEnv);
        }
    }
}

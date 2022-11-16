using Flagship.Main;
using Microsoft.AspNetCore.Mvc;

namespace Test_asp.net_core.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var visitor = Fs.NewVisitor().WithContext(new Dictionary<string, object>()
            {
                {"qa_report", true },
                {"is_net", true }
            }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("qa_report_var", "default");
            var flagValue = flag.GetValue();

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                FlagValue = flagValue
            })
            .ToArray();
        }
    }
}
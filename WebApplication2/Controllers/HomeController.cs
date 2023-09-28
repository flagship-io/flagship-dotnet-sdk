using Flagship.FsVisitor;
using Flagship.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<Visitor> GetFsVisitor()
        {
            var visitorId = "visitorId";
            var initialFlagsJson = HttpContext.Session.GetString(visitorId);
            Flagship.FsVisitor.Visitor fsVisitor;


            if (string.IsNullOrEmpty(initialFlagsJson))
            {
                fsVisitor = Flagship.Main.Fs.NewVisitor(visitorId).Build();
                await fsVisitor.FetchFlags();
                var flags = fsVisitor.GetFlagDTOs();
                initialFlagsJson = JsonConvert.SerializeObject(flags);
                HttpContext.Session.SetString(visitorId, initialFlagsJson);
            }
            else
            {
                var initialFlags = JsonConvert.DeserializeObject<ICollection<FlagDTO>>(initialFlagsJson) ;
                fsVisitor = Flagship.Main.Fs.NewVisitor(visitorId)
                    .WithInitialFlagsData(initialFlags)
                    .Build();
            }
            return fsVisitor;
        }
        public async Task<IActionResult> Index()
        {
            var fsVisitor = await GetFsVisitor();

            var flag = fsVisitor.GetFlag("btnColor", "default");

            var model = new { FlagValue = flag.GetValue() };

            return View( model);
        }

        public async Task<IActionResult> Privacy()
        {
            var fsVisitor = await GetFsVisitor();

            var flag = fsVisitor.GetFlag("btnColor", "default");

            var model = new { FlagValue = flag.GetValue() };
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
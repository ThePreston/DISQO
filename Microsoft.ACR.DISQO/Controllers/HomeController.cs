using Microsoft.ACR.DISQO.Models;
using Microsoft.ACR.DISQO.Service.ContainerService;
using Microsoft.ACR.DISQO.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.ACR.DISQO.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;

        private readonly IConfiguration _config;

        private readonly IContainerService _service;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IContainerService service)
        {
            _logger = logger;
            _config = configuration;
            _service = service;

        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<bool> PassQuarantine(string repository, string manifestId, bool pass = true)
        {
            return await _service.PassQuarantine(repository, manifestId, pass);
        }

        public async Task<string> SetServerQuarantine(string repository, bool enabled)
        {
            string retVal;
            try
            {

                retVal = await _service.UpdateQuarantineStatus(repository, enabled);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw ex;
            }

            return retVal;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<JsonResult> Search(string searchParam)
        {

            await new TaskFactory().StartNew(() => _logger.LogInformation($"Entered Azure Search = {searchParam}"));

            return new JsonResult(await _service.GetRepositoryManifests(new ACRModel(searchParam)));

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

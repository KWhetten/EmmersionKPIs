using DataWrapper.DatabaseAccess;
using KPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OverviewController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public OverviewController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public OverviewData Get()
        {
            var dataAccess = new DataAccess();
            var workItemCardList = dataAccess.GetWorkItemCardList();
            var releaseList = dataAccess.GetReleaseList();
            var data = new OverviewData(workItemCardList, releaseList);
            return data;
        }
    }
}

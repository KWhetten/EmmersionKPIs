using System;
using System.Threading.Tasks;
using KPIWebApp.Helpers;
using KPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OverviewController : ControllerBase
    {
        [HttpGet]
        public async Task<OverviewData> Get(string startDateString, string finishDateString)
        {
            var overviewHelper = new OverviewHelper();

            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString);

            return await overviewHelper.GetOverviewData(startDate, finishDate);
        }
    }
}

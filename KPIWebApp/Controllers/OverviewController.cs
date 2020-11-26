using System;
using System.Threading.Tasks;
using KPIWebApp.Helpers;
using KPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("overview")]
    public class OverviewController : ControllerBase
    {
        [HttpGet]
        public async Task<OverviewData> Get(string startDateString, string finishDateString, bool product, bool engineering, bool unanticipated)
        {
            var overviewHelper = new OverviewHelper();

            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString);

            var result = await overviewHelper.GetOverviewDataAsync(startDate, finishDate, product, engineering, unanticipated);

            return result;
        }
    }
}

using System;
using System.Threading.Tasks;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("lead-time")]
    public class LeadTimeController : ControllerBase
    {
        [HttpGet]
        public async Task<ScatterPlotData[]> Get(string startDateString, string finishDateString)
        {
            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString);

            var scatterPlotHelper = new ScatterPlotHelper();
            return await scatterPlotHelper.GetLeadTimeScatterPlotData(startDate, finishDate);
        }
    }
}

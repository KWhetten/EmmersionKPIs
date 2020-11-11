using System;
using System.Threading.Tasks;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("lead-time-scatter")]
    public class LeadTimeController : ControllerBase
    {
        [HttpGet]
        public ScatterPlotData[] Get(string startDateString, string finishDateString)
        {
            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString);

            var scatterPlotHelper = new ScatterPlotHelper();
            return  scatterPlotHelper.GetLeadTimeScatterPlotData(startDate, finishDate);
        }
    }
}

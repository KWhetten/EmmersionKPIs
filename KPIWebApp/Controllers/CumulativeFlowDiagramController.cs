using System.Collections.Generic;
using System.Threading.Tasks;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("cumulative-flow")]
    public class CumulativeFlowDiagramController
    {
        [HttpGet]
        public async Task<CumulativeFlowData> Get(string startDateString, string finishDateString)
        {
            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString);

            var cumulativeFlowDiagramHelper = new CumulativeFlowDiagramHelper();

            var data =  await cumulativeFlowDiagramHelper.GetCumulativeFlowData(startDate, finishDate);

            return data;
        }
    }
}

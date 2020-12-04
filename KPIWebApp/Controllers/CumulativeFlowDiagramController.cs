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
        public async Task<CumulativeFlowData> Get(string startDateString, string finishDateString, bool product, bool engineering, bool unanticipated)
        {
            var startDate = DateHelper.GetStartDate(startDateString).Date;
            var finishDate = DateHelper.GetFinishDate(finishDateString).Date;

            var cumulativeFlowDiagramHelper = new CumulativeFlowDiagramHelper();

            var data =  await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(startDate, finishDate, product, engineering, unanticipated);

            return data;
        }
    }
}

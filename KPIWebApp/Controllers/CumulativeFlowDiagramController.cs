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
        public async Task<CumulativeFlowData> Get(string startDateString, string finishDateString, bool product, bool engineering, bool unanticipated, bool assessmentsTeam, bool enterpriseTeam)
        {
            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString);

            var cumulativeFlowDiagramHelper = new CumulativeFlowDiagramHelper();

            var data =  await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(startDate, finishDate, product, engineering, unanticipated, assessmentsTeam, enterpriseTeam);

            return data;
        }
    }
}

using System.Threading.Tasks;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("releases-bar-graph")]
    public class ReleasesBarGraphController : ControllerBase
    {
        [HttpGet]
        public async Task<BarGraphData> Get(string startDateString, string finishDateString,
            bool assessmentsTeam, bool enterpriseTeam)
        {
            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString);

            var barGraphHelper = new BarGraphHelper();
            return await barGraphHelper.GetReleaseBarGraphData(startDate, finishDate, assessmentsTeam, enterpriseTeam);
        }
    }
}

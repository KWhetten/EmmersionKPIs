using System.Threading.Tasks;
using KPIWebApp.Helpers;
using KPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("release-overview")]
    public class ReleaseOverviewController : ControllerBase
    {
        [HttpGet]
        public async Task<ReleaseOverviewData> Get(string startDateString, string finishDateString, bool product, bool engineering, bool unanticipated)
        {
            var overviewHelper = new OverviewHelper();

            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString);

            var result = await overviewHelper.GetReleaseOverviewDataAsync(startDate, finishDate, product, engineering, unanticipated);

            return result;
        }
    }
}

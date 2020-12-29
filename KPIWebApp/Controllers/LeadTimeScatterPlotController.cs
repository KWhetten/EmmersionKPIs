using System.Threading.Tasks;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("lead-time-scatter")]
    public class LeadTimeController : ControllerBase
    {
        [HttpGet]
        public async Task<ScatterPlotData[]> Get(string startDateString, string finishDateString,
            bool product, bool engineering, bool unanticipated,
            bool assessmentsTeam, bool enterpriseTeam)
        {
            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString).AddDays(1);

            var scatterPlotHelper = new ScatterPlotHelper();
            var result = await scatterPlotHelper
                .GetLeadTimeScatterPlotData(startDate, finishDate, product, engineering, unanticipated, assessmentsTeam, enterpriseTeam);

            var returning = new ScatterPlotData[result.Count];

            for (var i = 0; i < result.Count; i++)
            {
                if (product)
                {
                    returning[i] = result[TaskItemType.Product];
                    product = false;
                }
                else if (engineering)
                {
                    returning[i] = result[TaskItemType.Engineering];
                    engineering = false;
                }
                else if (unanticipated)
                {
                    returning[i] = result[TaskItemType.Unanticipated];
                    unanticipated = false;
                }
            }

            return returning;
        }
    }
}

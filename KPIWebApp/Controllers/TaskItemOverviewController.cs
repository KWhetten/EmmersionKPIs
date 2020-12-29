using System.Threading.Tasks;
using KPIWebApp.Helpers;
using KPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("task-item-overview")]
    public class TaskItemOverviewController : ControllerBase
    {
        [HttpGet]
        public async Task<TaskItemOverviewData> Get(string startDateString, string finishDateString,
            bool product, bool engineering, bool unanticipated, bool assessmentsTeam, bool enterpriseTeam)
        {
            var overviewHelper = new OverviewHelper();

            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString);

            var result = await overviewHelper
                .GetTaskItemOverviewDataAsync(startDate, finishDate, product, engineering, unanticipated,
                    assessmentsTeam, enterpriseTeam);

            return result;
        }
    }
}

﻿using System.Threading.Tasks;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("lead-time-box")]
    public class LeadTimeBoxGraphController : ControllerBase
    {
        [HttpGet]
        public async Task<BoxGraphData> Get(string startDateString, string finishDateString,
            bool product, bool engineering, bool unanticipated,
            bool assessmentsTeam, bool enterpriseTeam)
        {
            var startDate = DateHelper.GetStartDate(startDateString);
            var finishDate = DateHelper.GetFinishDate(finishDateString);

            var boxGraphHelper = new BoxGraphHelper();
            return await boxGraphHelper.GetLeadTimeBoxGraphData(startDate, finishDate, product, engineering, unanticipated, assessmentsTeam, enterpriseTeam);
        }
    }
}

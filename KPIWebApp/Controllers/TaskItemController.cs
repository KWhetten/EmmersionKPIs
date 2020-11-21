using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("work-item-card-data")]
    public class TaskItemController : ControllerBase
    {
        [HttpGet]
        public async Task<List<TaskItem>> Get(string startDateString, string endDateString)
        {
            var taskItemHelper = new TaskItemHelper();
            var startDate = DateHelper.GetStartDate(startDateString);
            var endDate = DateHelper.GetStartDate(endDateString);

            return await taskItemHelper.GetTaskItems(startDate, endDate);
        }
    }
}

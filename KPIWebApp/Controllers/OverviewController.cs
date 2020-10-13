using System;
 using System.Threading.Tasks;
 using DataAccess.DataRepositories;
 using KPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OverviewController : ControllerBase
    {
        private readonly DateTime startDateDefault = new DateTime(2000, 01, 01);

        [HttpGet]
        public async Task<OverviewData> Get(string startDateString, string endDateString)
        {

            var startDate = startDateDefault;
            var endDate = DateTime.Today;
            try
            {
                startDate = Convert.ToDateTime(startDateString);
                endDate = Convert.ToDateTime(endDateString);
            }
            catch (Exception ex)
            {
                // ignored
            }

            endDate = endDate.AddDays(1);

            if (startDate == DateTime.MinValue)
            {
                startDate = startDateDefault;
            }
            if (endDate == DateTime.MinValue.AddDays(1))
            {
                endDate = DateTime.Now;
            }

            var taskItemData = new TaskItemRepository(new DatabaseConnection());
            var releaseData = new ReleaseRepository(new DatabaseConnection());

            var taskItemList = await taskItemData.GetTaskItemListAsync(startDate, endDate);
            var releaseList = await releaseData.GetReleaseListAsync(startDate, endDate);

            return new OverviewData(taskItemList, releaseList, startDate, endDate);
        }
    }
}

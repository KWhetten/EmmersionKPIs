using System;
using DataManipulation.DatabaseAccess;
using KPIWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OverviewController : ControllerBase
    {
        [HttpGet]
        public OverviewData Get(string startDateString, string endDateString)
        {
            var startDate = new DateTime(2015, 01, 01);
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
                startDate = new DateTime(2015, 01, 01);
            }
            if (endDate == DateTime.MinValue.AddDays(1))
            {
                endDate = DateTime.Now;
            }

            var dataAccess = new DatabaseWrapper();

            var workItemCardList = dataAccess.GetWorkItemCardList(startDate, endDate);
            var releaseList = dataAccess.GetReleaseList(startDate, endDate);

            return new OverviewData(workItemCardList, releaseList);
        }
    }
}

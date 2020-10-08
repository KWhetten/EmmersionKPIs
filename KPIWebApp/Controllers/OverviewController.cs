﻿using System;
using DataAccess.DatabaseAccess;
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
        public OverviewData Get(string startDateString, string endDateString)
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

            var TaskItemData = new TaskItemRepository();
            var releaseData = new ReleaseRepository();

            var TaskItemList = TaskItemData.GetTaskItemList(startDate, endDate);
            var releaseList = releaseData.GetReleaseList(startDate, endDate);

            return new OverviewData(TaskItemList, releaseList, startDate, endDate);
        }
    }
}

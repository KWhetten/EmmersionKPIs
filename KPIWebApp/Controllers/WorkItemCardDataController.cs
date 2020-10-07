﻿using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.DatabaseAccess;
using DataObjects.Objects;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("work-item-card-data")]
    public class WorkItemCardDataController : ControllerBase
    {
        private readonly WorkItemCardDataAccess workItemCardDataAccess;
        private readonly DateTime startDateDefault = new DateTime(2000, 01, 01);

        public WorkItemCardDataController()
        {
            workItemCardDataAccess = new WorkItemCardDataAccess();
        }

        // USED FOR TESTING
        // public WorkItemCardDataController(WorkItemCardDataAccess workItemCardDataAccess)
        // {
        //     this.workItemCardDataAccess = workItemCardDataAccess;
        // }

        [HttpGet]
        public WorkItemCard[] Get(string startDateString, string endDateString)
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
            var workItemCards = workItemCardDataAccess.GetWorkItemCardList(startDate, endDate);

            var badWorkItemCards = workItemCards.Where(workItemCard => workItemCard.FinishTime == DateTime.MaxValue).ToList();

            foreach (var badWorkItemCard in badWorkItemCards)
            {
                workItemCards.Remove(badWorkItemCard);
            }

            return workItemCards.ToArray();
        }
    }
}

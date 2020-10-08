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
    public class TaskItemDataController : ControllerBase
    {
        private readonly TaskItemRepository TaskItemRepository;
        private readonly DateTime startDateDefault = new DateTime(2000, 01, 01);

        public TaskItemDataController()
        {
            TaskItemRepository = new TaskItemRepository();
        }

        // USED FOR TESTING
        // public TaskItemDataController(TaskItemDataAccess TaskItemDataAccess)
        // {
        //     this.TaskItemDataAccess = TaskItemDataAccess;
        // }

        [HttpGet]
        public TaskItem[] Get(string startDateString, string endDateString)
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
            var TaskItems = TaskItemRepository.GetTaskItemList(startDate, endDate);

            var badTaskItems = TaskItems.Where(TaskItem => TaskItem.FinishTime == DateTime.MaxValue).ToList();

            foreach (var badTaskItem in badTaskItems)
            {
                TaskItems.Remove(badTaskItem);
            }

            return TaskItems.ToArray();
        }
    }
}

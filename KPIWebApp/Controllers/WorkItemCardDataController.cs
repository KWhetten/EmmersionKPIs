using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DatabaseAccess;
using DataAccess.DataRepositories;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("work-item-card-data")]
    public class TaskItemDataController : ControllerBase
    {
        private readonly TaskItemRepository taskItemRepository;
        private readonly DateTime startDateDefault = new DateTime(2000, 01, 01);

        public TaskItemDataController()
        {
            taskItemRepository = new TaskItemRepository(new DatabaseConnection());
        }

        // USED FOR TESTING
        // public TaskItemDataController(TaskItemDataAccess TaskItemDataAccess)
        // {
        //     this.TaskItemDataAccess = TaskItemDataAccess;
        // }

        [HttpGet]
        public async Task<TaskItem[]> Get(string startDateString, string endDateString)
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
            var taskItems = await taskItemRepository.GetTaskItemListAsync(startDate, endDate);

            var badTaskItems = taskItems.Where(taskItem => taskItem.FinishTime == DateTime.MaxValue).ToList();

            foreach (var badTaskItem in badTaskItems)
            {
                taskItems.Remove(badTaskItem);
            }

            return taskItems.ToArray();
        }
    }
}

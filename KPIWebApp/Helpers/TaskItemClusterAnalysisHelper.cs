using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;

namespace KPIWebApp.Helpers
{
    public class TaskItemClusterAnalysisHelper
    {
        private readonly ITaskItemRepository taskItemRepository = new TaskItemRepository();

        public async Task<List<TaskItem>> GetTaskItems(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var taskItems = await taskItemRepository.GetTaskItemListAsync(startDate, endDate);

            return taskItems;
        }
    }
}

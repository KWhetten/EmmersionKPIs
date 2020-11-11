using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;

namespace KPIWebApp.Helpers
{
    public class TaskItemHelper
    {
        private readonly ITaskItemRepository taskItemRepository;

        public TaskItemHelper()
        {
            taskItemRepository = new TaskItemRepository();
        }
        public TaskItemHelper(ITaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
        }

        public async Task<TaskItem[]> GetTaskItems(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var taskItems = await taskItemRepository.GetTaskItemListAsync(startDate, endDate);

            var badTaskItems = taskItems.Where(taskItem => taskItem.FinishTime == null).ToList();

            foreach (var badTaskItem in badTaskItems)
            {
                taskItems.Remove(badTaskItem);
            }

            return taskItems.ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
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

        public async Task<List<TaskItem>> GetTaskItems(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var taskItems = await taskItemRepository.GetTaskItemListAsync(startDate, endDate);

            var badTaskItems = taskItems.Where(taskItem => taskItem.FinishTime == null).ToList();

            foreach (var badTaskItem in badTaskItems)
            {
                taskItems.Remove(badTaskItem);
            }

            return taskItems;
        }

        public bool TaskItemDevTeamIsSelected(bool assessmentsTeam, bool enterpriseTeam, TaskItem item)
        {
            return item.DevelopmentTeam == "Assessments Team" && assessmentsTeam
                   || item.DevelopmentTeam == "Enterprise Team" && enterpriseTeam;
        }

        public bool TaskItemTypeIsSelected(bool product, bool engineering, bool unanticipated, TaskItem item)
        {
            var result = item.Type == TaskItemType.Product && product
                         || item.Type == TaskItemType.Engineering && engineering
                         || item.Type == TaskItemType.Unanticipated && unanticipated;
            return result;
        }
    }
}

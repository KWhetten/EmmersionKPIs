using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.ApiWrapper;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using Newtonsoft.Json.Linq;

namespace DataAccess.Deserialize
{
    public interface IKanbanizeDeserializer
    {
        Task<IEnumerable<TaskItem>> TaskItemListAsync(IEnumerable<JToken> jsonTaskItems, int boardId);
        Task<TaskItem> TaskItemAsync(JToken jsonTaskItem, int boardId);
        TaskItem TaskItemHistoryItems(JToken jsonTaskItem, TaskItem taskItem, int boardId);
        TaskItemType GetCardType(JToken workItem);
    }

    public class KanbanizeDeserializer : IKanbanizeDeserializer
    {
        private readonly IKanbanizeApiRepository kanbanizeApiRepository;
        private readonly ReleaseRepository releaseRepository = new ReleaseRepository(new DatabaseConnection());

        public KanbanizeDeserializer(IKanbanizeApiRepository kanbanizeApiRepository)
        {
            this.kanbanizeApiRepository = kanbanizeApiRepository;
        }

        public async Task<IEnumerable<TaskItem>> TaskItemListAsync(IEnumerable<JToken> jsonTaskItems, int boardId)
        {
            var list = new List<TaskItem>();
            foreach (var item in jsonTaskItems)
            {
                var taskItemRepository = new TaskItemRepository(new DatabaseConnection());
                var cardState = "";
                try
                {
                    cardState = taskItemRepository.GetTaskItemByIdAsync((int) item["taskid"]).Result.CardState;
                }
                catch (Exception ex)
                {
                    // ignored
                }

                if (cardState != "Archived")
                {
                     list.Add(await TaskItemAsync(item, boardId));
                }
            }

            return list;
        }

        public async Task<TaskItem> TaskItemAsync(JToken jsonTaskItem, int boardId)
        {
            var taskItem = new TaskItem
            {
                Id = (int) jsonTaskItem["taskid"],
                Title = jsonTaskItem["title"].ToString(),
                StartTime = DateTime.MinValue,
                FinishTime = DateTime.MaxValue,
                Type = GetCardType(jsonTaskItem),
                DevelopmentTeamName = boardId == 4
                    ? "Enterprise Team"
                    : "Assessment Team",
                CreatedOn = ((DateTime) jsonTaskItem["createdat"]).ToUniversalTime(),
                CreatedBy = jsonTaskItem["reporter"].ToString(),
                LastChangedOn = ((DateTime) jsonTaskItem["updatedat"]).ToUniversalTime(),
                LastChangedBy = "",
                CurrentBoardColumn = jsonTaskItem["columnname"].ToString(),
                CardState = GetCardState(jsonTaskItem),
                Impact = jsonTaskItem["priority"]?.ToString(),
                CommentCount = jsonTaskItem["comments"].ToString() == ""
                    ? 0
                    : int.Parse(jsonTaskItem["comments"].Count().ToString()),
                NumRevisions = 0
            };

            taskItem = TaskItemHistoryItems(jsonTaskItem, taskItem, boardId);

            taskItem.Release = await releaseRepository.GetFirstReleaseBeforeDateAsync(taskItem.FinishTime);

            return taskItem;
        }

        private string GetCardState(JToken jsonTaskItem)
        {
            return jsonTaskItem["columnname"].ToString() switch
            {
                "Engineering Backlog" => "New",
                "Engineering" => "New",
                "Product Backlog" => "New",
                "Product" => "New",
                "Top Priority" => "Active",
                "Working" => "Active",
                "Ready for Prod Deploy" => "Resolved",
                "Released to Prod this week" => "Closed",
                "Ready to Archive" => "Closed",
                "Archive" => "Archived",
                _ => ""
            };
        }

        public TaskItem TaskItemHistoryItems(JToken jsonTaskItem, TaskItem taskItem, int boardId)
        {
            var history = kanbanizeApiRepository.GetTaskItemHistory(jsonTaskItem, boardId);

            taskItem.NumRevisions = history.Count();

            foreach (var item in history)
            {
                try
                {
                    var historyItemDate = (DateTime) item["entrydate"];
                    if (item["historyevent"].ToString() == "Task moved")
                    {
                        var columnName = item["details"].ToString();
                        if ((columnName.Contains("to 'Top Priority'")
                             || columnName.Contains("to 'In Process.Working'")
                             || columnName.Contains("Ready for Deploy")
                             || columnName.Contains("to 'In Process.Ready for Prod Deploy'"))
                            && (taskItem.StartTime > historyItemDate
                                || taskItem.StartTime == DateTime.MinValue))
                        {
                            taskItem.StartTime = historyItemDate.ToUniversalTime();
                        }

                        if ((columnName.Contains("to 'Released to Prod this week'")
                             || columnName.Contains("to 'Ready to Archive'"))
                            && (taskItem.FinishTime > historyItemDate
                                || taskItem.FinishTime == DateTime.MaxValue))
                        {
                            taskItem.FinishTime = historyItemDate.ToUniversalTime();;
                        }
                    }

                    if (taskItem.LastChangedOn == historyItemDate
                        || taskItem.LastChangedBy == "")
                    {
                        taskItem.LastChangedBy = item["author"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            if (taskItem.StartTime == DateTime.MinValue && taskItem.CardState != "New")
            {
                taskItem.StartTime = taskItem.CreatedOn.ToUniversalTime();;
            }

            if (taskItem.NumRevisions == 0)
            {
                if (taskItem.CurrentBoardColumn == "Ready for Prod Deploy"
                    || taskItem.CurrentBoardColumn == "Released to Prod this week"
                    || taskItem.CurrentBoardColumn == "Ready to Archive"
                    || taskItem.CurrentBoardColumn == "Archive")
                {
                    taskItem.FinishTime = taskItem.CreatedOn.ToUniversalTime();;
                }
            }

            return taskItem;
        }

        public TaskItemType GetCardType(JToken workItem)
        {
            var workItemTypeString = workItem["type"].ToString();

            return workItemTypeString switch
            {
                "Product" => TaskItemType.Product,
                "Engineering" => TaskItemType.Engineering,
                _ => TaskItemType.Unanticipated
            };
        }
    }
}

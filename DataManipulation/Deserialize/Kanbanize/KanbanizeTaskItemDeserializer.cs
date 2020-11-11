using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Api;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DataAccess.Deserialize.Kanbanize
{
    public interface IKanbanizeTaskItemDeserializer
    {
        Task<List<TaskItem>> DeserializeTaskItemListAsync(IEnumerable<JToken> jsonTaskItems,
            int boardId);

        TaskItem DeserializeTaskItem(JToken jsonTaskItem, int boardId);
        Task<TaskItem> FillInTaskItemStateDetails(HistoryEvent historyEvent, TaskItem taskItem);
        string GetTaskItemState(string column);
        TaskItemType GetCardType(string type);
    }

    public class KanbanizeTaskItemDeserializer : IKanbanizeTaskItemDeserializer
    {
        private readonly IKanbanizeHistoryEventDeserializer historyEventDeserializer;

        public KanbanizeTaskItemDeserializer()
        {
            historyEventDeserializer = new KanbanizeHistoryEventDeserializer();
        }

        public async Task<List<TaskItem>> DeserializeTaskItemListAsync(IEnumerable<JToken> jsonTaskItems,
            int boardId)
        {
            var taskItems = jsonTaskItems.Select(taskItem => DeserializeTaskItem(taskItem, boardId)).ToDictionary(task => task.Id);

            var taskIds = taskItems.Select(taskItem => taskItem.Key).ToList();

            var kanbanizeApi = new KanbanizeApi(new RestClient());
            var history = await kanbanizeApi.GetHistoryEventsAsync(taskIds, boardId);

            taskItems = await historyEventDeserializer.DeserializeHistoryEventsAsync(history, taskItems);

            return taskItems.Values.ToList();
        }

        public TaskItem DeserializeTaskItem(JToken jsonTaskItem, int boardId)
        {
            var taskItem = new TaskItem
            {
                Id = (int) jsonTaskItem["taskid"],
                Title = jsonTaskItem["title"].ToString(),
                StartTime = null,
                FinishTime = null,
                Type = GetCardType(jsonTaskItem["type"].ToString()),
                DevelopmentTeamName = boardId == 4
                    ? "Enterprise Team"
                    : "Assessments Team",
                Impact = jsonTaskItem["priority"].ToString(),
                CommentCount = jsonTaskItem["comments"].Count(),
                LastChangedOn = null,
                CurrentBoardColumn = jsonTaskItem["columnname"].ToString()
            };
            taskItem.State = GetTaskItemState(taskItem.CurrentBoardColumn);

            return taskItem;
        }

        public async Task<TaskItem> FillInTaskItemStateDetails(HistoryEvent historyEvent, TaskItem taskItem)
        {
            switch (historyEvent.EventType)
            {
                case "Task created":
                    taskItem.CreatedOn = historyEvent.EventDate;
                    taskItem.CreatedBy = historyEvent.Author;
                    break;
                case "Task moved":
                {
                    if (historyEvent.TaskItemState == "Top Priority")
                    {
                        taskItem.StartTime = historyEvent.EventDate;
                    }

                    if (historyEvent.TaskItemState == "Released")
                    {
                        if (historyEvent.EventDate < taskItem.FinishTime || taskItem.FinishTime == null)
                        {
                            var releaseRepository = new ReleaseRepository();
                            taskItem.FinishTime = historyEvent.EventDate;
                            taskItem.Release =
                                await releaseRepository.GetFirstReleaseBeforeDateAsync(taskItem.FinishTime);
                        }
                    }

                    break;
                }
            }

            return taskItem;
        }

        public string GetTaskItemState(string column)
        {
            return column switch
            {
                "Backlog" => "Backlog",
                "Engineering Backlog" => "Backlog",
                "Engineering" => "Backlog",
                "Product Backlog" => "Backlog",
                "Product" => "Backlog",
                "Top Priority" => "Top Priority",
                "In Process.Working" => "In Process",
                "In Process" => "In Process",
                "Working" => "In Process",
                "Ready for Prod Deploy" => "In Process",
                "In Process.Ready for Prod Deploy" => "In Process",
                "Released to Prod this week" => "Released",
                "Ready to Archive" => "Released",
                "Archive" => "Released",
                _ => null
            };
        }

        public TaskItemType GetCardType(string type)
        {
            return type switch
            {
                "Product" => TaskItemType.Product,
                "Engineering" => TaskItemType.Engineering,
                _ => TaskItemType.Unanticipated
            };
        }
    }
}

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
        Task<TaskItem> FillInTaskItemStateDetailsAsync(HistoryEvent historyEvent, TaskItem taskItem);
        TaskItemState GetTaskItemState(string column);
        TaskItemType GetCardType(string type);
    }

    public class KanbanizeTaskItemDeserializer : IKanbanizeTaskItemDeserializer
    {
        private readonly IKanbanizeHistoryEventDeserializer kanbanizeHistoryEventDeserializer;

        public KanbanizeTaskItemDeserializer()
        {
            kanbanizeHistoryEventDeserializer = new KanbanizeHistoryEventDeserializer();
        }

        public KanbanizeTaskItemDeserializer(IKanbanizeHistoryEventDeserializer kanbanizeHistoryEventDeserializer)
        {
            this.kanbanizeHistoryEventDeserializer = kanbanizeHistoryEventDeserializer;
        }

        public async Task<List<TaskItem>> DeserializeTaskItemListAsync(IEnumerable<JToken> jsonTaskItems,
            int boardId)
        {
            var taskItems = jsonTaskItems.Select(taskItem => DeserializeTaskItem(taskItem, boardId)).ToDictionary(task => task.Id);

            var taskIds = taskItems.Select(taskItem => taskItem.Key).ToList();

            var kanbanizeApi = new KanbanizeApi(new RestClient());
            var history = await kanbanizeApi.GetHistoryEventsAsync(taskIds, boardId);

            taskItems = await kanbanizeHistoryEventDeserializer.DeserializeHistoryEventsAsync(history, taskItems);

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
                DevelopmentTeam = boardId == 4
                    ? "Enterprise Team"
                    : "Assessments Team",
                LastChangedOn = null,
                CurrentBoardColumn = jsonTaskItem["columnname"].ToString()
            };

            if(taskItem.Id == 540)
            {
                Console.WriteLine();
            }

            taskItem.State = GetTaskItemState(taskItem.CurrentBoardColumn);

            return taskItem;
        }

        public virtual async Task<TaskItem> FillInTaskItemStateDetailsAsync(HistoryEvent historyEvent, TaskItem taskItem)
        {
            switch (historyEvent.EventType)
            {
                case "Task created":
                    taskItem.CreatedOn = historyEvent.EventDate;
                    taskItem.CreatedBy = historyEvent.Author;
                    break;
                case "Task moved":
                {
                    if (historyEvent.TaskItemState == TaskItemState.TopPriority)
                    {
                        taskItem.StartTime = historyEvent.EventDate;
                    }

                    if (historyEvent.TaskItemState == TaskItemState.Released)
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

        public TaskItemState GetTaskItemState(string column)
        {
            return column switch
            {
                "Backlog" => TaskItemState.Backlog,
                "Engineering Backlog" => TaskItemState.Backlog,
                "Engineering" => TaskItemState.Backlog,
                "Product Backlog" => TaskItemState.Backlog,
                "Product" => TaskItemState.Backlog,
                "Top Priority" => TaskItemState.TopPriority,
                "In Process.Working" => TaskItemState.InProcess,
                "In Process" => TaskItemState.InProcess,
                "Working" => TaskItemState.InProcess,
                "Ready for Prod Deploy" => TaskItemState.InProcess,
                "In Process.Ready for Prod Deploy" => TaskItemState.InProcess,
                "Released to Prod this week" => TaskItemState.Released,
                "Ready to Archive" => TaskItemState.Released,
                "Archive" => TaskItemState.Released,
                _ => TaskItemState.None
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

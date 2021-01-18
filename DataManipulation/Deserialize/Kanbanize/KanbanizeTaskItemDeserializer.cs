using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Api;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using Newtonsoft.Json.Linq;
using RestSharp;
using BoardColumn = DataAccess.Objects.BoardColumn;

namespace DataAccess.Deserialize.Kanbanize
{
    public interface IKanbanizeTaskItemDeserializer
    {
        Task<List<TaskItem>> DeserializeTaskItemListAsync(IEnumerable<JToken> jsonTaskItems,
            int boardId);

        Task<TaskItem> DeserializeTaskItemAsync(JToken jsonTaskItem, int boardId);

        Task<TaskItem> FillInTaskItemStateDetailsAsync(HistoryEvent historyEvent,
            TaskItem taskItem);

        BoardColumn GetBoardColumn(string columnString);
        TaskItemState GetTaskItemState(BoardColumn column);
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
            var taskItems = new Dictionary<int, TaskItem>();
            foreach (var taskItem in jsonTaskItems)
            {
                var newTaskItem = await DeserializeTaskItemAsync(taskItem, boardId);
                taskItems.Add(newTaskItem.Id, newTaskItem);
            }

            var taskIds = taskItems.Select(taskItem => taskItem.Key).ToList();

            var kanbanizeApi = new KanbanizeApi();
            var history = kanbanizeApi.GetHistoryEvents(taskIds, boardId);

            taskItems = await kanbanizeHistoryEventDeserializer.DeserializeHistoryEventsAsync(history, taskItems);

            return taskItems.Values.ToList();
        }

        public async Task<TaskItem> DeserializeTaskItemAsync(JToken jsonTaskItem, int boardId)
        {
            var developmentTeamRepository = new DevelopmentTeamsRepository();
            var taskItem = new TaskItem
            {
                Id = (int) jsonTaskItem["taskid"],
                Title = jsonTaskItem["title"].ToString(),
                StartTime = null,
                FinishTime = null,
                Type = GetCardType(jsonTaskItem["type"].ToString()),
                DevelopmentTeam = await developmentTeamRepository.GetTeamAsync(boardId),
                LastChangedOn = null,
                CurrentBoardColumn = GetBoardColumn(jsonTaskItem["columnname"].ToString())
            };

            taskItem.State = GetTaskItemState(taskItem.CurrentBoardColumn);

            return taskItem;
        }

        public virtual async Task<TaskItem> FillInTaskItemStateDetailsAsync(HistoryEvent historyEvent,
            TaskItem taskItem)
        {
            var developerRepository = new DeveloperRepository();
            switch (historyEvent.EventType)
            {
                case "Task created":
                    taskItem.CreatedOn = historyEvent.EventDate;
                    taskItem.CreatedBy = await developerRepository.GetDeveloperByNameAsync(historyEvent.Author);
                    break;
                case "Task moved":
                {
                    switch (historyEvent.TaskItemState)
                    {
                        case TaskItemState.None:
                            break;
                        case TaskItemState.Backlog:
                            break;
                        case TaskItemState.TopPriority:
                            if (taskItem.StartTime == null || taskItem.StartTime > historyEvent.EventDate)
                            {
                                taskItem.StartTime = historyEvent.EventDate;
                            }
                            break;
                        case TaskItemState.InProcess:
                            taskItem.StartTime ??= taskItem.CreatedOn;
                            if (taskItem.StartTime > historyEvent.EventDate)
                            {
                                taskItem.StartTime = historyEvent.EventDate;
                            }
                            break;
                        case TaskItemState.Released:
                            taskItem.StartTime ??= taskItem.CreatedOn;
                            if (taskItem.StartTime > historyEvent.EventDate)
                            {
                                taskItem.StartTime = historyEvent.EventDate;
                            }
                            if (historyEvent.EventDate < taskItem.FinishTime || taskItem.FinishTime == null)
                            {
                                var releaseRepository = new ReleaseRepository();
                                taskItem.FinishTime = historyEvent.EventDate;
                                taskItem.Release =
                                    await releaseRepository.GetFirstReleaseBeforeDateAsync(taskItem.FinishTime);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }
            }

            return taskItem;
        }

        public BoardColumn GetBoardColumn(string columnString)
        {
            return columnString switch
            {
                "Backlog" => BoardColumn.Backlog,
                "Engineering Backlog" => BoardColumn.EngineeringBacklog,
                "Engineering" => BoardColumn.Engineering,
                "Product Backlog" => BoardColumn.ProductBacklog,
                "Product" => BoardColumn.Product,
                "Top Priority" => BoardColumn.TopPriority,
                "In Process.Working" => BoardColumn.InProcessWorking,
                "In Process" => BoardColumn.InProcess,
                "Working" => BoardColumn.Working,
                "Ready for Prod deploy" => BoardColumn.ReadyForProdDeploy,
                "In Process.Ready for Prod deploy" => BoardColumn.InProcessReadyForProdDeploy,
                "Released to Prod this week" => BoardColumn.ReleasedToProdThisWeek,
                "Ready to Archive" => BoardColumn.ReadyToArchive,
                "Archive" => BoardColumn.Archive,
                _ => BoardColumn.None
            };
        }

        public TaskItemState GetTaskItemState(BoardColumn column)
        {
            return column switch
            {
                BoardColumn.Backlog => TaskItemState.Backlog,
                BoardColumn.EngineeringBacklog => TaskItemState.Backlog,
                BoardColumn.Engineering => TaskItemState.Backlog,
                BoardColumn.ProductBacklog => TaskItemState.Backlog,
                BoardColumn.Product => TaskItemState.Backlog,
                BoardColumn.TopPriority => TaskItemState.TopPriority,
                BoardColumn.InProcessWorking => TaskItemState.InProcess,
                BoardColumn.InProcess => TaskItemState.InProcess,
                BoardColumn.Working => TaskItemState.InProcess,
                BoardColumn.ReadyForProdDeploy => TaskItemState.InProcess,
                BoardColumn.InProcessReadyForProdDeploy => TaskItemState.InProcess,
                BoardColumn.ReleasedToProdThisWeek => TaskItemState.Released,
                BoardColumn.ReadyToArchive => TaskItemState.Released,
                BoardColumn.Archive => TaskItemState.Released,
                BoardColumn.None => TaskItemState.None,
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

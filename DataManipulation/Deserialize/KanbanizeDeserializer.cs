using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.ApiWrapper;
using DataAccess.DatabaseAccess;
using DataAccess.DataRepositories;
using DataManipulation.ApiWrapper;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;
using Newtonsoft.Json.Linq;

namespace DataAccess.Deserialize
{
    public interface IKanbanizeDeserializer
    {
        Task<IEnumerable<TaskItem>> TaskItemListAsync(IEnumerable<JToken> jsonTaskItems, int boardId);
        Task<TaskItem> TaskItemAsync(JToken jsonTaskItem, int boardId);
        void TaskItemHistoryItems(JToken jsonTaskItem, TaskItem taskItem, int boardId);
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
                if (!item["columnid"].ToString().Contains("archive") || (DateTime) item["updatedat"] < DateTime.Now.AddDays(-90)) list.Add(await TaskItemAsync(item, boardId));
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
                CreatedOn = (DateTime) jsonTaskItem["createdat"],
                CreatedBy = jsonTaskItem["reporter"].ToString(),
                LastChangedOn = (DateTime) jsonTaskItem["updatedat"],
                LastChangedBy = "",
                CurrentBoardColumn = jsonTaskItem["columnname"].ToString(),
                CardState = GetCardState(jsonTaskItem),
                Impact = jsonTaskItem["priority"]?.ToString(),
                CommentCount = jsonTaskItem["comments"].ToString() == ""
                    ? 0
                    : int.Parse(jsonTaskItem["comments"].Count().ToString()),
                NumRevisions = 0
            };


            TaskItemHistoryItems(jsonTaskItem, taskItem, boardId);

            var releases = await releaseRepository.GetReleasesBeforeDateAsync(taskItem.FinishTime);
            var release = new Release();
            if (releases.Count > 0)
            {
                release = releases.First();
            }

            taskItem.Release = release;

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
                _ => ""
            };
        }

        public void TaskItemHistoryItems(JToken jsonTaskItem, TaskItem taskItem, int boardId)
        {
            var history = kanbanizeApiRepository.GetTaskItemHistory(jsonTaskItem, boardId);

            taskItem.NumRevisions = history.Count();

            foreach (var item in history)
            {
                try
                {
                    if (item["historyevent"].ToString() == "Task moved")
                    {
                        if ((item["details"].ToString().Contains("to 'Top Priority'")
                             || item["details"].ToString().Contains("to 'Working'"))
                            && (taskItem.StartTime > (DateTime) item["entrydate"]
                                || taskItem.StartTime == DateTime.MinValue))
                        {
                            taskItem.StartTime = (DateTime) item["entrydate"];
                        }
                        else if ((item["details"].ToString().Contains("to 'Ready for Prod Deploy'")
                                  || item["details"].ToString().Contains("to 'Released to Prod this week'")
                                  || item["details"].ToString().Contains("to 'Ready to Archive'"))
                                 && (taskItem.FinishTime < (DateTime) item["entrydate"]
                                     || taskItem.FinishTime == DateTime.MaxValue))
                        {
                            taskItem.FinishTime = (DateTime) item["entrydate"];
                        }
                    }

                    if (taskItem.LastChangedOn == (DateTime) item["entrydate"]
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
                taskItem.StartTime = taskItem.CreatedOn;
            }

            if (taskItem.NumRevisions == 0)
            {
                if (taskItem.CurrentBoardColumn == "Ready for Prod Deploy"
                    || taskItem.CurrentBoardColumn == "Released to Prod this week"
                    || taskItem.CurrentBoardColumn == "Ready to Archive")
                {
                    taskItem.FinishTime = taskItem.CreatedOn;
                }
            }
        }

        public TaskItemType GetCardType(JToken workItem)
        {
            var workItemTypeString = workItem["type"].ToString();

            return workItemTypeString switch
            {
                "Product" => (int.Parse(workItem["links"]["child"].ToString()) > 0
                    ? TaskItemType.StrategicProduct
                    : TaskItemType.TacticalProduct),
                "Engineering" => (int.Parse(workItem["links"]["child"].ToString()) > 0
                    ? TaskItemType.StrategicEngineering
                    : TaskItemType.TacticalEngineering),
                _ => TaskItemType.Unanticipated
            };
        }
    }
}

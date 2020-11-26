using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIDevOpsDataExtractor_DEPRECATED.ApiWrapper;
using Newtonsoft.Json.Linq;

namespace KPIDevOpsDataExtractor_DEPRECATED.Deserializer
{
    public interface IDevOpsDeserializer
    {
        Task<IEnumerable<TaskItem>> TaskItemListAsync(IEnumerable<JToken> jsonTaskItems);
        Task<TaskItem> TaskItem(JToken jsonTaskItem);
        TaskItem GetHistoryDetails(TaskItem taskItem, JToken jsonWorkItemUpdates);
        TaskItemType GetTaskItemType(JToken workItemType);
    }

    public class DevOpsDeserializer : IDevOpsDeserializer
    {
        private readonly IDevOpsApiWrapper devOpsApiWrapper;
        private readonly IReleaseRepository releaseRepository;
        private DateTimeOffset minStartTime;

        public DevOpsDeserializer(IDevOpsApiWrapper devOpsApiWrapper, IReleaseRepository releaseRepository)
        {
            this.releaseRepository = releaseRepository;
            this.devOpsApiWrapper = devOpsApiWrapper;
        }

        public async Task<IEnumerable<TaskItem>> TaskItemListAsync(IEnumerable<JToken> jsonTaskItems)
        {
            var taskItemList = new List<TaskItem>();
            var i = 1;
            foreach (var item in jsonTaskItems)
            {
                taskItemList.Add(await TaskItem(item));
                Console.WriteLine($"Card Number: {i}");
                ++i;
            }

            return taskItemList;
        }

        public async Task<TaskItem> TaskItem(JToken jsonTaskItem)
        {
            minStartTime = new DateTimeOffset(new DateTime(2015, 1, 1));
            var taskItem = new TaskItem
            {
                Id = (int) jsonTaskItem["id"],
                Title = jsonTaskItem["fields"]["System.Title"].ToString(),
                StartTime = minStartTime,
                FinishTime = new DateTimeOffset(DateTime.Now.AddYears(50)),
                Type = GetTaskItemType(jsonTaskItem["fields"]["System.WorkItemType"]),
                DevelopmentTeam = jsonTaskItem["fields"]["System.BoardLane"]?.ToString(),
                CreatedOn = (DateTimeOffset) jsonTaskItem["fields"]["System.CreatedDate"],
                CreatedBy = jsonTaskItem["fields"]["System.CreatedBy"]["displayName"].ToString(),
                LastChangedOn = (DateTimeOffset) jsonTaskItem["fields"]["System.ChangedDate"],
                LastChangedBy = jsonTaskItem["fields"]["System.ChangedBy"]["displayName"].ToString(),
                CurrentBoardColumn = jsonTaskItem["fields"]["System.BoardColumn"].ToString(),
                State = GetTaskItemState(jsonTaskItem["fields"]["System.State"].ToString()),
                NumRevisions = (int) jsonTaskItem["rev"],
                Release = new Release(),
                HistoryEvents = new List<HistoryEvent>()
            };

            var jsonWorkItemUpdates = devOpsApiWrapper.GetWorkItemUpdates(taskItem);

            taskItem = GetHistoryDetails(taskItem, jsonWorkItemUpdates);

            taskItem.Release = await releaseRepository.GetFirstReleaseBeforeDateAsync(taskItem.FinishTime);

            Console.WriteLine($"Finished Deserializing Card: {taskItem.Id}");
            return taskItem;
        }

        public TaskItem GetHistoryDetails(TaskItem taskItem, JToken jsonWorkItemUpdates)
        {
            foreach (var workItemUpdate in jsonWorkItemUpdates)
            {
                var historyEvent = new HistoryEvent();
                try
                {
                    historyEvent.Author = workItemUpdate["revisedBy"]["name"].ToString();
                    historyEvent.Id = (int) workItemUpdate["fields"]["System.Watermark"]["newValue"];
                    historyEvent.EventDate = (DateTimeOffset) workItemUpdate["revisedDate"];
                    historyEvent.TaskId = (int) workItemUpdate["workItemId"];
                    var boardColumn = workItemUpdate["fields"]["System.BoardColumn"]["newValue"].ToString();
                    switch (boardColumn)
                    {
                        case "Parking Lot":
                        case "Engineering Backlog":
                        case "Product Backlog":
                        {
                            historyEvent.EventType = "Task created";
                            historyEvent.TaskItemColumn = "Backlog";
                            historyEvent.TaskItemState = TaskItemState.Backlog;
                            if (taskItem.CreatedOn == minStartTime || taskItem.CreatedOn > historyEvent.EventDate)
                            {
                                taskItem.CreatedOn = historyEvent.EventDate;
                            }

                            break;
                        }
                        case "Top Priority":
                            historyEvent.EventType = "Task moved";
                            historyEvent.TaskItemColumn = "Top Priority";
                            historyEvent.TaskItemState = TaskItemState.TopPriority;
                            taskItem.StartTime = historyEvent.EventDate;
                            break;
                        case "Working On":
                        {
                            historyEvent.EventType = "Task moved";
                            historyEvent.TaskItemColumn = "In Process.Working";
                            historyEvent.TaskItemState = TaskItemState.InProcess;
                            if (taskItem.CreatedOn == minStartTime)
                            {
                                taskItem.StartTime = historyEvent.EventDate;
                            }

                            break;
                        }
                        case "Ready for Prod Deploy":
                        case "Merged into Master":
                        {
                            historyEvent.EventType = "Task moved";
                            historyEvent.TaskItemColumn = "In Process.Ready for Prod Deploy";
                            historyEvent.TaskItemState = TaskItemState.InProcess;
                            if (taskItem.StartTime == minStartTime)
                            {
                                taskItem.StartTime = historyEvent.EventDate;
                            }

                            break;
                        }
                        case "Released To Production This week":
                            historyEvent.EventType = "Task moved";
                            historyEvent.TaskItemColumn = "Released to Prod this week";
                            historyEvent.TaskItemState = TaskItemState.Released;
                            taskItem.FinishTime = historyEvent.EventDate;
                            break;
                        case "In Production":
                        {
                            historyEvent.EventType = "Task moved";
                            historyEvent.TaskItemColumn = "Archive";
                            historyEvent.TaskItemState = TaskItemState.Released;
                            if (taskItem.FinishTime > historyEvent.EventDate)
                            {
                                taskItem.FinishTime = historyEvent.EventDate;
                            }

                            break;
                        }
                    }
                    taskItem.HistoryEvents.Add(historyEvent);
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            return taskItem;
        }

        public TaskItemType GetTaskItemType(JToken workItemType)
        {
            var workItemTypeString = workItemType.ToString().ToLower();

            if (workItemTypeString.Contains("unanticipated"))
            {
                return TaskItemType.Unanticipated;
            }

            if (workItemTypeString.Contains("product"))
            {
                return TaskItemType.Product;
            }

            if (workItemTypeString.Contains("engineering"))
            {
                return TaskItemType.Engineering;
            }
            throw new Exception("Unknown Card Type...");
        }

        public TaskItemState GetTaskItemState(string taskItemState)
        {
            if (taskItemState == "")
            {
                return TaskItemState.Backlog;
            }

            return TaskItemState.None;
        }
    }
}

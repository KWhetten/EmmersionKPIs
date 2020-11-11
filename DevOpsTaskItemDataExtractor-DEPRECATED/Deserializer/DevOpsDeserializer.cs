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
        DateTimeOffset? JsonWorkItemStartTime(JToken jsonWorkItemUpdates);
        DateTimeOffset? JsonWorkItemFinishTime(JToken jsonWorkItemUpdates);
        TaskItemType GetCardType(JToken workItemType);
    }

    public class DevOpsDeserializer : IDevOpsDeserializer
    {
        private readonly IDevOpsApiWrapper devOpsApiWrapper;
        private readonly IReleaseRepository releaseRepository;

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
            var taskItem = new TaskItem
            {
                Id = (int) jsonTaskItem["id"],
                Title = jsonTaskItem["fields"]["System.Title"].ToString(),
                Type = GetCardType(jsonTaskItem["fields"]["System.WorkItemType"]),
                DevelopmentTeamName = jsonTaskItem["fields"]["System.BoardLane"]?.ToString(),
                CreatedOn = (DateTimeOffset) jsonTaskItem["fields"]["System.CreatedDate"],
                CreatedBy = jsonTaskItem["fields"]["System.CreatedBy"]["displayName"].ToString(),
                LastChangedOn = (DateTimeOffset) jsonTaskItem["fields"]["System.ChangedDate"],
                LastChangedBy = jsonTaskItem["fields"]["System.ChangedBy"]["displayName"].ToString(),
                CurrentBoardColumn = jsonTaskItem["fields"]["System.BoardColumn"].ToString(),
                State = jsonTaskItem["fields"]["System.State"].ToString(),
                Impact = jsonTaskItem["fields"]["Custom.Impact"]?.ToString(),
                CommentCount = (int) jsonTaskItem["fields"]["System.CommentCount"],
                NumRevisions = (int) jsonTaskItem["rev"]
            };

            var jsonWorkItemUpdates = devOpsApiWrapper.GetWorkItemUpdates(taskItem);

            taskItem.StartTime = JsonWorkItemStartTime(jsonWorkItemUpdates);

            taskItem.FinishTime = JsonWorkItemFinishTime(jsonWorkItemUpdates);

            taskItem.Release = await releaseRepository.GetFirstReleaseBeforeDateAsync(taskItem.FinishTime);


            Console.WriteLine($"Finished Deserializing Card: {taskItem.Id}");
            return taskItem;
        }

        public TaskItemType GetCardType(JToken workItemType)
        {
            var workItemTypeString = workItemType.ToString();
            if (workItemTypeString.ToLower().Contains("unanticipated"))
            {
                workItemTypeString = "Unanticipated";
            }
            return workItemTypeString switch
            {
                "Strategic Product Work" => TaskItemType.Product,
                "Tactical Product Work" => TaskItemType.Product,
                "Strategic Engineering Work" => TaskItemType.Engineering,
                "Tactical Engineering Work" => TaskItemType.Engineering,
                "Unanticipated" => TaskItemType.Unanticipated,
                _ => throw new Exception("Unknown Work Item Type...")
            };
        }

        public DateTimeOffset? JsonWorkItemStartTime(JToken jsonWorkItemUpdates)
        {
            foreach (var itemUpdate in jsonWorkItemUpdates)
            {
                try
                {
                    if (itemUpdate["fields"]["System.BoardColumn"]["oldValue"].ToString() == "Parking Lot")
                    {
                        return new DateTimeOffset((DateTime)itemUpdate["fields"]["System.ChangedDate"]["newValue"]);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return null;
        }

        public DateTimeOffset? JsonWorkItemFinishTime(JToken jsonWorkItemUpdates)
        {
            foreach (var itemUpdate in jsonWorkItemUpdates)
            {
                try
                {
                    if (itemUpdate["fields"]["System.State"]["newValue"].ToString() == "Resolved"
                        || itemUpdate["fields"]["System.State"]["newValue"].ToString() == "Closed")
                        return new DateTimeOffset((DateTime)itemUpdate["fields"]["System.ChangedDate"]["newValue"]);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return null;
        }
    }
}

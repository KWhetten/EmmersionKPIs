using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DatabaseAccess;
using DataAccess.DataRepositories;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;
using KPIDevOpsDataExtractor_DEPRECATED.ApiWrapper;
using Newtonsoft.Json.Linq;

namespace KPIDevOpsDataExtractor_DEPRECATED.Deserializer
{
    public interface IDevOpsDeserializer
    {
        Task<IEnumerable<TaskItem>> TaskItemListAsync(IEnumerable<JToken> jsonTaskItems);
        Task<TaskItem> TaskItem(JToken jsonTaskItem);
        DateTime JsonWorkItemStartTime(JToken jsonWorkItemUpdates);
        DateTime JsonWorkItemFinishTime(JToken jsonWorkItemUpdates);
        TaskItemType GetCardType(JToken workItemType);
    }

    public class DevOpsDeserializer : IDevOpsDeserializer
    {
        private readonly IDevOpsApiWrapper devOpsApiWrapper;
        private readonly ITaskItemRepository TaskItemRepository;
        private readonly IUserRepository userRepository;
        private readonly IReleaseRepository releaseRepository;

        public DevOpsDeserializer(IDevOpsApiWrapper devOpsApiWrapper, IReleaseRepository releaseRepository, ITaskItemRepository TaskItemRepository, IUserRepository userRepository)
        {
            this.releaseRepository = releaseRepository;
            this.devOpsApiWrapper = devOpsApiWrapper;
            this.TaskItemRepository = TaskItemRepository;
            this.userRepository = userRepository;
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
                CreatedOn = (DateTime) jsonTaskItem["fields"]["System.CreatedDate"],
                CreatedBy = jsonTaskItem["fields"]["System.CreatedBy"]["displayName"].ToString(),
                LastChangedOn = (DateTime) jsonTaskItem["fields"]["System.ChangedDate"],
                LastChangedBy = jsonTaskItem["fields"]["System.ChangedBy"]["displayName"].ToString(),
                CurrentBoardColumn = jsonTaskItem["fields"]["System.BoardColumn"].ToString(),
                CardState = jsonTaskItem["fields"]["System.State"].ToString(),
                Impact = jsonTaskItem["fields"]["Custom.Impact"]?.ToString(),
                CommentCount = (int) jsonTaskItem["fields"]["System.CommentCount"],
                NumRevisions = (int) jsonTaskItem["rev"]
            };

            var jsonWorkItemUpdates = devOpsApiWrapper.GetWorkItemUpdates(taskItem);

            taskItem.StartTime = JsonWorkItemStartTime(jsonWorkItemUpdates);

            taskItem.FinishTime = JsonWorkItemFinishTime(jsonWorkItemUpdates);

            var releases = await releaseRepository.GetReleasesBeforeDateAsync(taskItem.FinishTime);
            var release = new Release();
            if (releases.Count > 0)
            {
                release = releases.First();
            }

            taskItem.Release = release;

            Console.WriteLine($"Finished Deserializing Card: {taskItem.Id}");
            return taskItem;
        }

        public TaskItemType GetCardType(JToken workItemType)
        {
            var workItemTypeString = workItemType.ToString();
            return workItemTypeString switch
            {
                "Strategic Product Work" => TaskItemType.StrategicProduct,
                "Tactical Product Work" => TaskItemType.TacticalProduct,
                "Unanticipated Product Work" => TaskItemType.UnanticipatedProduct,
                "Strategic Engineering Work" => TaskItemType.StrategicEngineering,
                "Tactical Engineering Work" => TaskItemType.TacticalEngineering,
                "Unanticipated Engineering Work" => TaskItemType.UnanticipatedEngineering,
                _ => throw new Exception("Unknown Work Item Type...")
            };
        }

        public DateTime JsonWorkItemStartTime(JToken jsonWorkItemUpdates)
        {
            foreach (var itemUpdate in jsonWorkItemUpdates)
            {
                try
                {
                    if (itemUpdate["fields"]["System.BoardColumn"]["oldValue"].ToString() == "Parking Lot")
                    {
                        return (DateTime) itemUpdate["fields"]["System.ChangedDate"]["newValue"];
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return DateTime.MinValue;
        }

        public DateTime JsonWorkItemFinishTime(JToken jsonWorkItemUpdates)
        {
            foreach (var itemUpdate in jsonWorkItemUpdates)
            {
                try
                {
                    if (itemUpdate["fields"]["System.State"]["newValue"].ToString() == "Resolved"
                        || itemUpdate["fields"]["System.State"]["newValue"].ToString() == "Closed")
                        return (DateTime) itemUpdate["fields"]["System.ChangedDate"]["newValue"];
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return DateTime.MaxValue;
        }
    }
}

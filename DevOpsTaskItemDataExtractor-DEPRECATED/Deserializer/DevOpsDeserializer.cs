using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.DatabaseAccess;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;
using KPIDevOpsDataExtractor_DEPRECATED.ApiWrapper;
using Newtonsoft.Json.Linq;

namespace KPIDevOpsDataExtractor_DEPRECATED.Deserializer
{
    public interface IDevOpsDeserializer
    {
        IEnumerable<TaskItem> TaskItemList(IEnumerable<JToken> jsonTaskItems);
        TaskItem TaskItem(JToken jsonTaskItem);
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

        public IEnumerable<TaskItem> TaskItemList(IEnumerable<JToken> jsonTaskItems)
        {
            var TaskItemList = new List<TaskItem>();
            var i = 1;
            foreach (var item in jsonTaskItems)
            {
                TaskItemList.Add(TaskItem(item));
                Console.WriteLine($"Card Number: {i}");
                ++i;
            }

            return TaskItemList;
        }

        public TaskItem TaskItem(JToken jsonTaskItem)
        {
            var TaskItem = new TaskItem
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

            var jsonWorkItemUpdates = devOpsApiWrapper.GetWorkItemUpdates(TaskItem);

            TaskItem.StartTime = JsonWorkItemStartTime(jsonWorkItemUpdates);

            TaskItem.FinishTime = JsonWorkItemFinishTime(jsonWorkItemUpdates);

            var releases = releaseRepository.GetReleasesBeforeDate(TaskItem.FinishTime);
            var release = new Release();
            if (releases.Count > 0)
            {
                release = releases.First();
            }

            TaskItem.Release = release;

            Console.WriteLine($"Finished Deserializing Card: {TaskItem.Id}");
            return TaskItem;
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

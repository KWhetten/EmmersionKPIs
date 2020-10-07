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
        IEnumerable<WorkItemCard> WorkItemCardList(IEnumerable<JToken> jsonWorkItemCards);
        WorkItemCard WorkItemCard(JToken jsonWorkItemCard);
        DateTime JsonWorkItemStartTime(JToken jsonWorkItemUpdates);
        DateTime JsonWorkItemFinishTime(JToken jsonWorkItemUpdates);
        WorkItemCardType GetCardType(JToken workItemType);
    }

    public class DevOpsDeserializer : IDevOpsDeserializer
    {
        private readonly IDevOpsApiWrapper devOpsApiWrapper;
        private readonly IWorkItemCardDataAccess workItemCardDataAccess;
        private readonly IUserDataAccess userDataAccess;
        private readonly IReleaseDataAccess releaseDataAccess;

        public DevOpsDeserializer(IDevOpsApiWrapper devOpsApiWrapper, IReleaseDataAccess releaseDataAccess, IWorkItemCardDataAccess workItemCardDataAccess, IUserDataAccess userDataAccess)
        {
            this.releaseDataAccess = releaseDataAccess;
            this.devOpsApiWrapper = devOpsApiWrapper;
            this.workItemCardDataAccess = workItemCardDataAccess;
            this.userDataAccess = userDataAccess;
        }

        public IEnumerable<WorkItemCard> WorkItemCardList(IEnumerable<JToken> jsonWorkItemCards)
        {
            var workItemCardList = new List<WorkItemCard>();
            var i = 1;
            foreach (var item in jsonWorkItemCards)
            {
                workItemCardList.Add(WorkItemCard(item));
                Console.WriteLine($"Card Number: {i}");
                ++i;
            }

            return workItemCardList;
        }

        public WorkItemCard WorkItemCard(JToken jsonWorkItemCard)
        {
            var workItemCard = new WorkItemCard
            {
                Id = (int) jsonWorkItemCard["id"],
                Title = jsonWorkItemCard["fields"]["System.Title"].ToString(),
                Type = GetCardType(jsonWorkItemCard["fields"]["System.WorkItemType"]),
                DevelopmentTeamName = jsonWorkItemCard["fields"]["System.BoardLane"]?.ToString(),
                CreatedOn = (DateTime) jsonWorkItemCard["fields"]["System.CreatedDate"],
                CreatedBy = jsonWorkItemCard["fields"]["System.CreatedBy"]["displayName"].ToString(),
                LastChangedOn = (DateTime) jsonWorkItemCard["fields"]["System.ChangedDate"],
                LastChangedBy = jsonWorkItemCard["fields"]["System.ChangedBy"]["displayName"].ToString(),
                CurrentBoardColumn = jsonWorkItemCard["fields"]["System.BoardColumn"].ToString(),
                CardState = jsonWorkItemCard["fields"]["System.State"].ToString(),
                Impact = jsonWorkItemCard["fields"]["Custom.Impact"]?.ToString(),
                CommentCount = (int) jsonWorkItemCard["fields"]["System.CommentCount"],
                NumRevisions = (int) jsonWorkItemCard["rev"]
            };

            var jsonWorkItemUpdates = devOpsApiWrapper.GetWorkItemUpdates(workItemCard);

            workItemCard.StartTime = JsonWorkItemStartTime(jsonWorkItemUpdates);

            workItemCard.FinishTime = JsonWorkItemFinishTime(jsonWorkItemUpdates);

            var releases = releaseDataAccess.GetReleasesBeforeDate(workItemCard.FinishTime);
            var release = new Release();
            if (releases.Count > 0)
            {
                release = releases.First();
            }

            workItemCard.Release = release;

            Console.WriteLine($"Finished Deserializing Card: {workItemCard.Id}");
            return workItemCard;
        }

        public WorkItemCardType GetCardType(JToken workItemType)
        {
            var workItemTypeString = workItemType.ToString();
            return workItemTypeString switch
            {
                "Strategic Product Work" => WorkItemCardType.StrategicProduct,
                "Tactical Product Work" => WorkItemCardType.TacticalProduct,
                "Unanticipated Product Work" => WorkItemCardType.UnanticipatedProduct,
                "Strategic Engineering Work" => WorkItemCardType.StrategicEngineering,
                "Tactical Engineering Work" => WorkItemCardType.TacticalEngineering,
                "Unanticipated Engineering Work" => WorkItemCardType.UnanticipatedEngineering,
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

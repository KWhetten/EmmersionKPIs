using System;
using System.Collections.Generic;
using System.Linq;
using DataObjects;
using DataWrapper.ApiWrapper;
using DataWrapper.DatabaseAccess;
using Newtonsoft.Json.Linq;

namespace DataWrapper.Deserialize
{
    public interface IDevOpsDeserializer
    {
        List<Release> Releases(JToken jsonObjects);
        IEnumerable<WorkItemCard> WorkItemCardList(IEnumerable<JToken> jsonWorkItemCards);
        WorkItemCard WorkItemCard(JToken jsonWorkItemCard);
        DateTime JsonWorkItemStartTime(JToken jsonWorkItemUpdates);
        DateTime JsonWorkItemFinishTime(JToken jsonWorkItemUpdates);
        WorkItemCardType GetCardType(JToken workItemType);
    }

    public class DevOpsDeserializer : IDevOpsDeserializer
    {
        private readonly IDevOpsApiWrapper devOpsApiWrapper;
        private readonly IDataAccess dataAccess;

        public DevOpsDeserializer(IDevOpsApiWrapper devOpsApiWrapper, IDataAccess dataAccess)
        {
            this.devOpsApiWrapper = devOpsApiWrapper;
            this.dataAccess = dataAccess;
        }

        public List<Release> Releases(JToken jsonObjects)
        {
            return jsonObjects.Select(jsonObject => new Release
                {
                    Id = (int) jsonObject["id"],
                    Name = (string) jsonObject["release"]["name"],
                    Status = (string) jsonObject["deploymentStatus"],
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = (int) jsonObject["definitionEnvironmentId"],
                        Name = (string) jsonObject["releaseEnvironment"]["name"]
                    },
                    Attempts = (int) jsonObject["attempt"],
                    StartTime = (DateTime) jsonObject["startedOn"],
                    FinishTime = (DateTime) jsonObject["completedOn"]
                })
                .ToList();
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

            var releases = dataAccess.GetReleasesBeforeDate(workItemCard.FinishTime);
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

using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.DatabaseAccess;
using DataManipulation.ApiWrapper;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;
using Newtonsoft.Json.Linq;

namespace DataManipulation.Deserialize
{
    public interface IKanbanizeDeserializer
    {
        IEnumerable<WorkItemCard> WorkItemCardList(IEnumerable<JToken> jsonWorkItemCards, int boardId);
        WorkItemCard WorkItemCard(JToken jsonWorkItemCard, int boardId);
        void WorkItemCardHistoryItems(JToken jsonWorkItemCard, WorkItemCard workItemCard, int boardId);
        WorkItemCardType GetCardType(JToken workItem);
    }

    public class KanbanizeDeserializer : IKanbanizeDeserializer
    {
        private readonly IKanbanizeApiWrapper kanbanizeApiWrapper;
        private readonly ReleaseDataAccess releaseDataAccess = new ReleaseDataAccess();
        private readonly IWorkItemCardDataAccess workItemCardDataAccess;
        private readonly IUserDataAccess userDataAccess;

        public KanbanizeDeserializer(IKanbanizeApiWrapper kanbanizeApiWrapper, IReleaseDataAccess releaseDataAccess, IWorkItemCardDataAccess workItemCardDataAccess, IUserDataAccess userDataAccess)
        {
            this.kanbanizeApiWrapper = kanbanizeApiWrapper;
            this.workItemCardDataAccess = workItemCardDataAccess;
            this.userDataAccess = userDataAccess;
        }

        public IEnumerable<WorkItemCard> WorkItemCardList(IEnumerable<JToken> jsonWorkItemCards, int boardId)
        {
            return (from item in jsonWorkItemCards
                where !item["columnid"].ToString().Contains("archive")
                      || (DateTime) item["updatedat"] < DateTime.Now.AddDays(-90)
                select WorkItemCard(item, boardId)).ToList();
        }

        public WorkItemCard WorkItemCard(JToken jsonWorkItemCard, int boardId)
        {
            var workItemCard = new WorkItemCard
            {
                Id = (int) jsonWorkItemCard["taskid"],
                Title = jsonWorkItemCard["title"].ToString(),
                StartTime = DateTime.MinValue,
                FinishTime = DateTime.MaxValue,
                Type = GetCardType(jsonWorkItemCard),
                DevelopmentTeamName = boardId == 4
                    ? "Enterprise Team"
                    : "Assessment Team",
                CreatedOn = (DateTime) jsonWorkItemCard["createdat"],
                CreatedBy = jsonWorkItemCard["reporter"].ToString(),
                LastChangedOn = (DateTime) jsonWorkItemCard["updatedat"],
                LastChangedBy = "",
                CurrentBoardColumn = jsonWorkItemCard["columnname"].ToString(),
                CardState = GetCardState(jsonWorkItemCard),
                Impact = jsonWorkItemCard["priority"]?.ToString(),
                CommentCount = jsonWorkItemCard["comments"].ToString() == ""
                    ? 0
                    : int.Parse(jsonWorkItemCard["comments"].Count().ToString()),
                NumRevisions = 0
            };


            WorkItemCardHistoryItems(jsonWorkItemCard, workItemCard, boardId);

            var releases = releaseDataAccess.GetReleasesBeforeDate(workItemCard.FinishTime);
            var release = new Release();
            if (releases.Count > 0)
            {
                release = releases.First();
            }

            workItemCard.Release = release;

            return workItemCard;
        }

        private string GetCardState(JToken jsonWorkItemCard)
        {
            return jsonWorkItemCard["columnname"].ToString() switch
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

        public void WorkItemCardHistoryItems(JToken jsonWorkItemCard, WorkItemCard workItemCard, int boardId)
        {
            var history = kanbanizeApiWrapper.GetWorkItemCardHistory(jsonWorkItemCard, boardId);

            workItemCard.NumRevisions = history.Count();

            foreach (var item in history)
            {
                try
                {
                    if (item["historyevent"].ToString() == "Task moved")
                    {
                        if ((item["details"].ToString().Contains("to 'Top Priority'")
                             || item["details"].ToString().Contains("to 'Working'"))
                            && (workItemCard.StartTime > (DateTime) item["entrydate"]
                                || workItemCard.StartTime == DateTime.MinValue))
                        {
                            workItemCard.StartTime = (DateTime) item["entrydate"];
                        }
                        else if ((item["details"].ToString().Contains("to 'Ready for Prod Deploy'")
                                  || item["details"].ToString().Contains("to 'Released to Prod this week'")
                                  || item["details"].ToString().Contains("to 'Ready to Archive'"))
                                 && (workItemCard.FinishTime < (DateTime) item["entrydate"]
                                     || workItemCard.FinishTime == DateTime.MaxValue))
                        {
                            workItemCard.FinishTime = (DateTime) item["entrydate"];
                        }
                    }

                    if (workItemCard.LastChangedOn == (DateTime) item["entrydate"]
                        || workItemCard.LastChangedBy == "")
                    {
                        workItemCard.LastChangedBy = item["author"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            if (workItemCard.StartTime == DateTime.MinValue && workItemCard.CardState != "New")
            {
                workItemCard.StartTime = workItemCard.CreatedOn;
            }

            if (workItemCard.NumRevisions == 0)
            {
                if (workItemCard.CurrentBoardColumn == "Ready for Prod Deploy"
                    || workItemCard.CurrentBoardColumn == "Released to Prod this week"
                    || workItemCard.CurrentBoardColumn == "Ready to Archive")
                {
                    workItemCard.FinishTime = workItemCard.CreatedOn;
                }
            }
        }

        public WorkItemCardType GetCardType(JToken workItem)
        {
            var workItemTypeString = workItem["type"].ToString();

            return workItemTypeString switch
            {
                "Product" => (int.Parse(workItem["links"]["child"].ToString()) > 0
                    ? WorkItemCardType.StrategicProduct
                    : WorkItemCardType.TacticalProduct),
                "Engineering" => (int.Parse(workItem["links"]["child"].ToString()) > 0
                    ? WorkItemCardType.StrategicEngineering
                    : WorkItemCardType.TacticalEngineering),
                _ => WorkItemCardType.Unanticipated
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using DataObjects;
using DataWrapper.ApiWrapper;
using DataWrapper.DatabaseAccess;
using Newtonsoft.Json.Linq;

namespace DataWrapper.Deserialize
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
        private readonly IDataAccess dataAccess;

        public KanbanizeDeserializer(IKanbanizeApiWrapper kanbanizeApiWrapper, IDataAccess dataAccess)
        {
            this.kanbanizeApiWrapper = kanbanizeApiWrapper;
            this.dataAccess = dataAccess;
        }

        public IEnumerable<WorkItemCard> WorkItemCardList(IEnumerable<JToken> jsonWorkItemCards, int boardId)
        {
            var workItemCardList = new List<WorkItemCard>();
            var i = 0;
            foreach (var item in jsonWorkItemCards)
            {
                if (item["columnid"].ToString().Contains("archive")) continue;
                workItemCardList.Add(WorkItemCard(item, boardId));
                Console.WriteLine($"Card Number: {++i}");
            }

            return workItemCardList;
        }

        public WorkItemCard WorkItemCard(JToken jsonWorkItemCard, int boardId)
        {
            var workItemCard = new WorkItemCard();

            workItemCard.Id = (int) jsonWorkItemCard["taskid"];
            workItemCard.Title = jsonWorkItemCard["title"].ToString();
            workItemCard.StartTime = DateTime.MinValue;
            workItemCard.FinishTime = DateTime.MaxValue;
            workItemCard.Type = GetCardType(jsonWorkItemCard);
            workItemCard.DevelopmentTeamName = boardId == 4
                ? "Enterprise Team"
                : "Assessment Team";
            workItemCard.CreatedOn = (DateTime) jsonWorkItemCard["createdat"];
            workItemCard.CreatedBy = jsonWorkItemCard["reporter"].ToString();
            workItemCard.LastChangedOn = (DateTime) jsonWorkItemCard["updatedat"];
            workItemCard.LastChangedBy = "";
            workItemCard.CurrentBoardColumn = jsonWorkItemCard["columnname"].ToString();
            workItemCard.CardState = GetCardState(jsonWorkItemCard);
            workItemCard.Impact = jsonWorkItemCard["priority"]?.ToString();
            workItemCard.CommentCount = jsonWorkItemCard["comments"].ToString() == ""
                ? 0
                : int.Parse(jsonWorkItemCard["comments"].Count().ToString());
            workItemCard.NumRevisions = 0;

            WorkItemCardHistoryItems(jsonWorkItemCard, workItemCard, boardId);

            var releases = dataAccess.GetReleasesBeforeDate(workItemCard.FinishTime);
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

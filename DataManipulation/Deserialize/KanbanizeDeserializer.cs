﻿﻿using System;
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
        IEnumerable<TaskItem> TaskItemList(IEnumerable<JToken> jsonTaskItems, int boardId);
        TaskItem TaskItem(JToken jsonTaskItem, int boardId);
        void TaskItemHistoryItems(JToken jsonTaskItem, TaskItem TaskItem, int boardId);
        TaskItemType GetCardType(JToken workItem);
    }

    public class KanbanizeDeserializer : IKanbanizeDeserializer
    {
        private readonly IKanbanizeApiRepository kanbanizeApiRepository;
        private readonly ReleaseRepository releaseRepository = new ReleaseRepository();
        private readonly ITaskItemRepository TaskItemRepository;
        private readonly IUserRepository userRepository;

        public KanbanizeDeserializer(IKanbanizeApiRepository kanbanizeApiRepository, IReleaseRepository releaseRepository, ITaskItemRepository TaskItemRepository, IUserRepository userRepository)
        {
            this.kanbanizeApiRepository = kanbanizeApiRepository;
            this.TaskItemRepository = TaskItemRepository;
            this.userRepository = userRepository;
        }

        public IEnumerable<TaskItem> TaskItemList(IEnumerable<JToken> jsonTaskItems, int boardId)
        {
            return (from item in jsonTaskItems
                where !item["columnid"].ToString().Contains("archive")
                      || (DateTime) item["updatedat"] < DateTime.Now.AddDays(-90)
                select TaskItem(item, boardId)).ToList();
        }

        public TaskItem TaskItem(JToken jsonTaskItem, int boardId)
        {
            var TaskItem = new TaskItem
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


            TaskItemHistoryItems(jsonTaskItem, TaskItem, boardId);

            var releases = releaseRepository.GetReleasesBeforeDate(TaskItem.FinishTime);
            var release = new Release();
            if (releases.Count > 0)
            {
                release = releases.First();
            }

            TaskItem.Release = release;

            return TaskItem;
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

        public void TaskItemHistoryItems(JToken jsonTaskItem, TaskItem TaskItem, int boardId)
        {
            var history = kanbanizeApiRepository.GetTaskItemHistory(jsonTaskItem, boardId);

            TaskItem.NumRevisions = history.Count();

            foreach (var item in history)
            {
                try
                {
                    if (item["historyevent"].ToString() == "Task moved")
                    {
                        if ((item["details"].ToString().Contains("to 'Top Priority'")
                             || item["details"].ToString().Contains("to 'Working'"))
                            && (TaskItem.StartTime > (DateTime) item["entrydate"]
                                || TaskItem.StartTime == DateTime.MinValue))
                        {
                            TaskItem.StartTime = (DateTime) item["entrydate"];
                        }
                        else if ((item["details"].ToString().Contains("to 'Ready for Prod Deploy'")
                                  || item["details"].ToString().Contains("to 'Released to Prod this week'")
                                  || item["details"].ToString().Contains("to 'Ready to Archive'"))
                                 && (TaskItem.FinishTime < (DateTime) item["entrydate"]
                                     || TaskItem.FinishTime == DateTime.MaxValue))
                        {
                            TaskItem.FinishTime = (DateTime) item["entrydate"];
                        }
                    }

                    if (TaskItem.LastChangedOn == (DateTime) item["entrydate"]
                        || TaskItem.LastChangedBy == "")
                    {
                        TaskItem.LastChangedBy = item["author"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            if (TaskItem.StartTime == DateTime.MinValue && TaskItem.CardState != "New")
            {
                TaskItem.StartTime = TaskItem.CreatedOn;
            }

            if (TaskItem.NumRevisions == 0)
            {
                if (TaskItem.CurrentBoardColumn == "Ready for Prod Deploy"
                    || TaskItem.CurrentBoardColumn == "Released to Prod this week"
                    || TaskItem.CurrentBoardColumn == "Ready to Archive")
                {
                    TaskItem.FinishTime = TaskItem.CreatedOn;
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

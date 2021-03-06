﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using Newtonsoft.Json.Linq;

namespace DataAccess.Deserialize.Kanbanize
{
    public interface IKanbanizeHistoryEventDeserializer
    {
        Task<Dictionary<int, TaskItem>> DeserializeHistoryEventsAsync(JToken jsonTaskList,
            Dictionary<int, TaskItem> taskItems);

        HistoryEvent DeserializeHistoryEvent(JToken jsonHistoryEvent);
        BoardColumn GetTaskItemBoardColumn(string details);
    }

    public class KanbanizeHistoryEventDeserializer : IKanbanizeHistoryEventDeserializer
    {
        public virtual async Task<Dictionary<int, TaskItem>> DeserializeHistoryEventsAsync(JToken jsonTaskList,
            Dictionary<int, TaskItem> taskItems)
        {
            foreach (var jsonTaskItem in jsonTaskList)
            {
                var historyEventList = new List<HistoryEvent>();

                foreach (var jsonHistoryEvent in GetJTokenHistoryEventList(jsonTaskItem))
                {
                    try
                    {
                        if ((jsonHistoryEvent["historyevent"].ToString() != "Task moved" &&
                             jsonHistoryEvent["historyevent"].ToString() != "Task created") ||
                            jsonHistoryEvent["details"].ToString().Contains("reordered")) continue;

                        var historyEvent = DeserializeHistoryEvent(jsonHistoryEvent);
                        historyEventList.Add(historyEvent);

                        var taskItem = taskItems[(int) jsonTaskItem["taskid"]];
                        var taskItemDeserializer = new KanbanizeTaskItemDeserializer();
                        var developerRepository = new DeveloperRepository();
                        taskItem = await taskItemDeserializer.FillInTaskItemStateDetailsAsync(historyEvent, taskItem);

                        if (taskItem.LastChangedOn < historyEvent.EventDate || taskItem.LastChangedOn == null)
                        {
                            taskItem.LastChangedOn = historyEvent.EventDate;
                            taskItem.LastChangedBy = await developerRepository.GetDeveloperByNameAsync(historyEvent.Author);
                        }

                        taskItem.NumRevisions++;

                        taskItems[historyEvent.TaskId] = taskItem;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Unable to process History Event for task: {jsonTaskItem["taskid"]}\nJson: {jsonTaskItem["historydetails"]["item"]} - {ex.Message}");
                    }
                }

                taskItems[(int) jsonTaskItem["taskid"]].HistoryEvents = historyEventList;
            }

            return taskItems;
        }

        private static IEnumerable<JToken> GetJTokenHistoryEventList(JToken jsonTaskItem)
        {
            List<JToken> jTokenHistoryEventList;
            try
            {
                var temp = jsonTaskItem["historydetails"]["item"][0]["taskid"];
                jTokenHistoryEventList = jsonTaskItem["historydetails"]["item"].ToList();
            }
            catch (Exception ex)
            {
                jTokenHistoryEventList = new List<JToken>
                {
                    jsonTaskItem["historydetails"]["item"]
                };
            }
            jTokenHistoryEventList.Reverse();

            return jTokenHistoryEventList;
        }

        public virtual HistoryEvent DeserializeHistoryEvent(JToken jsonHistoryEvent)
        {
            var historyEvent = new HistoryEvent();
            var taskItemDeserializer = new KanbanizeTaskItemDeserializer();

            historyEvent.Id = (int) jsonHistoryEvent["historyid"];
            historyEvent.EventType = jsonHistoryEvent["historyevent"].ToString();
            historyEvent.EventDate = (DateTimeOffset) jsonHistoryEvent["entrydate"];
            historyEvent.TaskItemColumn = historyEvent.EventType == "Task moved"
                ? GetTaskItemBoardColumn(jsonHistoryEvent["details"].ToString())
                : BoardColumn.Backlog;
            historyEvent.TaskItemState = taskItemDeserializer.GetTaskItemState(historyEvent.TaskItemColumn);
            historyEvent.Author = jsonHistoryEvent["author"].ToString();
            historyEvent.TaskId = (int) jsonHistoryEvent["taskid"];

            return historyEvent;
        }

        public BoardColumn GetTaskItemBoardColumn(string details)
        {
            var column = details.Substring(details.IndexOf("to '", StringComparison.Ordinal) + 4);
            column = column.Substring(0, column.Length - 1);

            var kanbanizeTaskItemDeserializer = new KanbanizeTaskItemDeserializer();
            return kanbanizeTaskItemDeserializer.GetBoardColumn(column);
        }
    }
}

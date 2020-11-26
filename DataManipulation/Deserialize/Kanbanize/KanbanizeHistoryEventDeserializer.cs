using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Objects;
using Newtonsoft.Json.Linq;

namespace DataAccess.Deserialize.Kanbanize
{
    public interface IKanbanizeHistoryEventDeserializer
    {
        Task<Dictionary<int, TaskItem>> DeserializeHistoryEventsAsync(JToken jsonTaskList,
            Dictionary<int, TaskItem> taskItems);

        HistoryEvent DeserializeHistoryEvent(JToken jsonHistoryEvent);
        string GetCardColumn(string details);
    }

    public class KanbanizeHistoryEventDeserializer : IKanbanizeHistoryEventDeserializer
    {
        public virtual async Task<Dictionary<int, TaskItem>> DeserializeHistoryEventsAsync(JToken jsonTaskList,
            Dictionary<int, TaskItem> taskItems)
        {
            foreach (var jsonTaskItem in jsonTaskList)
            {
                var historyEventList = new List<HistoryEvent>();
                if ((int) jsonTaskItem["taskid"] == 540)
                {
                    Console.WriteLine();
                }
                foreach (var jsonHistoryEvent in jsonTaskItem["historydetails"]["item"])
                {
                    try
                    {
                        if ((jsonHistoryEvent["historyevent"].ToString() == "Task moved"
                             || jsonHistoryEvent["historyevent"].ToString() == "Task created")
                            && !jsonHistoryEvent["details"].ToString().Contains("reordered"))
                        {
                            var historyEvent = DeserializeHistoryEvent(jsonHistoryEvent);
                            historyEventList.Add(historyEvent);

                            var taskItem = taskItems[(int) jsonTaskItem["taskid"]];
                            var taskItemDeserializer = new KanbanizeTaskItemDeserializer();
                            taskItem = await taskItemDeserializer.FillInTaskItemStateDetailsAsync(historyEvent, taskItem);

                            if (taskItem.LastChangedOn < historyEvent.EventDate || taskItem.LastChangedOn == null)
                            {
                                taskItem.LastChangedOn = historyEvent.EventDate;
                                taskItem.LastChangedBy = historyEvent.Author;
                            }

                            taskItem.NumRevisions++;
                            if (taskItem.StartTime == null && taskItem.State != TaskItemState.Backlog)
                            {
                                taskItem.StartTime = taskItem.CreatedOn;
                            }

                            taskItems[historyEvent.TaskId] = taskItem;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Unable to process History Event for task: {jsonTaskItem["taskid"]}\nJson: {jsonTaskItem["historydetails"]["item"]}");
                    }
                }

                taskItems[(int) jsonTaskItem["taskid"]].HistoryEvents = historyEventList;
            }

            return taskItems;
        }

        public virtual HistoryEvent DeserializeHistoryEvent(JToken jsonHistoryEvent)
        {
            var historyEvent = new HistoryEvent();
            var taskItemDeserializer = new KanbanizeTaskItemDeserializer();

            historyEvent.Id = (int) jsonHistoryEvent["historyid"];
            historyEvent.EventType = jsonHistoryEvent["historyevent"].ToString();
            historyEvent.EventDate = (DateTimeOffset) jsonHistoryEvent["entrydate"];
            historyEvent.TaskItemColumn = historyEvent.EventType == "Task moved"
                ? GetCardColumn(jsonHistoryEvent["details"].ToString())
                : "Backlog";
            historyEvent.TaskItemState = taskItemDeserializer.GetTaskItemState(historyEvent.TaskItemColumn);
            historyEvent.Author = jsonHistoryEvent["author"].ToString();
            historyEvent.TaskId = (int) jsonHistoryEvent["taskid"];

            return historyEvent;
        }

        public string GetCardColumn(string details)
        {
            var column = details.Substring(details.IndexOf("to '", StringComparison.Ordinal) + 4);
            column = column.Substring(0, column.Length - 1);

            return column;
        }
    }
}

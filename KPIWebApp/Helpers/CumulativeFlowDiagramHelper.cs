using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;

namespace KPIWebApp.Helpers
{
    interface ICumulativeFlowDiagramHelper
    {
        Task<CumulativeFlowData> GetCumulativeFlowDataAsync(DateTimeOffset startTime, DateTimeOffset finishTime,
            bool product, bool engineering, bool unanticipated, bool assessmentsTeam, bool enterpriseTeam);

        Task<List<TaskItem>> GetTaskItemsAsync(DateTimeOffset startTime, DateTimeOffset finishTime);
    }

    public class CumulativeFlowDiagramHelper : ICumulativeFlowDiagramHelper
    {
        private readonly ITaskItemRepository taskItemRepository;
        private readonly IHistoryEventRepository historyEventRepository;

        public CumulativeFlowDiagramHelper()
        {
            taskItemRepository = new TaskItemRepository();
            historyEventRepository = new HistoryEventRepository();
        }

        public CumulativeFlowDiagramHelper(ITaskItemRepository taskItemRepository, IHistoryEventRepository historyEventRepository)
        {
            this.taskItemRepository = taskItemRepository;
            this.historyEventRepository = historyEventRepository;
        }

        public async Task<CumulativeFlowData> GetCumulativeFlowDataAsync(DateTimeOffset startTime, DateTimeOffset finishTime,
            bool product, bool engineering, bool unanticipated, bool assessmentsTeam, bool enterpriseTeam)
        {
            var cumulativeFlowData = new CumulativeFlowData();

            var taskList = await GetTaskItemsAsync(startTime, finishTime);

            var startDate = GetStartDate(taskList, startTime);
            var finishDate = GetFinishDate(taskList, finishTime);
            var currentDate = startDate;
            var rawData = new Dictionary<DateTimeOffset, Dictionary<TaskItemState, int>>();
            var dates = new List<DateTimeOffset>();

            while (currentDate <= finishDate)
            {
                dates.Add(currentDate);
                cumulativeFlowData.dates.Add(currentDate.ToString("M"));
                rawData.Add(currentDate, new Dictionary<TaskItemState, int>
                {
                    {TaskItemState.Backlog, 0},
                    {TaskItemState.TopPriority, 0},
                    {TaskItemState.InProcess, 0},
                    {TaskItemState.Released, 0}
                });
                currentDate = currentDate.AddDays(1);
            }

            var taskItemHelper = new TaskItemHelper();

            foreach (var task in taskList)
            {
                if (taskItemHelper.TaskItemTypeIsSelected(product, engineering, unanticipated, task)
                && taskItemHelper.TaskItemDevTeamIsSelected(assessmentsTeam, enterpriseTeam, task))
                {
                    HistoryEvent lastHistoryEvent = null;
                    foreach (var historyEvent in task.HistoryEvents)
                    {
                        if (historyEvent.TaskItemState == TaskItemState.None
                        || historyEvent.EventDate.Date < startDate
                        || historyEvent.EventDate.Date > finishDate)
                        {
                            continue;
                        }

                        var date = historyEvent.EventDate.Date;

                        while (date <= finishDate)
                        {
                            rawData[date][historyEvent.TaskItemState]++;
                            if (lastHistoryEvent != null)
                            {
                                rawData[date][lastHistoryEvent.TaskItemState]--;
                            }

                            date = date.AddDays(1);
                        }

                        lastHistoryEvent = historyEvent;
                    }
                }
            }

            var backlogData = new List<int>();
            var topPriorityData = new List<int>();
            var inProcessData = new List<int>();
            var releasedData = new List<int>();

            foreach (var date in dates)
            {
                backlogData.Add(rawData[date][TaskItemState.Backlog]);
                topPriorityData.Add(rawData[date][TaskItemState.TopPriority]);
                inProcessData.Add(rawData[date][TaskItemState.InProcess]);
                releasedData.Add(rawData[date][TaskItemState.Released]);
            }

            cumulativeFlowData.data.Add(new CumulativeFlowDataRow{
                name = "Backlog",
                data = backlogData
            });
            cumulativeFlowData.data.Add(new CumulativeFlowDataRow{
                name = "Top Priority",
                data = topPriorityData
            });
            cumulativeFlowData.data.Add(new CumulativeFlowDataRow{
                name = "In Process",
                data = inProcessData
            });
            cumulativeFlowData.data.Add(new CumulativeFlowDataRow{
                name = "Released",
                data = releasedData
            });

            return cumulativeFlowData;
        }

        private static DateTimeOffset GetFinishDate(List<TaskItem> taskList, DateTimeOffset finishTime)
        {
            return taskList.Last().HistoryEvents.Last().EventDate.Date < finishTime
                ? taskList.Last().HistoryEvents.Last().EventDate
                : finishTime;
        }

        private static DateTime GetStartDate(List<TaskItem> taskList, DateTimeOffset startTime)
        {
            return taskList.First().HistoryEvents.First().EventDate.Date > startTime.Date
                ? taskList.First().HistoryEvents.First().EventDate.Date
                : startTime.Date;
        }

        public async Task<List<TaskItem>> GetTaskItemsAsync(DateTimeOffset startTime, DateTimeOffset finishTime)
        {
            var taskList = await taskItemRepository.GetTaskItemListAsync(startTime, finishTime);

            foreach (var task in taskList)
            {
                task.HistoryEvents = await historyEventRepository.GetHistoryEventsByTaskIdAsync(task.Id);
            }

            var removeList = taskList.Where(taskItem => taskItem.StartTime == null).ToList();

            foreach (var removeItem in removeList)
            {
                taskList.Remove(removeItem);
            }

            return taskList;
        }
    }

    public class CumulativeFlowData
    {
        public List<CumulativeFlowDataRow> data { get; set; } = new List<CumulativeFlowDataRow>();
        public List<string> dates { get; set; } = new List<string>();
    }

    public class CumulativeFlowDataRow
    {
        public string name { get; set; }
        public List<int> data { get; set; } = new List<int>();
    }
}

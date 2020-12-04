using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.Collections;
using DataAccess.DataRepositories;
using DataAccess.Objects;

namespace KPIWebApp.Helpers
{
    interface ICumulativeFlowDiagramHelper
    {
        Task<CumulativeFlowData> GetCumulativeFlowDataAsync(DateTime startTime, DateTime finishTime,
            bool product, bool engineering, bool unanticipated);

        Task<List<TaskItem>> GetTaskItems(DateTime startTime, DateTime finishTime);

        OrderedDictionary<DateTime, List<int>> SetupData(OrderedDictionary<DateTime, List<int>> data,
            DateTime startDate, DateTime finishDate);

        CumulativeFlowData FormatData(OrderedDictionary<DateTime, List<int>> data,
            DateTime startDate, DateTime finishDate);

        OrderedDictionary<DateTime, List<int>> ProcessTaskItemHistory(TaskItem taskItem,
            OrderedDictionary<DateTime, List<int>> data);

        DateTime UpdateCumulativeFlowData(HistoryEvent historyEvent, DateTime lastResultDate,
            OrderedDictionary<DateTime, List<int>> data);
    }

    public class CumulativeFlowDiagramHelper : ICumulativeFlowDiagramHelper
    {
        private readonly ITaskItemRepository taskItemRepository;

        public CumulativeFlowDiagramHelper()
        {
            taskItemRepository = new TaskItemRepository();
        }

        public CumulativeFlowDiagramHelper(ITaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
        }

        public async Task<CumulativeFlowData> GetCumulativeFlowDataAsync(DateTime startTime, DateTime finishTime,
            bool product, bool engineering, bool unanticipated)
        {
            var data = new OrderedDictionary<DateTime, List<int>>();

            var taskList = await GetTaskItems(startTime, finishTime);

            data = SetupData(data, GetStartDate(taskList), GetFinishDate(taskList));

            try
            {
                data = (from task in taskList
                        let type = task.Type
                        where (type == TaskItemType.Product && product)
                              || (type == TaskItemType.Engineering && engineering)
                              || (type == TaskItemType.Unanticipated && unanticipated)
                        select task)
                    .Aggregate(data, (current, task)
                        => ProcessTaskItemHistory(task, current));

                return FormatData(data, startTime.Date, finishTime.Date);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static DateTime GetFinishDate(List<TaskItem> taskList)
        {
            return taskList.Last().HistoryEvents.Last().EventDate.Date;
        }

        private static DateTime GetStartDate(List<TaskItem> taskList)
        {
            return taskList.First().HistoryEvents.First().EventDate.Date;
        }

        public async Task<List<TaskItem>> GetTaskItems(DateTime startTime, DateTime finishTime)
        {
            var taskList = await taskItemRepository.GetTaskItemListAsync(startTime, finishTime);

            foreach (var task in taskList)
            {
                task.HistoryEvents = await taskItemRepository.GetHistoryEventsByTaskIdAsync(task.Id);
            }

            var removeList = taskList.Where(taskItem => taskItem.StartTime == null).ToList();

            foreach (var removeItem in removeList)
            {
                taskList.Remove(removeItem);
            }

            return taskList;
        }

        public OrderedDictionary<DateTime, List<int>> SetupData(
            OrderedDictionary<DateTime, List<int>> data,
            DateTime startDate, DateTime finishDate)
        {
            var date = startDate;
            do
            {
                data.Add(date!, new List<int> {0, 0, 0, 0});
                date = date.AddDays(1);
            } while (date <= finishDate);

            return data;
        }

        public CumulativeFlowData FormatData(OrderedDictionary<DateTime, List<int>> data,
            DateTime startDate, DateTime finishDate)
        {
            var newData = new List<CumulativeFlowDataRow>();
            var dateList = new List<string>();

            for (var i = 0; i < data.Values.First().Count; ++i)
            {
                newData.Add(new CumulativeFlowDataRow
                {
                    data = new List<int>(),
                    name = ((TaskItemState) i).ToString()
                });
            }

            for (var i = 0; i < data.Count; i++)
            {
                if (data.Keys.ElementAt(i) < startDate || data.Keys.ElementAt(i) > finishDate)
                {
                    data.Remove(data.ElementAt(i));
                }
                else
                {
                    newData[0].data.Add(data.Values.ElementAt(i)[0]);
                    newData[1].data.Add(data.Values.ElementAt(i)[1]);
                    newData[2].data.Add(data.Values.ElementAt(i)[2]);
                    newData[3].data.Add(data.Values.ElementAt(i)[3]);

                    dateList.Add(data.Keys.ElementAt(i).ToString("dd MMMM"));
                }
            }

            return new CumulativeFlowData
            {
                data = newData,
                dates = dateList
            };
        }

        public OrderedDictionary<DateTime, List<int>> ProcessTaskItemHistory(TaskItem taskItem,
            OrderedDictionary<DateTime, List<int>> data)
        {
            var lastResultDate = data.Keys.First();
            foreach (var historyEvent in taskItem.HistoryEvents)
            {
                lastResultDate = UpdateCumulativeFlowData(historyEvent, lastResultDate, data);
                if (lastResultDate == DateTime.MinValue)
                {
                    break;
                }
            }

            return data;
        }

        public DateTime UpdateCumulativeFlowData(HistoryEvent historyEvent, DateTime lastResultDate,
            OrderedDictionary<DateTime, List<int>> data)
        {
            if (historyEvent.EventDate < data.Keys.First()) return data.Keys.First();
            if (historyEvent.EventDate > data.Keys.Last()) return data.Keys.Last();

            if (historyEvent.EventType == "Task created")
            {
                data[historyEvent.EventDate.Date][(int) historyEvent.TaskItemState]++;
                return historyEvent.EventDate.Date;
            }

            if (historyEvent.TaskItemState == TaskItemState.None) return lastResultDate;

            if (historyEvent.EventDate.Date == lastResultDate)
            {
                data[historyEvent.EventDate.Date][(int) historyEvent.TaskItemState]--;
            }

            data[historyEvent.EventDate.Date][(int) historyEvent.TaskItemState]++;

            var checkDate = historyEvent.EventDate.AddDays(-1).Date;
            while (checkDate > lastResultDate)
            {
                data[checkDate][(int) historyEvent.TaskItemState - 1]++;
                checkDate = checkDate.AddDays(-1);
            }

            if (historyEvent.TaskItemState != TaskItemState.Released) return historyEvent.EventDate!.Date;

            checkDate = historyEvent.EventDate.AddDays(1).Date;
            while (checkDate <= data.Keys.Last())
            {
                data[checkDate][(int) historyEvent.TaskItemState]++;
                checkDate = checkDate.AddDays(1);
            }

            return DateTime.MinValue;

        }
    }

    public class CumulativeFlowData
    {
        public List<CumulativeFlowDataRow> data { get; set; }
        public List<string> dates { get; set; }
    }

    public class CumulativeFlowDataRow
    {
        public string name { get; set; }
        public List<int> data { get; set; }
    }
}

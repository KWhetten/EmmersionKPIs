using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;

namespace KPIWebApp.Helpers
{
    public class CumulativeFlowDiagramHelper
    {
        private readonly ITaskItemRepository taskItemRepository;
        private DateTimeOffset? startDate;
        private DateTimeOffset? finishDate;
        private List<DateTimeOffset> dates;

        public CumulativeFlowDiagramHelper(ITaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
        }

        public CumulativeFlowDiagramHelper()
        {
            taskItemRepository = new TaskItemRepository();
        }

        public async Task<CumulativeFlowData> GetCumulativeFlowData(DateTimeOffset startTime,
            DateTimeOffset finishTime)
        {
            var taskList = await taskItemRepository.GetTaskItemListAsync(startTime, finishTime);
            var removeList = new List<TaskItem>();

            foreach (var taskItem in taskList)
            {
                taskItem.HistoryEvents = await taskItemRepository.GetHistoryEventsByTaskIdAsync(taskItem.Id);
                if (taskItem.HistoryEvents.Count == 0)
                {
                    removeList.Add(taskItem);
                }
            }

            foreach (var removeItem in removeList)
            {
                taskList.Remove(removeItem);
            }

            startDate = taskList.First().CreatedOn?.ToUniversalTime().Date;
            finishDate = finishTime.ToUniversalTime().Date;

            try
            {
                var cumulativeFlowData = BuildCumulativeFlowDataStructure();

                var cumulativeFlowDataFinal = taskList.Aggregate(cumulativeFlowData.data,
                    (current, taskItem) => ProcessTaskItemHistory(taskItem, current));

                cumulativeFlowData.data = cumulativeFlowDataFinal;

                return cumulativeFlowData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private CumulativeFlowData BuildCumulativeFlowDataStructure()
        {
            var cumulativeFlowData = new CumulativeFlowData
            {
                data = new List<CumulativeFlowDataRow>
                {
                    new CumulativeFlowDataRow
                    {
                        name = "Backlog",
                        data = new List<int>()
                    },
                    new CumulativeFlowDataRow
                    {
                        name = "Top Priority",
                        data = new List<int>()
                    },
                    new CumulativeFlowDataRow
                    {
                        name = "In Process",
                        data = new List<int>()
                    },
                    new CumulativeFlowDataRow
                    {
                        name = "Released",
                        data = new List<int>()
                    }
                }
            };
            dates = new List<DateTimeOffset>();
            if (startDate?.Date != null)
            {
                var currentDate = new DateTimeOffset((DateTime) startDate?.Date);

                do
                {
                    if (currentDate.DayOfWeek != DayOfWeek.Saturday
                        && currentDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        dates.Add(new DateTimeOffset(currentDate.Date));
                        cumulativeFlowData.data[0].data.Add(0);
                        cumulativeFlowData.data[1].data.Add(0);
                        cumulativeFlowData.data[2].data.Add(0);
                        cumulativeFlowData.data[3].data.Add(0);
                    }

                    currentDate = currentDate.AddDays(1);
                } while (currentDate <= finishDate);
            }

            cumulativeFlowData.dates = dates;

            return cumulativeFlowData;
        }

        private List<CumulativeFlowDataRow> ProcessTaskItemHistory(TaskItem taskItem,
            List<CumulativeFlowDataRow> cumulativeFlowData)
        {
            var dateNum = 0;
            var inState = InState.None;
            var historyEventIndex = 0;

            foreach (var date in dates)
            {
                try
                {
                    var currentHistoryItem = taskItem.HistoryEvents[historyEventIndex];

                    while (currentHistoryItem.EventDate.Date == date)
                    {
                        ++historyEventIndex;
                        inState = GetState(currentHistoryItem.TaskItemState);
                        currentHistoryItem = taskItem.HistoryEvents[historyEventIndex];
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }

                if (inState != InState.None)
                {
                    cumulativeFlowData[(int) inState].data[dateNum]++;
                }

                ++dateNum;
            }

            return cumulativeFlowData;
        }

        private static InState GetState(string state)
        {
            return state switch
            {
                "Backlog" => InState.Backlog,
                "Top Priority" => InState.TopPriority,
                "In Process" => InState.InProcess,
                "Released" => InState.Released,
                _ => InState.None
            };
        }

        private enum InState
        {
            None = -1,
            Backlog = 0,
            TopPriority = 1,
            InProcess = 2,
            Released = 3
        }
    }

    public class CumulativeFlowData
    {
        public List<CumulativeFlowDataRow> data { get; set; }
        public List<DateTimeOffset> dates { get; set; }
    }

    public class CumulativeFlowDataRow
    {
        public string name { get; set; }
        public List<int> data { get; set; }
    }
}

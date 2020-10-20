using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using Microsoft.TeamFoundation.Common;

namespace KPIWebApp.Helpers
{
    public class BoxGraphHelper
    {
        private readonly ITaskItemRepository taskItemRepository;

        public BoxGraphHelper()
        {
            taskItemRepository = new TaskItemRepository(new DatabaseConnection());
        }

        public BoxGraphHelper(ITaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
        }

        public async Task<BoxGraphData> GetLeadTimeBoxGraphData(DateTime startDate, DateTime finishDate)
        {
            var boxGraphData = new BoxGraphData
            {
                Entries = new List<BoxGraphDataEntry>
                {
                    new BoxGraphDataEntry(),
                    new BoxGraphDataEntry(),
                    new BoxGraphDataEntry(),
                    new BoxGraphDataEntry()
                },
                Outliers = new List<object[]>()
            };

            var taskItems = await taskItemRepository.GetTaskItemListAsync(startDate, finishDate);
            var taskItemsByType = new[]
            {
                new List<TaskItem>(),
                new List<TaskItem>(),
                new List<TaskItem>(),
                new List<TaskItem>()
            };

            foreach (var item in taskItems)
            {
                taskItemsByType[(int) item.Type - 1].Add(item);
            }

            foreach (var itemList in taskItemsByType)
            {
                if (itemList.IsNullOrEmpty()) continue;

                var sortedItemList = SortByLeadTime(itemList);

                var index = (int) itemList[0].Type - 1;
                var boxGraphDataEntry = boxGraphData.Entries[index];

                boxGraphDataEntry.TaskItemType = itemList[0].Type;

                var lowerQuartileIndex = (int) ((sortedItemList.Count - 1) * (1m / 4m));
                var middleIndex = (int) ((sortedItemList.Count - 1) / 2m);
                var upperQuartileIndex = (int) ((sortedItemList.Count - 1) * (3m / 4m));

                boxGraphDataEntry.LowerQuartile = sortedItemList[lowerQuartileIndex].LeadTimeHours;
                boxGraphDataEntry.Median = sortedItemList[middleIndex].LeadTimeHours;
                boxGraphDataEntry.UpperQuartile = sortedItemList[upperQuartileIndex].LeadTimeHours;

                var iqr = sortedItemList[upperQuartileIndex].LeadTimeHours -
                          sortedItemList[lowerQuartileIndex].LeadTimeHours;

                var minWhiskerValue = boxGraphDataEntry.LowerQuartile - (iqr * 1.5m) > 0
                    ? boxGraphDataEntry.LowerQuartile - (iqr * 1.5m)
                    : 0;
                var maxWhiskerValue = boxGraphDataEntry.UpperQuartile + (iqr * 1.5m);

                var outliers = boxGraphData.Outliers;
                (boxGraphDataEntry.Minimum, outliers) = GetMinimumAndOutliers(sortedItemList, minWhiskerValue, outliers);
                (boxGraphDataEntry.Maximum, outliers) = GetMaximumAndSetOutliers(sortedItemList, maxWhiskerValue, outliers);

                boxGraphData.Outliers = outliers;
            }

            return boxGraphData;
        }

        private (decimal, List<object[]>) GetMinimumAndOutliers(List<TaskItem> sortedItemList, decimal minWhiskerValue, List<object[]> outliers)
        {
            decimal minimum;
            for (var i = 0;; i++)
            {
                var leadTimeHours = sortedItemList[i].LeadTimeHours;
                if (leadTimeHours < minWhiskerValue)
                {
                    var type = (int) sortedItemList[i].Type - 1;
                    outliers.Add(new object[]
                    {
                        type,
                        leadTimeHours
                    });
                    continue;
                }

                minimum = sortedItemList[i].LeadTimeHours;
                break;
            }

            return (minimum, outliers);
        }

        private (decimal, List<object[]>) GetMaximumAndSetOutliers(List<TaskItem> itemList, decimal maxWhiskerValue, List<object[]> outliers)
        {
            decimal maximum;
            for (var i = 1;; i++)
            {
                var leadTimeHours = itemList[^i].LeadTimeHours;
                if (itemList[^i].LeadTimeHours > maxWhiskerValue)
                {
                    var type = (int) itemList[^i].Type - 1;
                    outliers.Add(new object[]
                    {
                        type,
                        leadTimeHours
                    });
                    continue;
                }

                maximum = itemList[^i].LeadTimeHours;
                break;
            }

            return (maximum, outliers);
        }

        private List<TaskItem> SortByLeadTime(List<TaskItem> itemList)
        {
            foreach (var item in itemList)
            {
                item.CalculateLeadTimeHours();
            }

            return itemList.OrderBy(o => o.LeadTimeHours).ToList();
        }
    }

    public class BoxGraphData
    {
        public List<BoxGraphDataEntry> Entries { get; set; }
        public List<object[]> Outliers { get; set; }
    }

    public class BoxGraphDataEntry
    {
        public TaskItemType TaskItemType { get; set; }
        public decimal Minimum { get; set; }
        public decimal LowerQuartile { get; set; }
        public decimal Median { get; set; }
        public decimal UpperQuartile { get; set; }
        public decimal Maximum { get; set; }
    }
}

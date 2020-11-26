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
        public bool Product { get; set; }
        public bool Engineering { get; set; }
        public bool Unanticipated { get; set; }

        public BoxGraphHelper()
        {
            taskItemRepository = new TaskItemRepository();
        }

        public BoxGraphHelper(ITaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
        }

        public async Task<BoxGraphData> GetLeadTimeBoxGraphData(DateTimeOffset startDate, DateTimeOffset finishDate, bool product, bool engineering, bool unanticipated)
        {
            Product = product;
            Engineering = engineering;
            Unanticipated = unanticipated;

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
                new List<TaskItem>()
            };

            var finalTaskItemList = new List<TaskItem>();

            foreach (var item in taskItems)
            {
                taskItemsByType[(int) item.Type - 1].Add(item);
                if ((item.Type == TaskItemType.Product && Product)
                    || (item.Type == TaskItemType.Engineering && Engineering)
                    || (item.Type == TaskItemType.Unanticipated && Unanticipated))
                {
                    finalTaskItemList.Add(item);
                }
            }

            foreach (var itemList in taskItemsByType)
            {
                var sortedItemList = SortByLeadTime(itemList);
                var index = (int) itemList[0].Type - 1;
                boxGraphData = CalculateBoxGraphData(sortedItemList, boxGraphData, index);
            }

            boxGraphData = CalculateBoxGraphData(SortByLeadTime(finalTaskItemList), boxGraphData, 3);
            boxGraphData.Entries[3].TaskItemType = "Selected Task Items";

            return boxGraphData;
        }

        public BoxGraphData CalculateBoxGraphData(List<TaskItem> itemList, BoxGraphData boxGraphData, int index)
        {
            if (itemList.IsNullOrEmpty()) return null;

            var boxGraphDataEntry = boxGraphData.Entries[index];

            boxGraphDataEntry.TaskItemType = itemList[0].Type.ToString();

            var lowerQuartileIndex = (int) ((itemList.Count - 1) * (1m / 4m));
            var middleIndex = (int) ((itemList.Count - 1) / 2m);
            var upperQuartileIndex = (int) ((itemList.Count - 1) * (3m / 4m));

            boxGraphDataEntry.LowerQuartile = itemList[lowerQuartileIndex].LeadTimeHours;
            boxGraphDataEntry.Median = itemList[middleIndex].LeadTimeHours;
            boxGraphDataEntry.UpperQuartile = itemList[upperQuartileIndex].LeadTimeHours;

            var iqr = itemList[upperQuartileIndex].LeadTimeHours -
                      itemList[lowerQuartileIndex].LeadTimeHours;

            var minWhiskerValue = boxGraphDataEntry.LowerQuartile - (iqr * 1.5m) > 0
                ? boxGraphDataEntry.LowerQuartile - (iqr * 1.5m)
                : 0;
            var maxWhiskerValue = boxGraphDataEntry.UpperQuartile + (iqr * 1.5m);

            var outliers = boxGraphData.Outliers;
            (boxGraphDataEntry.Minimum, outliers) = GetMinimumAndOutliers(itemList, minWhiskerValue, outliers, index);
            (boxGraphDataEntry.Maximum, outliers) = GetMaximumAndSetOutliers(itemList, maxWhiskerValue, outliers, index);

            boxGraphData.Outliers = outliers;

            return boxGraphData;
        }

        public (decimal, List<object[]>) GetMinimumAndOutliers(List<TaskItem> sortedItemList, decimal minWhiskerValue,
            List<object[]> outliers, int index)
        {
            decimal minimum;
            for (var i = 0;; i++)
            {
                var leadTimeHours = sortedItemList[i].LeadTimeHours;
                if (leadTimeHours < minWhiskerValue)
                {
                    if (leadTimeHours < 150 && leadTimeHours >= 0)
                    {
                        outliers.Add(new object[]
                        {
                            index,
                            leadTimeHours
                        });
                    }

                    continue;
                }

                minimum = sortedItemList[i].LeadTimeHours;
                break;
            }

            return (minimum, outliers);
        }

        private (decimal, List<object[]>) GetMaximumAndSetOutliers(List<TaskItem> itemList, decimal maxWhiskerValue,
            List<object[]> outliers, int index)
        {
            decimal maximum;
            for (var i = 1;; i++)
            {
                var leadTimeHours = itemList[^i].LeadTimeHours;
                if (itemList[^i].LeadTimeHours > maxWhiskerValue)
                {
                    if (leadTimeHours < 150)
                    {
                        outliers.Add(new object[]
                        {
                            index,
                            leadTimeHours
                        });
                    }

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
        public string TaskItemType { get; set; }
        public decimal Minimum { get; set; }
        public decimal LowerQuartile { get; set; }
        public decimal Median { get; set; }
        public decimal UpperQuartile { get; set; }
        public decimal Maximum { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Models;

namespace KPIWebApp.Helpers
{
    public class OverviewHelper
    {
        private readonly ITaskItemRepository taskItemRepository = new TaskItemRepository();
        private readonly IReleaseRepository releaseRepository = new ReleaseRepository();
        private readonly ILeadTimeHelper leadTimeHelper = new LeadTimeHelper();
        public bool Product { get; set; } = true;
        public bool Engineering { get; set; } = true;
        public bool Unanticipated { get; set; } = true;

        public OverviewHelper(){ }

        public OverviewHelper(ITaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
        }

        public OverviewHelper(bool product, bool engineering, bool unanticipated)
        {
            Product = product;
            Engineering = engineering;
            Unanticipated = unanticipated;
        }

        public OverviewHelper(IReleaseRepository releaseRepository)
        {
            this.releaseRepository = releaseRepository;
        }

        public async Task<List<TaskItem>> GetTaskItemData(DateTimeOffset startDate, DateTimeOffset finishDate)
        {
            return await taskItemRepository.GetTaskItemListAsync(startDate, finishDate);
        }

        public async Task<List<Release>> GetReleaseData(DateTimeOffset startDate, DateTimeOffset finishDate)
        {
            return (await releaseRepository.GetReleaseListAsync(startDate, finishDate)).ToList();
        }

        public async Task<OverviewData> GetOverviewDataAsync(DateTimeOffset startDate, DateTimeOffset finishDate,
            bool product, bool engineering, bool unanticipated)
        {
            Product = product;
            Engineering = engineering;
            Unanticipated = unanticipated;

            var taskItemList = await GetTaskItemData(startDate, finishDate);
            var releaseList = await GetReleaseData(startDate, finishDate);

            var releaseHelper = new ReleaseHelper();
            var overviewData = new OverviewData();

            overviewData = PopulateOverviewData(overviewData, taskItemList);
            overviewData = releaseHelper.PopulateOverviewData(overviewData, releaseList, finishDate);

            return overviewData;
        }

        public virtual OverviewData PopulateOverviewData(OverviewData overviewData, List<TaskItem> taskItemList)
        {
            var count = 0;

            if (taskItemList.Count == 0)
            {
                return overviewData;
            }

            foreach (var item in taskItemList.Where(
                item => (item.Type == TaskItemType.Product && Product)
                        || (item.Type == TaskItemType.Engineering && Engineering)
                        || (item.Type == TaskItemType.Unanticipated && Unanticipated)))
            {
                if (item.StartTime != null && item.FinishTime != null)
                {
                    item.LeadTimeHours = leadTimeHelper.CalculateLeadTimeHours(item);
                }

                if (item.LeadTimeHours > overviewData.LongestLeadTime
                    && item.StartTime != null &&
                    item.FinishTime != null)
                {
                    overviewData.LongestLeadTime = item.LeadTimeHours;
                }

                if (item.LeadTimeHours < overviewData.ShortestLeadTime
                    && item.StartTime != null
                    && item.FinishTime != null
                    && item.LeadTimeHours > 0)
                {
                    overviewData.ShortestLeadTime = item.LeadTimeHours;
                }

                count++;
            }

            var averageLeadTimeTaskItems = taskItemList.Where(taskItem =>
                taskItem.StartTime != null && taskItem.FinishTime != null).ToList();

            overviewData.AverageLeadTime = (averageLeadTimeTaskItems.Sum(item => item.LeadTimeHours) /
                                            averageLeadTimeTaskItems.Count);

            overviewData.TotalCards = count;
            overviewData.AverageLeadTime =
                decimal.Round(overviewData.AverageLeadTime, 2, MidpointRounding.AwayFromZero);
            overviewData.LongestLeadTime =
                decimal.Round(overviewData.LongestLeadTime, 2, MidpointRounding.AwayFromZero);
            overviewData.ShortestLeadTime =
                decimal.Round(overviewData.ShortestLeadTime, 2, MidpointRounding.AwayFromZero);

            return overviewData;
        }
    }
}

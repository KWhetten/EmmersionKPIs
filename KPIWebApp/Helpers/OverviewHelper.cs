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
        private bool Product { get; set; } = true;
        private bool Engineering { get; set; } = true;
        private bool Unanticipated { get; set; } = true;
        private bool EnterpriseTeam { get; set; } = true;
        private bool AssessmentsTeam { get; set; } = true;

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

        public async Task<TaskItemOverviewData> GetTaskItemOverviewDataAsync(DateTimeOffset startDate, DateTimeOffset finishDate,
            bool product, bool engineering, bool unanticipated, bool assessmentsTeam, bool enterpriseTeam)
        {
            Product = product;
            Engineering = engineering;
            Unanticipated = unanticipated;
            AssessmentsTeam = assessmentsTeam;
            EnterpriseTeam = enterpriseTeam;

            var taskItemList = await GetTaskItemData(startDate, finishDate);

            var overviewData = new TaskItemOverviewData();

            overviewData = PopulateOverviewData(overviewData, taskItemList);

            return overviewData;
        }


        public async Task<ReleaseOverviewData> GetReleaseOverviewDataAsync(DateTimeOffset startDate, DateTimeOffset finishDate,
            bool product, bool engineering, bool unanticipated)
        {
            Product = product;
            Engineering = engineering;
            Unanticipated = unanticipated;

            var releaseList = await GetReleaseData(startDate, finishDate);

            var releaseHelper = new ReleaseHelper();
            var overviewData = new ReleaseOverviewData();

            overviewData = releaseHelper.PopulateOverviewData(overviewData, releaseList, finishDate);

            return overviewData;
        }

        public virtual TaskItemOverviewData PopulateOverviewData(TaskItemOverviewData taskItemOverviewData, List<TaskItem> taskItemList)
        {
            var count = 0;

            if (taskItemList.Count == 0)
            {
                return taskItemOverviewData;
            }

            var taskItemHelper = new TaskItemHelper();

            foreach (var item in taskItemList.Where(
                item => taskItemHelper.TaskItemTypeIsSelected(Product, Engineering, Unanticipated, item)
                        && taskItemHelper.TaskItemDevTeamIsSelected(AssessmentsTeam, EnterpriseTeam, item)))
            {
                if (item.StartTime != null && item.FinishTime != null)
                {
                    item.LeadTimeHours = leadTimeHelper.CalculateLeadTimeHours(item);
                }

                if (item.LeadTimeHours > taskItemOverviewData.LongestLeadTime
                    && item.StartTime != null &&
                    item.FinishTime != null)
                {
                    taskItemOverviewData.LongestLeadTime = item.LeadTimeHours;
                }

                if (item.LeadTimeHours < taskItemOverviewData.ShortestLeadTime
                    && item.StartTime != null
                    && item.FinishTime != null
                    && item.LeadTimeHours > 0)
                {
                    taskItemOverviewData.ShortestLeadTime = item.LeadTimeHours;
                }

                count++;
            }

            var averageLeadTimeTaskItems = taskItemList.Where(taskItem =>
                taskItem.StartTime != null && taskItem.FinishTime != null).ToList();

            taskItemOverviewData.AverageLeadTime = (averageLeadTimeTaskItems.Sum(item => item.LeadTimeHours) /
                                            averageLeadTimeTaskItems.Count);

            taskItemOverviewData.TotalCards = count;
            taskItemOverviewData.AverageLeadTime =
                decimal.Round(taskItemOverviewData.AverageLeadTime, 2, MidpointRounding.AwayFromZero);
            taskItemOverviewData.LongestLeadTime =
                decimal.Round(taskItemOverviewData.LongestLeadTime, 2, MidpointRounding.AwayFromZero);
            taskItemOverviewData.ShortestLeadTime =
                decimal.Round(taskItemOverviewData.ShortestLeadTime, 2, MidpointRounding.AwayFromZero);

            return taskItemOverviewData;
        }
    }
}

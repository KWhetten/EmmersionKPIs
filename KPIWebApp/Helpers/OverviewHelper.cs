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
        private readonly ITaskItemRepository taskItemRepository;
        private readonly IReleaseRepository releaseRepository;
        private readonly ILeadTimeHelper leadTimeHelper;
        private readonly IReleaseHelper releaseHelper;
        private readonly ITaskItemHelper taskItemHelper;
        private bool Product { get; set; } = true;
        private bool Engineering { get; set; } = true;
        private bool Unanticipated { get; set; } = true;
        private bool EnterpriseTeam { get; set; } = true;
        private bool AssessmentsTeam { get; set; } = true;

        public OverviewHelper()
        {
            taskItemRepository = new TaskItemRepository();
            releaseRepository = new ReleaseRepository();
            leadTimeHelper = new LeadTimeHelper();
            releaseHelper = new ReleaseHelper();
            taskItemHelper = new TaskItemHelper();
        }
        public OverviewHelper(ITaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
            releaseRepository = new ReleaseRepository();
            leadTimeHelper = new LeadTimeHelper();
            releaseHelper = new ReleaseHelper();
            taskItemHelper = new TaskItemHelper();
        }
        public OverviewHelper(IReleaseRepository releaseRepository)
        {
            taskItemRepository = new TaskItemRepository();
            this.releaseRepository = releaseRepository;
            leadTimeHelper = new LeadTimeHelper();
            releaseHelper = new ReleaseHelper();
            taskItemHelper = new TaskItemHelper();
        }

        public OverviewHelper(IReleaseRepository releaseRepository, IReleaseHelper releaseHelper)
        {
            taskItemRepository = new TaskItemRepository();
            this.releaseRepository = releaseRepository;
            leadTimeHelper = new LeadTimeHelper();
            this.releaseHelper = releaseHelper;
            taskItemHelper = new TaskItemHelper();
        }

        public async Task<List<TaskItem>> GetTaskItemData(DateTimeOffset startDate, DateTimeOffset finishDate)
        {
            var result = await taskItemRepository.GetTaskItemListAsync(startDate, finishDate);
            return result;
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

            var overviewData = PopulateOverviewData(taskItemList);

            return overviewData;
        }


        public async Task<ReleaseOverviewData> GetReleaseOverviewDataAsync(DateTimeOffset startDate, DateTimeOffset finishDate,
            bool assessmentsTeam, bool enterpriseTeam)
        {
            AssessmentsTeam = assessmentsTeam;
            EnterpriseTeam = enterpriseTeam;

            var releaseList = await GetReleaseData(startDate, finishDate);

            var selectedReleases = releaseList.Where(release => releaseHelper.DevTeamForReleaseIsSelected(assessmentsTeam, enterpriseTeam, release)).ToList();

            var overviewData = new ReleaseOverviewData();

            if (selectedReleases.Count != 0)
            {
                overviewData = releaseHelper.PopulateOverviewData(selectedReleases, finishDate,
                    assessmentsTeam, enterpriseTeam);
            }

            return overviewData;
        }

        public virtual TaskItemOverviewData PopulateOverviewData(List<TaskItem> taskItemList)
        {
            var count = 0;
            var taskItemOverviewData = new TaskItemOverviewData();

            if (taskItemList.Count == 0)
            {
                taskItemOverviewData.ShortestLeadTime = 0;
                taskItemOverviewData.LongestLeadTime = 0;
                return taskItemOverviewData;
            }

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
                taskItem.StartTime != null && taskItem.FinishTime != null
                && taskItemHelper.TaskItemTypeIsSelected(Product, Engineering, Unanticipated, taskItem)
                && taskItemHelper.TaskItemDevTeamIsSelected(AssessmentsTeam, EnterpriseTeam, taskItem)).ToList();

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

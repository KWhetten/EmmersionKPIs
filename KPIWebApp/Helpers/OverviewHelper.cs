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

        public OverviewHelper()
        {
            taskItemRepository = new TaskItemRepository();
            releaseRepository = new ReleaseRepository();
        }

        public OverviewHelper(ITaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
        }

        public OverviewHelper(IReleaseRepository releaseRepository)
        {
            this.releaseRepository = releaseRepository;
        }

        public OverviewHelper(ITaskItemRepository taskItemRepository, IReleaseRepository releaseRepository)
        {
            this.taskItemRepository = taskItemRepository;
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

        public async Task<OverviewData> GetOverviewData(DateTimeOffset startDate, DateTimeOffset finishDate)
        {
            var taskItemList = await GetTaskItemData(startDate, finishDate);
            var releaseList = await GetReleaseData(startDate, finishDate);

            var leadTimeHelper = new LeadTimeHelper();
            var releaseHelper = new ReleaseHelper();
            var overviewData = new OverviewData();

            overviewData = leadTimeHelper.PopulateOverviewData(overviewData, taskItemList);
            overviewData = releaseHelper.PopulateOverviewData(overviewData, releaseList, finishDate);

            return overviewData;
        }
    }
}

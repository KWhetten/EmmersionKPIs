using System;
using System.Collections.Generic;
using System.Linq;
using DataObjects;

namespace KPIWebApp.Models
{
    public class OverviewData
    {
        public decimal AverageLeadTime { get; set; }
        public decimal LongestLeadTime { get; set; }
        public decimal ShortestLeadTime { get; set; } = decimal.MaxValue;

        public int TotalDeploys { get; set; }
        public int SuccessfulDeploys { get; set; }
        public int RolledBackDeploys { get; set; }
        public decimal DeployFrequency { get; set; }
        public decimal MeanTimeToRestore { get; set; }
        public decimal ChangeFailPercentage { get; set; }

        public OverviewData(IReadOnlyCollection<WorkItemCard> workItemCardList, IEnumerable<Release> releaseList)
        {
            var averageLeadTimeWorkItemCards = workItemCardList.Where(workItemCard =>
                workItemCard.StartTime != DateTime.MinValue && workItemCard.FinishTime != DateTime.MinValue).ToList();
            AverageLeadTime = averageLeadTimeWorkItemCards.Sum(item => item.LeadTimeHours) /
                              averageLeadTimeWorkItemCards.Count;

            foreach (var item in workItemCardList)
            {
                if (item.LeadTimeHours > LongestLeadTime
                    && item.StartTime != DateTime.MinValue &&
                    item.FinishTime != DateTime.MaxValue)
                {
                    LongestLeadTime = item.LeadTimeHours;
                }
                if (item.LeadTimeHours < ShortestLeadTime
                    && item.StartTime != DateTime.MinValue
                    && item.FinishTime != DateTime.MaxValue
                    && item.LeadTimeHours > 0)
                {
                    ShortestLeadTime = item.LeadTimeHours;
                }
            }

            AverageLeadTime = decimal.Round(AverageLeadTime, 2, MidpointRounding.AwayFromZero);
            LongestLeadTime = decimal.Round(LongestLeadTime, 2, MidpointRounding.AwayFromZero);
            ShortestLeadTime = decimal.Round(ShortestLeadTime, 2, MidpointRounding.AwayFromZero);

            TotalDeploys = releaseList.Count();
            SuccessfulDeploys = 0;
            RolledBackDeploys = 0;
            DeployFrequency = 0;
            MeanTimeToRestore = 0;
            ChangeFailPercentage = 0;
        }
    }
}

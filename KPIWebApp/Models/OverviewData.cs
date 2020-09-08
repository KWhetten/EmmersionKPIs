using System.Collections.Generic;
using System.Linq;
using DataObjects;

namespace KPIWebApp.Models
{
    public class OverviewData
    {
        public double AverageLeadTime { get; set; }
        public double LongestLeadTime { get; set; }
        public double ShortestLeadTime { get; set; }

        public int TotalDeploys { get; set; }
        public int SuccessfulDeploys { get; set; }
        public int RolledBackDeploys { get; set; }
        public int DeployFrequency { get; set; }
        public int MeanTimeToRestore { get; set; }
        public int ChangeFailPercentage { get; set; }

        public OverviewData(IReadOnlyCollection<WorkItemCard> workItemCardList, IEnumerable<Release> releaseList)
        {
            AverageLeadTime = workItemCardList.Sum(item => item.LeadTimeHours) / workItemCardList.Count();
            LongestLeadTime = workItemCardList.Select(item => item.LeadTimeHours).Concat(new[] {0.0}).Max();
            ShortestLeadTime = workItemCardList.Select(item => item.LeadTimeHours).Concat(new[] {double.MaxValue}).Min();
            TotalDeploys = releaseList.Count();
            SuccessfulDeploys = 0;
            RolledBackDeploys = 0;
            DeployFrequency = 0;
            MeanTimeToRestore = 0;
            ChangeFailPercentage = 0;
        }
    }
}

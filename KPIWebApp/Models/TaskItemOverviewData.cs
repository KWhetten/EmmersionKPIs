using System.Security.Cryptography.X509Certificates;

namespace KPIWebApp.Models
{
    public class TaskItemOverviewData
    {
        public int TotalCards { get; set; }
        public decimal AverageLeadTime { get; set; }
        public decimal LongestLeadTime { get; set; } = decimal.MinValue;
        public decimal ShortestLeadTime { get; set; } = decimal.MaxValue;
    }

    public class ReleaseOverviewData
    {
        public int TotalDeploys { get; set; }
        public int SuccessfulDeploys { get; set; }
        public int RolledBackDeploys { get; set; }
        public decimal DeployFrequency { get; set; }
        public decimal MeanTimeToRestore { get; set; }
        public decimal ChangeFailPercentage { get; set; }

        public ReleaseOverviewData()
        {
            TotalDeploys = 0;
            SuccessfulDeploys = 0;
            RolledBackDeploys = 0;
            DeployFrequency = 0;
            MeanTimeToRestore = 0;
            ChangeFailPercentage = 0;
        }
    }
}

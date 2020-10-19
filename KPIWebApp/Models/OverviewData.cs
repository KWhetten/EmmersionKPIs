using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Objects;

namespace KPIWebApp.Models
{
    public class OverviewData
    {
        public int TotalCards { get; set; }
        public decimal AverageLeadTime { get; set; }
        public decimal LongestLeadTime { get; set; } = decimal.MinValue;
        public decimal ShortestLeadTime { get; set; } = decimal.MaxValue;

        public int TotalDeploys { get; set; }
        public int SuccessfulDeploys { get; set; }
        public int RolledBackDeploys { get; set; }
        public decimal DeployFrequency { get; set; }
        public decimal MeanTimeToRestore { get; set; }
        public decimal ChangeFailPercentage { get; set; }
    }
}

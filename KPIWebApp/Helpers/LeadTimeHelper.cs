using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Objects;
using KPIWebApp.Models;

namespace KPIWebApp.Helpers
{
    public class LeadTimeHelper
    {
        public OverviewData PopulateOverviewData(OverviewData overviewData, List<TaskItem> taskItemList)
        {
            var earliestTaskItemFinishTime = DateTime.MaxValue;
            foreach (var item in taskItemList)
            {
                if (item.StartTime != DateTime.MinValue && item.FinishTime != DateTime.MinValue)
                {
                    item.LeadTimeHours = CalculateLeadTimeHours(item);
                }

                if (item.LeadTimeHours > overviewData.LongestLeadTime
                    && item.StartTime != DateTime.MinValue &&
                    item.FinishTime != DateTime.MaxValue)
                {
                    overviewData.LongestLeadTime = item.LeadTimeHours;
                }

                if (item.LeadTimeHours < overviewData.ShortestLeadTime
                    && item.StartTime != DateTime.MinValue
                    && item.FinishTime != DateTime.MaxValue
                    && item.LeadTimeHours > 0)
                {
                    overviewData.ShortestLeadTime = item.LeadTimeHours;
                }

                if (item.FinishTime < earliestTaskItemFinishTime && item.FinishTime != DateTime.MinValue)
                {
                    earliestTaskItemFinishTime = item.FinishTime;
                }
            }

            var averageLeadTimeTaskItems = taskItemList.Where(taskItem =>
                taskItem.StartTime != DateTime.MinValue && taskItem.FinishTime != DateTime.MinValue).ToList();

            overviewData.AverageLeadTime = (averageLeadTimeTaskItems.Sum(item => item.LeadTimeHours) /
                                            averageLeadTimeTaskItems.Count);

            overviewData.TotalCards = taskItemList.Count;
            overviewData.AverageLeadTime =
                decimal.Round(overviewData.AverageLeadTime, 2, MidpointRounding.AwayFromZero);
            overviewData.LongestLeadTime =
                decimal.Round(overviewData.LongestLeadTime, 2, MidpointRounding.AwayFromZero);
            overviewData.ShortestLeadTime =
                decimal.Round(overviewData.ShortestLeadTime, 2, MidpointRounding.AwayFromZero);

            return overviewData;
        }

        private static decimal CalculateLeadTimeHours(TaskItem item)
        {
            const int hoursInAWorkDay = 8;
            var startOfDay = new TimeSpan(14, 30, 0);
            var endOfDay = new TimeSpan(22, 30, 0);

            var days = (decimal) (item.FinishTime - item.StartTime).TotalDays;
            var totalHours = 0m;
            if (days > 1)
            {
                for (var i = 1; i < Math.Floor(days); ++i)
                {
                    if (item.FinishTime.AddDays(-i).DayOfWeek != DayOfWeek.Saturday
                        && item.FinishTime.AddDays(-i).DayOfWeek != DayOfWeek.Sunday)
                    {
                        totalHours += hoursInAWorkDay;
                    }
                }
            }

            totalHours += (decimal) (endOfDay - item.StartTime.TimeOfDay).TotalHours;
            totalHours += (decimal) (item.FinishTime.TimeOfDay - startOfDay).TotalHours;

            return totalHours;
        }
    }
}

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
            DateTimeOffset? earliestTaskItemFinishTime = null;
            foreach (var item in taskItemList)
            {
                if (item.StartTime != null && item.FinishTime != null)
                {
                    item.LeadTimeHours = CalculateLeadTimeHours(item);
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

                if (item.FinishTime < earliestTaskItemFinishTime || item.FinishTime == null)
                {
                    earliestTaskItemFinishTime = item?.FinishTime;
                }
            }

            var averageLeadTimeTaskItems = taskItemList.Where(taskItem =>
                taskItem.StartTime != null && taskItem.FinishTime != null).ToList();

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

            var totalDays = (item.FinishTime - item.StartTime)?.TotalDays;
            if (totalDays != null)
            {
                var days = (decimal) totalDays;
                var totalHours = 0m;
                if (days > 1)
                {
                    for (var i = 1; i < Math.Floor(days); ++i)
                    {
                        if (item.FinishTime?.AddDays(-i).DayOfWeek != DayOfWeek.Saturday
                            && item.FinishTime?.AddDays(-i).DayOfWeek != DayOfWeek.Sunday)
                        {
                            totalHours += hoursInAWorkDay;
                        }
                    }
                }

                var hours = (endOfDay - item.StartTime?.TimeOfDay)?.TotalHours;
                if (hours != null)
                {
                    totalHours += (decimal) hours;
                }

                hours = (item.FinishTime?.TimeOfDay - startOfDay)?.TotalHours;
                if (hours != null)
                    totalHours += (decimal) totalHours;


                return totalHours;
            }

            return 0m;
        }
    }
}

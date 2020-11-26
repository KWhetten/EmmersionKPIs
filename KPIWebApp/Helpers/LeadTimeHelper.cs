using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Objects;
using KPIWebApp.Models;

namespace KPIWebApp.Helpers
{
    public interface ILeadTimeHelper
    {
        public decimal CalculateLeadTimeHours(TaskItem item);
    }

    public class LeadTimeHelper : ILeadTimeHelper
    {
        public decimal CalculateLeadTimeHours(TaskItem item)
        {
            const int hoursInAWorkDay = 8;
            var startOfDay = new TimeSpan(14, 30, 0);
            var endOfDay = new TimeSpan(22, 30, 0);

            var totalDays = (item.FinishTime - item.StartTime)?.TotalDays;

            if (totalDays == null) return 0m;

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
                totalHours += (decimal) hours;


            return totalHours;

        }
    }
}

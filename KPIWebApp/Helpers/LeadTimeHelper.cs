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
        const int hoursInAWorkDay = 8;
        public decimal CalculateLeadTimeHours(TaskItem item)
        {
            var startOfDay = new TimeSpan(14, 30, 0);
            var endOfDay = new TimeSpan(22, 30, 0);

            var totalDays = (item.FinishTime - item.StartTime)?.TotalDays;

            if (totalDays == null) return 0m;

            var days = (decimal) totalDays;
            var totalHours = GetHoursFromFullDays(item, days);

            totalHours += GetHoursFromPartialDays(item, endOfDay, startOfDay);


            return totalHours;

        }

        private static decimal GetHoursFromFullDays(TaskItem item, decimal days)
        {
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

            return totalHours;
        }

        private static decimal GetHoursFromPartialDays(TaskItem item, TimeSpan endOfDay, TimeSpan startOfDay)
        {
            var totalPartialDayHours = 0m;

            var firstDayEveningHours = (endOfDay - item.StartTime?.TimeOfDay)?.TotalHours;
            if (firstDayEveningHours != null)
            {
                totalPartialDayHours += (decimal) firstDayEveningHours;
            }

            var lastDayMorningHours = (item.FinishTime?.TimeOfDay - startOfDay)?.TotalHours;
            if (lastDayMorningHours != null)
                totalPartialDayHours += (decimal) lastDayMorningHours;

            return totalPartialDayHours;
        }
    }
}

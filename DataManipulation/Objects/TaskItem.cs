using System;
using System.Collections.Generic;
using DataAccess.DataRepositories;

namespace DataAccess.Objects
{
    public class TaskItem : IComparable<TaskItem>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? FinishTime { get; set; }
        public TaskItemType Type { get; set; }
        public string DevelopmentTeamName { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset? LastChangedOn { get; set; }
        public string LastChangedBy { get; set; }
        public string CurrentBoardColumn { get; set; }
        public string State { get; set; }
        public string Impact { get; set; }
        public int CommentCount { get; set; }
        public int NumRevisions { get; set; }
        public Release Release { get; set; }
        public List<HistoryEvent> HistoryEvents { get; set; }
        public decimal LeadTimeHours { get; set; }

        public int CompareTo(TaskItem other)
        {
            return LeadTimeHours.CompareTo(other.LeadTimeHours);
        }

        public decimal CalculateLeadTimeHours()
        {
            const int hoursInAWorkDay = 8;
            var startOfDay = new TimeSpan();
            if (StartTime != null)
            {
                startOfDay = new TimeSpan(14, 30, 0).Add(StartTime.Value.Offset);
            }

            var endOfDay = new TimeSpan();
            if (StartTime != null)
            {
                endOfDay = new TimeSpan(22, 30, 0).Add(StartTime.Value.Offset);
            }

            var totalDays = (FinishTime - StartTime)?.TotalDays;
            if (totalDays != null)
            {
                var days = (decimal) totalDays;
                LeadTimeHours = 0m;

                for (var i = 1; i < Math.Floor(days); ++i)
                {
                    if (FinishTime?.AddDays(-i).DayOfWeek != DayOfWeek.Saturday
                        && FinishTime?.AddDays(-i).DayOfWeek != DayOfWeek.Sunday)
                    {
                        LeadTimeHours += hoursInAWorkDay;
                    }
                }
            }

            var hours = (endOfDay - StartTime?.TimeOfDay)?.TotalHours;
            if (hours != null)
            {
                LeadTimeHours += (decimal) hours;
            }

            var totalHours = (FinishTime?.TimeOfDay - startOfDay)?.TotalHours;
            if (totalHours != null)
            {
                LeadTimeHours += (decimal) totalHours;
            }

            return LeadTimeHours;
        }
    }

    public class HistoryEvent
    {
        public int Id { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public string EventType { get; set; }
        public string TaskItemColumn { get; set; }
        public string TaskItemState { get; set; }
        public string Author { get; set; }
        public int TaskId { get; set; }
    }
}

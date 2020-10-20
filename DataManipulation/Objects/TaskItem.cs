using System;

namespace DataAccess.Objects
{
    public class TaskItem : IComparable<TaskItem>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public TaskItemType Type { get; set; }
        public string DevelopmentTeamName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastChangedOn { get; set; }
        public string LastChangedBy { get; set; }
        public string CurrentBoardColumn { get; set; }
        public string CardState { get; set; }
        public string Impact { get; set; }
        public int CommentCount { get; set; }
        public int NumRevisions { get; set; }
        public Release Release { get; set; }
        public decimal LeadTimeHours { get; set; }

        public int CompareTo(TaskItem other)
        {
            return LeadTimeHours.CompareTo(other.LeadTimeHours);
        }

        public decimal CalculateLeadTimeHours()
        {
            const int hoursInAWorkDay = 8;
            var startOfDay = new TimeSpan(14, 30, 0);
            var endOfDay = new TimeSpan(22, 30, 0);

            var days = (decimal) (FinishTime - StartTime).TotalDays;
            LeadTimeHours = 0m;

            for (var i = 1; i < Math.Floor(days); ++i)
            {
                if (FinishTime.AddDays(-i).DayOfWeek != DayOfWeek.Saturday
                    && FinishTime.AddDays(-i).DayOfWeek != DayOfWeek.Sunday)
                {
                    LeadTimeHours += hoursInAWorkDay;
                }
            }

            LeadTimeHours += (decimal) (endOfDay - StartTime.TimeOfDay).TotalHours;
            LeadTimeHours += (decimal) (FinishTime.TimeOfDay - startOfDay).TotalHours;

            return LeadTimeHours;
        }
    }
}

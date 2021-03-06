﻿using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Work.WebApi;

namespace DataAccess.Objects
{
    [Serializable]
    public class TaskItem : IEquatable<TaskItem>, IComparable<TaskItem>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? FinishTime { get; set; }
        public TaskItemType Type { get; set; }
        public DevelopmentTeam DevelopmentTeam { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }
        public Developer CreatedBy { get; set; }
        public DateTimeOffset? LastChangedOn { get; set; }
        public Developer LastChangedBy { get; set; }
        public BoardColumn CurrentBoardColumn { get; set; }
        public TaskItemState State { get; set; }
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

        public bool Equals(TaskItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Title == other.Title && Nullable.Equals(StartTime, other.StartTime) && Nullable.Equals(FinishTime, other.FinishTime) && Type == other.Type && DevelopmentTeam == other.DevelopmentTeam && Nullable.Equals(CreatedOn, other.CreatedOn) && CreatedBy == other.CreatedBy && Nullable.Equals(LastChangedOn, other.LastChangedOn) && LastChangedBy == other.LastChangedBy && CurrentBoardColumn == other.CurrentBoardColumn && State == other.State && NumRevisions == other.NumRevisions && Equals(Release, other.Release) && Equals(HistoryEvents, other.HistoryEvents) && LeadTimeHours == other.LeadTimeHours;
        }
    }
}

using System;

namespace DataObjects.Objects
{
    public class WorkItemCard
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public WorkItemCardType Type { get; set; }
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

        public decimal LeadTimeHours
        {
            get
            {
                if (StartTime != DateTime.MinValue && FinishTime != DateTime.MinValue)
                {
                    return (decimal)(FinishTime - StartTime).TotalHours;
                }
                return 0m;
            }
        }
    }
}

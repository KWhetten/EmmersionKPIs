using System;
using System.Collections.Generic;

namespace DataAccess.Objects
{
    public class RegressionAnalysisTaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public TimeSpan Lifetime { get; set; } // CreatedOn -> LastChangedOn
        public TimeSpan LeadTime { get; set; } // StartTime -> FinishTime
        public TimeSpan TimeSpentInBacklog { get; set; } // CreatedOn -> StartTime

        public TaskItemType TaskItemType { get; set; }

        public bool TypeIsProduct { get; set; }
        public bool TypeIsEngineering { get; set; }
        public bool TypeIsUnanticipated { get; set; }

        public bool DevTeamIsAssessments { get; set; }
        public bool DevTeamIsEnterprise { get; set; }

        public int CreatedById { get; set; }
        public Developer LastChangedBy { get; set; }

        public int NumRevisions { get; set; }
    }
}

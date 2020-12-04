using System.Collections.Generic;

namespace DataAccess.Objects
{
    public class MultipleLinearRegressionTaskItem
    {
        public double TimeSpentInBacklog { get; set; }

        public bool TypeIsProduct { get; set; }
        public bool TypeIsEngineering { get; set; }
        public bool TypeIsUnanticipated { get; set; }

        public bool DevTeamIsAssessments { get; set; }
        public bool DevTeamIsEnterprise { get; set; }

        public string CreatedBy { get; set; }
    }
}

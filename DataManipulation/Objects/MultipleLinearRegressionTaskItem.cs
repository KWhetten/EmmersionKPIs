using System.Collections.Generic;

namespace DataAccess.Objects
{
    public class MultipleLinearRegressionTaskItem
    {
        public int Id { get; set; }
        public double TimeSpentInBacklog { get; set; }

        public bool TypeIsProduct { get; set; }
        public bool TypeIsEngineering { get; set; }
        public bool TypeIsUnanticipated { get; set; }

        public bool DevTeamIsAssessments { get; set; }
        public bool DevTeamIsEnterprise { get; set; }

        public Developer CreatedBy { get; set; }
    }
}

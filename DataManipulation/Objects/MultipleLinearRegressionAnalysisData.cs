using System.Collections.Generic;

namespace DataAccess.Objects
{
    public class MultipleLinearRegressionAnalysisData
    {
        public List<int> Ids { get; set; }
        public double[][] Inputs { get; set; }
        public double[] Outputs { get; set; }
        public double[] Predicted { get; set; }
        public double Error { get; set; }
        public double R2 { get; set; }
        public List<int> UserIds { get; set; }

        public MultipleLinearRegressionAnalysisData()
        {
            Ids = new List<int>();
            UserIds = new List<int>();
        }
    }
}

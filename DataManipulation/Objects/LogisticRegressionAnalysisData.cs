using System.Collections.Generic;
using DataAccess.Objects;

public class MultinomialLogisticRegressionAnalysisItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Actual { get; set; }
    public int Prediction { get; set; }
    public double Probability { get; set; }
    public List<double> Inputs { get; set; }

    public MultinomialLogisticRegressionAnalysisItem()
    {
        Inputs = new List<double>();
    }
}

public class MultinomialLogisticRegressionAnalysisItemList
{
    public List<MultinomialLogisticRegressionAnalysisItem> Items { get; set; }
    public List<int> UserIds { get; set; }
    public double Error { get; set; }

    public MultinomialLogisticRegressionAnalysisItemList()
    {
        Items = new List<MultinomialLogisticRegressionAnalysisItem>();
        UserIds = new List<int>();
    }
}

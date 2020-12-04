using System.Collections.Generic;

public class MultinomialLogisticRegressionAnalysisItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Output { get; set; }
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
    public List<string> Users { get; set; }
    public double Error { get; set; }

    public MultinomialLogisticRegressionAnalysisItemList()
    {
        Items = new List<MultinomialLogisticRegressionAnalysisItem>();
        Users = new List<string>();
    }
}

using System;
using System.Threading.Tasks;
using KPIWebApp.Helpers;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class MultinomialLogisticRegressionAnalysisTests
    {
        [Test]
        public async Task When_getting_logistic_regression_analysis()
        {
            var helper = new MultinomialLogisticRegressionAnalysisHelper();
            var result = await helper.GetLogisticRegressionAnalysisData((new DateTimeOffset(new DateTime(2020, 1, 1))),
                new DateTimeOffset(DateTime.Today), true, true, true, true, true);

            Console.Write(
                "Id,Lifetime,LeadTime,TimeSpentInBacklog,DevTeamIsAssessments,DevTeamInEnterprise,NumRevisions,");
            foreach (var user in result.UserIds)
            {
                Console.Write($"CreatedBy({user}),");
            }
            foreach (var user in result.UserIds)
            {
                Console.Write($"LastChangedBy({user}),");
            }
            Console.Write("Probability,Output,Answer\n");


            foreach (var item in result.Items)
            {
                Console.Write($"{item.Id},");
                foreach (var input in item.Inputs)
                {
                    Console.Write($"{input},");
                }

                Console.Write($"{item.Probability},{item.Actual},{item.Prediction}\n");
            }

            Console.WriteLine($"Error: {result.Error}");
        }
    }
}

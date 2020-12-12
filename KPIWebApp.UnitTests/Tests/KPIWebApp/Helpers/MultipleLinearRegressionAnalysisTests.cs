using System;
using System.Threading.Tasks;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class MultipleLinearRegressionAnalysisTests
    {
        [Test]
        public async Task When_getting_multiple_linear_regression_analysis_data()
        {
            var multipleLinearRegressionAnalysis = new MultipleLinearRegressionAnalysisHelper();

            var item = new MultipleLinearRegressionTaskItem
            {
                TimeSpentInBacklog = .15,

                TypeIsProduct = false,
                TypeIsEngineering = true,
                TypeIsUnanticipated = false,

                DevTeamIsAssessments = true,
                DevTeamIsEnterprise = false,

                CreatedBy = "neil.sorensen"
            };

            var result = await multipleLinearRegressionAnalysis.GetEstimation(item);

            Console.WriteLine($"Prediction: {result}");

            Assert.That(result, Is.EqualTo("12.44"));
        }
    }
}

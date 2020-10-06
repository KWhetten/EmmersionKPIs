using System;
using KPIWebApp.Controllers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.WebApp
{
    [TestFixture]
    public class OverviewControllerTests
    {
        [Test]
        public void When_getting_all_information()
        {
            var overviewController =
                new OverviewController();

            var response = overviewController.Get(DateTime.Now.AddDays(-7).ToString(), DateTime.Now.ToString());

            Assert.That(response.AverageLeadTime, Is.GreaterThan(0m));
            Assert.That(response.LongestLeadTime, Is.GreaterThan(0m));
            Assert.That(response.ShortestLeadTime, Is.GreaterThan(0m));

            Assert.That(response.TotalDeploys, Is.GreaterThan(0));
            // Assert.That(response.SuccessfulDeploys, Is.GreaterThan(0));
            // Assert.That(response.RolledBackDeploys, Is.GreaterThanOrEqualTo(0));
            Assert.That(response.DeployFrequency, Is.GreaterThan(0m));
            // Assert.That(response.MeanTimeToRestore, Is.GreaterThan(0m));
            // Assert.That(response.ChangeFailPercentage, Is.GreaterThan(0m));
        }

        [Test]
        public void When_getting_info_without_dates()
        {
            var overviewController = new OverviewController();

            var response = overviewController.Get("", "");


            Assert.That(response.AverageLeadTime, Is.GreaterThan(0m));
            Assert.That(response.LongestLeadTime, Is.GreaterThan(0m));
            Assert.That(response.ShortestLeadTime, Is.GreaterThan(0m));

            Assert.That(response.TotalDeploys, Is.GreaterThan(0));
            // Assert.That(response.SuccessfulDeploys, Is.GreaterThan(0));
            // Assert.That(response.RolledBackDeploys, Is.GreaterThanOrEqualTo(0));
            Assert.That(response.DeployFrequency, Is.GreaterThan(0m));
            // Assert.That(response.MeanTimeToRestore, Is.GreaterThan(0m));
            // Assert.That(response.ChangeFailPercentage, Is.GreaterThan(0m));
        }

        [Test]
        public void When_getting_info_with_minimum_dates()
        {
            var overviewController = new OverviewController();

            var response = overviewController.Get(DateTime.MinValue.ToString(), DateTime.MinValue.ToString());


            Assert.That(response.AverageLeadTime, Is.GreaterThan(0m));
            Assert.That(response.LongestLeadTime, Is.GreaterThan(0m));
            Assert.That(response.ShortestLeadTime, Is.GreaterThan(0m));

            Assert.That(response.TotalDeploys, Is.GreaterThan(0));
            // Assert.That(response.SuccessfulDeploys, Is.GreaterThan(0));
            // Assert.That(response.RolledBackDeploys, Is.GreaterThanOrEqualTo(0));
            Assert.That(response.DeployFrequency, Is.GreaterThan(0m));
            // Assert.That(response.MeanTimeToRestore, Is.GreaterThan(0m));
            // Assert.That(response.ChangeFailPercentage, Is.GreaterThan(0m));
        }
    }
}

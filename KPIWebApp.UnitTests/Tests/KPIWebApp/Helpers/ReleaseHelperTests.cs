using System;
using System.Collections.Generic;
using Accord;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using KPIWebApp.Models;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class ReleaseHelperTests
    {
        [Test]
        public void When_populating_overview_data()
        {
            var releaseHelper = new ReleaseHelper();

            var result = releaseHelper.PopulateOverviewData(new List<Release>
                {
                    new Release
                    {

                        Id = 1,
                        Name = "1.0.001.1",
                        StartTime = new DateTimeOffset(new DateTime(2021, 1, 10)),
                        FinishTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                        State = "failed",
                        Attempts = 2,
                        ReleaseEnvironment = new ReleaseEnvironment
                        {
                            Id = 1
                        }
                    },
                    new Release
                    {

                        Id = 1,
                        Name = "1.0.001.2",
                        StartTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                        FinishTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                        State = "failed",
                        Attempts = 1,
                        ReleaseEnvironment = new ReleaseEnvironment
                        {
                            Id = 1
                        }
                    },
                    new Release
                    {

                        Id = 1,
                        Name = "1.0.001.1",
                        StartTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                        FinishTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                        State = "failed",
                        Attempts = 2,
                        ReleaseEnvironment = new ReleaseEnvironment
                        {
                            Id = 1
                        }
                    },
                    new Release
                    {

                        Id = 13,
                        Name = "1.0.001.3",
                        StartTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                        FinishTime = new DateTimeOffset(new DateTime(2021, 1, 16)),
                        State = "succeeded",
                        Attempts = 2,
                        ReleaseEnvironment = new ReleaseEnvironment
                        {
                            Id = 1
                        }
                    },
                    new Release
                    {

                        Id = 1,
                        Name = "1.0.001.4",
                        StartTime = new DateTimeOffset(new DateTime(2021, 1, 16)),
                        FinishTime = new DateTimeOffset(new DateTime(2021, 1, 17)),
                        State = "succeeded",
                        Attempts = 2,
                        ReleaseEnvironment = new ReleaseEnvironment
                        {
                            Id = 1
                        }
                    }
                },
                new DateTimeOffset(new DateTime(2021, 1, 15)), true, true);

            Assert.That(result.ChangeFailPercentage, Is.EqualTo(20));
            Assert.That(result.DeployFrequency, Is.EqualTo(8.75));
            Assert.That(result.MeanTimeToRestore, Is.EqualTo(1440));
            Assert.That(result.RolledBackDeploys, Is.EqualTo(1));
            Assert.That(result.SuccessfulDeploys, Is.EqualTo(4));
            Assert.That(result.TotalDeploys, Is.EqualTo(5));
        }
    }
}

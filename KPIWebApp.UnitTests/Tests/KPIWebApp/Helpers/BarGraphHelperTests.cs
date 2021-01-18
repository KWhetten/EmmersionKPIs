using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class BarGraphHelperTests
    {
        [Test]
        public async Task When_getting_release_bar_graph_data()
        {
            var releaseList = new List<Release>
            {
                new Release
                {
                    Name = "1.0.001.1",
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Assessments"
                    }
                },
                new Release
                {
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 2,
                        Name = "TrueNorthTest Release"
                    }
                },
                new Release
                {
                    Name = "1.0.002.1",
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                    Attempts = 1,
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Assessments"
                    }
                },
                new Release
                {
                    Name = "1.0.001.1",
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Attempts = 3,
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Assessments"
                    }
                }
            };

            var mockReleaseRepository = new Mock<ReleaseRepository>();
            mockReleaseRepository
                .Setup(x => x.GetReleaseListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(releaseList);

            var mockReleaseHelper = new Mock<ReleaseHelper>();
            mockReleaseHelper.Setup(x => x.GetRolledBackReleases(It.IsAny<List<Release>>())).Returns(new List<Release>{releaseList[2]});
            mockReleaseHelper.Setup(x => x.ReleaseVersionIsLater(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var barGraphHelper = new BarGraphHelper(mockReleaseRepository.Object, mockReleaseHelper.Object);

            var result = await barGraphHelper.GetReleaseBarGraphData(new DateTimeOffset(new DateTime(2021, 1, 12)),
                new DateTimeOffset(new DateTime(2021, 1, 15)), true, true);

            Assert.That(result.Dates[0], Is.EqualTo("January 12"));
            Assert.That(result.Dates[1], Is.EqualTo("January 13"));
            Assert.That(result.Dates[2], Is.EqualTo("January 14"));
            Assert.That(result.Dates[3], Is.EqualTo("January 15"));

            Assert.That(result.Rows[0].Name, Is.EqualTo("Releases"));
            Assert.That(result.Rows[0].Data, Is.EqualTo(new List<int> {2, 0, 1, 0}));

            Assert.That(result.Rows[1].Name, Is.EqualTo("Rolled Back Releases"));
            Assert.That(result.Rows[1].Data, Is.EqualTo(new List<int> {0, 1, 0, 0}));
        }

        [Test]
        public async Task When_getting_release_bar_graph_data_for_assessments_team()
        {
            var releaseList = new List<Release>
            {
                new Release
                {
                    Name = "1.0.001.1",
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Assessments"
                    }
                },
                new Release
                {
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 2,
                        Name = "TrueNorthTest Release"
                    }
                },
                new Release
                {
                    Name = "1.0.002.1",
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                    Attempts = 1,
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Assessments"
                    }
                },
                new Release
                {
                    Name = "1.0.001.1",
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Attempts = 3,
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Assessments"
                    }
                }
            };

            var mockReleaseRepository = new Mock<ReleaseRepository>();
            mockReleaseRepository
                .Setup(x => x.GetReleaseListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(releaseList);

            var mockReleaseHelper = new Mock<ReleaseHelper>();
            mockReleaseHelper.Setup(x => x.GetRolledBackReleases(It.IsAny<List<Release>>())).Returns(new List<Release>{releaseList[2]});
            mockReleaseHelper.Setup(x => x.ReleaseVersionIsLater(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var barGraphHelper = new BarGraphHelper(mockReleaseRepository.Object, mockReleaseHelper.Object);

            var result = await barGraphHelper.GetReleaseBarGraphData(new DateTimeOffset(new DateTime(2021, 1, 12)),
                new DateTimeOffset(new DateTime(2021, 1, 15)), true, false);

            Assert.That(result.Dates[0], Is.EqualTo("January 12"));
            Assert.That(result.Dates[1], Is.EqualTo("January 13"));
            Assert.That(result.Dates[2], Is.EqualTo("January 14"));
            Assert.That(result.Dates[3], Is.EqualTo("January 15"));

            Assert.That(result.Rows[0].Name, Is.EqualTo("Releases"));
            Assert.That(result.Rows[0].Data, Is.EqualTo(new List<int> {1, 0, 1, 0}));

            Assert.That(result.Rows[1].Name, Is.EqualTo("Rolled Back Releases"));
            Assert.That(result.Rows[1].Data, Is.EqualTo(new List<int> {0, 1, 0, 0}));
        }

        [Test]
        public async Task When_getting_release_bar_graph_data_for_enterprise_team()
        {
            var releaseList = new List<Release>
            {
                new Release
                {
                    Name = "1.0.001.1",
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Assessments"
                    }
                },
                new Release
                {
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 2,
                        Name = "TrueNorthTest Release"
                    }
                },
                new Release
                {
                    Name = "1.0.002.1",
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 13)),
                    Attempts = 1,
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Assessments"
                    }
                },
                new Release
                {
                    Name = "1.0.001.1",
                    FinishTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Attempts = 3,
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Assessments"
                    }
                }
            };

            var mockReleaseRepository = new Mock<ReleaseRepository>();
            mockReleaseRepository
                .Setup(x => x.GetReleaseListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(releaseList);

            var mockReleaseHelper = new Mock<ReleaseHelper>();
            mockReleaseHelper.Setup(x => x.GetRolledBackReleases(It.IsAny<List<Release>>())).Returns(new List<Release>());
            mockReleaseHelper.Setup(x => x.ReleaseVersionIsLater(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var barGraphHelper = new BarGraphHelper(mockReleaseRepository.Object, mockReleaseHelper.Object);

            var result = await barGraphHelper.GetReleaseBarGraphData(new DateTimeOffset(new DateTime(2021, 1, 12)),
                new DateTimeOffset(new DateTime(2021, 1, 15)), false, true);

            Assert.That(result.Dates[0], Is.EqualTo("January 12"));
            Assert.That(result.Dates[1], Is.EqualTo("January 13"));
            Assert.That(result.Dates[2], Is.EqualTo("January 14"));
            Assert.That(result.Dates[3], Is.EqualTo("January 15"));

            Assert.That(result.Rows[0].Name, Is.EqualTo("Releases"));
            Assert.That(result.Rows[0].Data, Is.EqualTo(new List<int> {1, 0, 0, 0}));

            Assert.That(result.Rows[1].Name, Is.EqualTo("Rolled Back Releases"));
            Assert.That(result.Rows[1].Data, Is.EqualTo(new List<int> {0, 0, 0, 0}));
        }
    }
}

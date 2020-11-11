using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using KPIWebApp.Models;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp
{
    [TestFixture]
    public class OverviewHelperTests
    {
        [Test]
        public async Task When_getting_task_item_data()
        {
            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            var taskItemList = new List<TaskItem>();
            mockTaskItemRepository.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(taskItemList);

            var overviewHelper = new OverviewHelper(mockTaskItemRepository.Object);
            var result = await overviewHelper.GetTaskItemData(new DateTimeOffset(new DateTime(2020, 10, 28), TimeSpan.Zero), new DateTimeOffset(new DateTime(2020, 10, 28), TimeSpan.Zero));

            Assert.That(result, Is.EqualTo(taskItemList));
        }

        [Test]
        public async Task When_getting_release_data()
        {
            var mockReleaseRepository = new Mock<IReleaseRepository>();
            var releaseList = new List<Release>();
            mockReleaseRepository.Setup(x => x.GetReleaseListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(releaseList);

            var overviewHelper = new OverviewHelper(mockReleaseRepository.Object);
            var result = await overviewHelper.GetReleaseData(new DateTimeOffset(new DateTime(2020, 10, 28), TimeSpan.Zero), new DateTimeOffset(new DateTime(2020, 10, 28), TimeSpan.Zero));

            Assert.That(result, Is.EqualTo(releaseList));
        }

        [Test]
        public async Task When_getting_overview_data()
        {
            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            var mockReleaseRepository = new Mock<IReleaseRepository>();

            var releaseList = new List<Release>
            {
                new Release
                {
                    Id = 1,
                    State = "Status1",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Name1"
                    },
                    StartTime = new DateTimeOffset(new DateTime(2020, 10, 28).AddDays(-5)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 10, 28).AddDays(-4)),
                    Name = "Name1",
                    Attempts = 1
                },
                new Release
                {
                    Id = 2,
                    State = "Status2",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 2,
                        Name = "Name2"
                    },
                    StartTime = new DateTimeOffset(new DateTime(2020, 10, 28).AddDays(-7)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 10, 28)),
                    Name = "Name2",
                    Attempts = 2
                }
            };
            var taskItemList = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Title1",
                    StartTime = new DateTimeOffset(new DateTime(2020, 10, 28).AddDays(-7)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 10, 28).AddDays(-3)),
                    Type = TaskItemType.Engineering,
                    DevelopmentTeamName = "Team1",
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 28).AddDays(-8)),
                    CreatedBy = "CreatedBy1",
                    LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 28)),
                    LastChangedBy = "ChangedBy1",
                    CurrentBoardColumn = "BoardColumn1",
                    State = "State1",
                    Impact = "Impact 1",
                    CommentCount = 1,
                    NumRevisions = 1,
                    Release = releaseList.First()
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Title2",
                    StartTime = new DateTimeOffset(new DateTime(2020, 10, 28).AddDays(-1)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 10, 28)),
                    Type = TaskItemType.Engineering,
                    DevelopmentTeamName = "Team2",
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 28).AddDays(-1)),
                    CreatedBy = "CreatedBy2",
                    LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 28)),
                    LastChangedBy = "ChangedBy2",
                    CurrentBoardColumn = "BoardColumn2",
                    State = "State2",
                    Impact = "Impact 2",
                    CommentCount = 2,
                    NumRevisions = 2,
                    Release = releaseList.Last()
                }
            };

            mockTaskItemRepository.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(taskItemList);
            mockReleaseRepository.Setup(x => x.GetReleaseListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(releaseList);

            var overviewHelper = new OverviewHelper(mockTaskItemRepository.Object, mockReleaseRepository.Object);
            var result = await overviewHelper.GetOverviewData(new DateTimeOffset(new DateTime(2020, 10, 28).AddDays(-10), TimeSpan.Zero), new DateTimeOffset(new DateTime(2020, 10, 28), TimeSpan.Zero));

            var expected = new OverviewData
            {
                TotalCards = 2,
                AverageLeadTime = 16m,
                LongestLeadTime = 24m,
                ShortestLeadTime = 8m,
                TotalDeploys = 2,
                SuccessfulDeploys = 0,
                RolledBackDeploys = 0,
                DeployFrequency = 4.67m,
                MeanTimeToRestore = 0,
                ChangeFailPercentage = 0,
            };

            Assert.That(result.TotalCards, Is.EqualTo(expected.TotalCards));
            Assert.That(result.AverageLeadTime, Is.EqualTo(expected.AverageLeadTime));
            Assert.That(result.LongestLeadTime, Is.EqualTo(expected.LongestLeadTime));
            Assert.That(result.ShortestLeadTime, Is.EqualTo(expected.ShortestLeadTime));
            Assert.That(result.TotalDeploys, Is.EqualTo(expected.TotalDeploys));
            Assert.That(result.SuccessfulDeploys, Is.EqualTo(expected.SuccessfulDeploys));
            Assert.That(result.RolledBackDeploys, Is.EqualTo(expected.RolledBackDeploys));
            Assert.That(result.DeployFrequency, Is.EqualTo(expected.DeployFrequency));
            Assert.That(result.MeanTimeToRestore, Is.EqualTo(expected.MeanTimeToRestore));
            Assert.That(result.ChangeFailPercentage, Is.EqualTo(expected.ChangeFailPercentage));
        }
    }
}

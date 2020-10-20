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
            mockTaskItemRepository.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(taskItemList);

            var overviewHelper = new OverviewHelper(mockTaskItemRepository.Object);
            var result = await overviewHelper.GetTaskItemData(DateTime.Today, DateTime.Today);

            Assert.That(result, Is.EqualTo(taskItemList));
        }

        [Test]
        public async Task When_getting_release_data()
        {
            var mockReleaseRepository = new Mock<IReleaseRepository>();
            var releaseList = new List<Release>();
            mockReleaseRepository.Setup(x => x.GetReleaseListAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(releaseList);

            var overviewHelper = new OverviewHelper(mockReleaseRepository.Object);
            var result = await overviewHelper.GetReleaseData(DateTime.Today, DateTime.Today);

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
                    Status = "Status1",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "Name1"
                    },
                    StartTime = DateTime.Today.AddDays(-5),
                    FinishTime = DateTime.Today.AddDays(-4),
                    Name = "Name1",
                    Attempts = 1
                },
                new Release
                {
                    Id = 2,
                    Status = "Status2",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 2,
                        Name = "Name2"
                    },
                    StartTime = DateTime.Today.AddDays(-7),
                    FinishTime = DateTime.Today,
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
                    StartTime = DateTime.Today.AddDays(-7),
                    FinishTime = DateTime.Today.AddDays(-3),
                    Type = TaskItemType.Engineering,
                    DevelopmentTeamName = "Team1",
                    CreatedOn = DateTime.Today.AddDays(-8),
                    CreatedBy = "CreatedBy1",
                    LastChangedOn = DateTime.Today,
                    LastChangedBy = "ChangedBy1",
                    CurrentBoardColumn = "BoardColumn1",
                    CardState = "State1",
                    Impact = "Impact 1",
                    CommentCount = 1,
                    NumRevisions = 1,
                    Release = releaseList.First()
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Title2",
                    StartTime = DateTime.Today.AddDays(-1),
                    FinishTime = DateTime.Today,
                    Type = TaskItemType.Engineering,
                    DevelopmentTeamName = "Team2",
                    CreatedOn = DateTime.Today.AddDays(-1),
                    CreatedBy = "CreatedBy2",
                    LastChangedOn = DateTime.Today,
                    LastChangedBy = "ChangedBy2",
                    CurrentBoardColumn = "BoardColumn2",
                    CardState = "State2",
                    Impact = "Impact 2",
                    CommentCount = 2,
                    NumRevisions = 2,
                    Release = releaseList.Last()
                }
            };

            mockTaskItemRepository.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(taskItemList);
            mockReleaseRepository.Setup(x => x.GetReleaseListAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(releaseList);

            var overviewHelper = new OverviewHelper(mockTaskItemRepository.Object, mockReleaseRepository.Object);
            var result = await overviewHelper.GetOverviewData(DateTime.Today.AddDays(-10), DateTime.Today);

            var expected = new OverviewData
            {
                TotalCards = 2,
                AverageLeadTime = 20m,
                LongestLeadTime = 32m,
                ShortestLeadTime = 8m,
                TotalDeploys = 2,
                SuccessfulDeploys = 0,
                RolledBackDeploys = 0,
                DeployFrequency = 3.5m,
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

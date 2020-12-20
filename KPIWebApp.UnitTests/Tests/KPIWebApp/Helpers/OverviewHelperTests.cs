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

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
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
        public async Task When_getting_overview_data() // TODO: Add in explicit dates
        {
            var overviewData = new TaskItemOverviewData();
            var taskItemList = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Task1",
                    StartTime = DateTimeOffset.Now.Date.AddDays(-1),
                    FinishTime = DateTimeOffset.Now.Date,
                    Type = TaskItemType.Product,
                    DevelopmentTeam = "Assessments Team",
                    CreatedOn = DateTimeOffset.Now.Date.AddDays(-1-1),
                    CreatedBy = "User1",
                    LastChangedOn = DateTimeOffset.Now.Date,
                    LastChangedBy = "User1",
                    CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                    State = TaskItemState.Released,
                    NumRevisions = 1,
                    Release = new Release(),
                    HistoryEvents = new List<HistoryEvent>(),
                    LeadTimeHours = 0
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Task2",
                    StartTime = DateTimeOffset.Now.Date.AddDays(-2),
                    FinishTime = DateTimeOffset.Now.Date,
                    Type = TaskItemType.Engineering,
                    DevelopmentTeam = "Assessments Team",
                    CreatedOn = DateTimeOffset.Now.Date.AddDays(-2-2),
                    CreatedBy = "User2",
                    LastChangedOn = DateTimeOffset.Now.Date,
                    LastChangedBy = "User2",
                    CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                    State = TaskItemState.Released,
                    NumRevisions = 2,
                    Release = new Release(),
                    HistoryEvents = new List<HistoryEvent>(),
                    LeadTimeHours = 0
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Task3",
                    StartTime = DateTimeOffset.Now.Date.AddDays(-3),
                    FinishTime = DateTimeOffset.Now.Date,
                    Type = TaskItemType.Unanticipated,
                    DevelopmentTeam = "Assessments Team",
                    CreatedOn = DateTimeOffset.Now.Date.AddDays(-3-3),
                    CreatedBy = "User3",
                    LastChangedOn = DateTimeOffset.Now.Date,
                    LastChangedBy = "User3",
                    CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                    State = TaskItemState.Released,
                    NumRevisions = 3,
                    Release = new Release(),
                    HistoryEvents = new List<HistoryEvent>(),
                    LeadTimeHours = 0
                },
                new TaskItem
                {
                    Id = 4,
                    Title = "Task4",
                    StartTime = DateTimeOffset.Now.Date.AddDays(-4),
                    FinishTime = DateTimeOffset.Now.Date,
                    Type = TaskItemType.Product,
                    DevelopmentTeam = "Assessments Team",
                    CreatedOn = DateTimeOffset.Now.Date.AddDays(-4-4),
                    CreatedBy = "User4",
                    LastChangedOn = DateTimeOffset.Now.Date,
                    LastChangedBy = "User4",
                    CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                    State = TaskItemState.Released,
                    NumRevisions = 4,
                    Release = new Release(),
                    HistoryEvents = new List<HistoryEvent>(),
                    LeadTimeHours = 0
                },
                new TaskItem
                {
                    Id = 5,
                    Title = "Task5",
                    StartTime = DateTimeOffset.Now.Date.AddDays(-5),
                    FinishTime = DateTimeOffset.Now.Date,
                    Type = TaskItemType.Engineering,
                    DevelopmentTeam = "Assessments Team",
                    CreatedOn = DateTimeOffset.Now.Date.AddDays(-5-5),
                    CreatedBy = "User5",
                    LastChangedOn = DateTimeOffset.Now.Date,
                    LastChangedBy = "User5",
                    CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                    State = TaskItemState.Released,
                    NumRevisions = 5,
                    Release = new Release(),
                    HistoryEvents = new List<HistoryEvent>(),
                    LeadTimeHours = 0
                }
            };


            var overviewHelper = new OverviewHelper();
            var result = overviewHelper.PopulateOverviewData(overviewData, taskItemList);

            var expected = new TaskItemOverviewData
            {
                AverageLeadTime = 19.2m,
                LongestLeadTime = 24m,
                ShortestLeadTime = 8m,
                TotalCards = 5
            };

            Assert.That(result.TotalCards, Is.EqualTo(expected.TotalCards));
            Assert.That(result.AverageLeadTime, Is.EqualTo(expected.AverageLeadTime));
            Assert.That(result.LongestLeadTime, Is.EqualTo(expected.LongestLeadTime));
            Assert.That(result.ShortestLeadTime, Is.EqualTo(expected.ShortestLeadTime));
        }

        [Test]
        public async Task When_getting_only_product_overview_data()
        {
            var overviewData = new TaskItemOverviewData();
            var taskItemList = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Task1",
                    StartTime = DateTimeOffset.Now.Date.AddDays(-1),
                    FinishTime = DateTimeOffset.Now.Date,
                    Type = TaskItemType.Product,
                    DevelopmentTeam = "Assessments Team",
                    CreatedOn = DateTimeOffset.Now.Date.AddDays(-1-1),
                    CreatedBy = "User1",
                    LastChangedOn = DateTimeOffset.Now.Date,
                    LastChangedBy = "User1",
                    CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                    State = TaskItemState.Released,
                    NumRevisions = 1,
                    Release = new Release(),
                    HistoryEvents = new List<HistoryEvent>(),
                    LeadTimeHours = 0
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Task2",
                    StartTime = DateTimeOffset.Now.Date.AddDays(-2),
                    FinishTime = DateTimeOffset.Now.Date,
                    Type = TaskItemType.Engineering,
                    DevelopmentTeam = "Assessments Team",
                    CreatedOn = DateTimeOffset.Now.Date.AddDays(-2-2),
                    CreatedBy = "User2",
                    LastChangedOn = DateTimeOffset.Now.Date,
                    LastChangedBy = "User2",
                    CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                    State = TaskItemState.Released,
                    NumRevisions = 2,
                    Release = new Release(),
                    HistoryEvents = new List<HistoryEvent>(),
                    LeadTimeHours = 0
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Task3",
                    StartTime = DateTimeOffset.Now.Date.AddDays(-3),
                    FinishTime = DateTimeOffset.Now.Date,
                    Type = TaskItemType.Unanticipated,
                    DevelopmentTeam = "Assessments Team",
                    CreatedOn = DateTimeOffset.Now.Date.AddDays(-3-3),
                    CreatedBy = "User3",
                    LastChangedOn = DateTimeOffset.Now.Date,
                    LastChangedBy = "User3",
                    CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                    State = TaskItemState.Released,
                    NumRevisions = 3,
                    Release = new Release(),
                    HistoryEvents = new List<HistoryEvent>(),
                    LeadTimeHours = 0
                },
                new TaskItem
                {
                    Id = 4,
                    Title = "Task4",
                    StartTime = DateTimeOffset.Now.Date.AddDays(-4),
                    FinishTime = DateTimeOffset.Now.Date,
                    Type = TaskItemType.Product,
                    DevelopmentTeam = "Assessments Team",
                    CreatedOn = DateTimeOffset.Now.Date.AddDays(-4-4),
                    CreatedBy = "User4",
                    LastChangedOn = DateTimeOffset.Now.Date,
                    LastChangedBy = "User4",
                    CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                    State = TaskItemState.Released,
                    NumRevisions = 4,
                    Release = new Release(),
                    HistoryEvents = new List<HistoryEvent>(),
                    LeadTimeHours = 0
                },
                new TaskItem
                {
                    Id = 5,
                    Title = "Task5",
                    StartTime = DateTimeOffset.Now.Date.AddDays(-5),
                    FinishTime = DateTimeOffset.Now.Date,
                    Type = TaskItemType.Engineering,
                    DevelopmentTeam = "Assessments Team",
                    CreatedOn = DateTimeOffset.Now.Date.AddDays(-5-5),
                    CreatedBy = "User5",
                    LastChangedOn = DateTimeOffset.Now.Date,
                    LastChangedBy = "User5",
                    CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                    State = TaskItemState.Released,
                    NumRevisions = 5,
                    Release = new Release(),
                    HistoryEvents = new List<HistoryEvent>(),
                    LeadTimeHours = 0
                }
            };


            var overviewHelper = new OverviewHelper(true, false, false);
            var result = overviewHelper.PopulateOverviewData(overviewData, taskItemList);

            var expected = new TaskItemOverviewData
            {
                AverageLeadTime = 6.4m,
                LongestLeadTime = 24m,
                ShortestLeadTime = 8m,
                TotalCards = 2
            };

            Assert.That(result.TotalCards, Is.EqualTo(expected.TotalCards));
            Assert.That(result.AverageLeadTime, Is.EqualTo(expected.AverageLeadTime));
            Assert.That(result.LongestLeadTime, Is.EqualTo(expected.LongestLeadTime));
            Assert.That(result.ShortestLeadTime, Is.EqualTo(expected.ShortestLeadTime));
        }
    }
}

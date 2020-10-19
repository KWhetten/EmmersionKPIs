using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp
{
    public class TaskItemHelperTests
    {
        [Test]
        public async Task When_getting_work_item_card_data()
        {
            var taskItem1 = new TaskItem
            {
                Id = 1,
                Title = "Title1",
                StartTime = DateTime.Today.AddDays(-2),
                FinishTime = DateTime.Today,
                Type = TaskItemType.Unanticipated,
                DevelopmentTeamName = "DevelopmentTeam1",
                CreatedOn = DateTime.Today.AddDays(-3),
                CreatedBy = "CreatedBy1",
                LastChangedOn = DateTime.Today,
                LastChangedBy = "LastChangedBy1",
                CurrentBoardColumn = "CurrentBoardColumn1",
                Impact = "Impact1",
                CardState = "CardState1",
                CommentCount = 1,
                NumRevisions = 3,
                Release = new Release
                {
                    Id = 1,
                    Status = "Status1",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "N1me1"
                    },
                    StartTime = DateTime.Today.AddDays(-2),
                    FinishTime = DateTime.Today,
                    Name = "Release1",
                    Attempts = 3
                }
            };
            var taskItem2 = new TaskItem
            {
                Id = 2,
                Title = "Title2",
                StartTime = DateTime.Today.AddDays(-2),
                FinishTime = DateTime.MaxValue,
                Type = TaskItemType.Unanticipated,
                DevelopmentTeamName = "DevelopmentTeam2",
                CreatedOn = DateTime.Today.AddDays(-3),
                CreatedBy = "CreatedBy2",
                LastChangedOn = DateTime.Today,
                LastChangedBy = "LastChangedBy2",
                CurrentBoardColumn = "CurrentBoardColumn2",
                Impact = "Impact2",
                CardState = "CardState2",
                CommentCount = 2,
                NumRevisions = 3,
                Release = new Release
                {
                    Id = 2,
                    Status = "Status2",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 2,
                        Name = "N2me2"
                    },
                    StartTime = DateTime.Today.AddDays(-2),
                    FinishTime = DateTime.Today,
                    Name = "Release2",
                    Attempts = 3
                }
            };
            var taskItemList = new List<TaskItem>
            {
                taskItem1,
                taskItem2
            };

            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(taskItemList);

            var taskItemHelper = new TaskItemHelper(mockTaskItemRepository.Object);

            var result = await taskItemHelper.GetTaskItems(DateTime.Today, DateTime.Today);

            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(taskItem1));
        }

        [Test]
        public async Task When_getting_work_item_card_and_dates_are_invalid()
        {
            var taskItem1 = new TaskItem
            {
                Id = 1,
                Title = "Title1",
                StartTime = DateTime.Today.AddDays(-2),
                FinishTime = DateTime.Today,
                Type = TaskItemType.Unanticipated,
                DevelopmentTeamName = "DevelopmentTeam1",
                CreatedOn = DateTime.Today.AddDays(-3),
                CreatedBy = "CreatedBy1",
                LastChangedOn = DateTime.Today,
                LastChangedBy = "LastChangedBy1",
                CurrentBoardColumn = "CurrentBoardColumn1",
                Impact = "Impact1",
                CardState = "CardState1",
                CommentCount = 1,
                NumRevisions = 3,
                Release = new Release
                {
                    Id = 1,
                    Status = "Status1",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "N1me1"
                    },
                    StartTime = DateTime.Today.AddDays(-2),
                    FinishTime = DateTime.Today,
                    Name = "Release1",
                    Attempts = 3
                }
            };
            var taskItem2 = new TaskItem
            {
                Id = 2,
                Title = "Title2",
                StartTime = DateTime.Today.AddDays(-2),
                FinishTime = DateTime.MaxValue,
                Type = TaskItemType.Unanticipated,
                DevelopmentTeamName = "DevelopmentTeam2",
                CreatedOn = DateTime.Today.AddDays(-3),
                CreatedBy = "CreatedBy2",
                LastChangedOn = DateTime.Today,
                LastChangedBy = "LastChangedBy2",
                CurrentBoardColumn = "CurrentBoardColumn2",
                Impact = "Impact2",
                CardState = "CardState2",
                CommentCount = 2,
                NumRevisions = 3,
                Release = new Release
                {
                    Id = 2,
                    Status = "Status2",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 2,
                        Name = "N2me2"
                    },
                    StartTime = DateTime.Today.AddDays(-2),
                    FinishTime = DateTime.Today,
                    Name = "Release2",
                    Attempts = 3
                }
            };
            var taskItemList = new List<TaskItem>
            {
                taskItem1,
                taskItem2
            };

            var mockTaskItemDataAccess = new Mock<ITaskItemRepository>();
            mockTaskItemDataAccess.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(taskItemList);

            var taskItemHelper = new TaskItemHelper(mockTaskItemDataAccess.Object);

            var result = await taskItemHelper.GetTaskItems(DateTime.Today, DateTime.Today);

            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(taskItem1));
        }
    }
}

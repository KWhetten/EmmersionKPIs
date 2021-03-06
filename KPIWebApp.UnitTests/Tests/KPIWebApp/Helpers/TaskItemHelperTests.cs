﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
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
                StartTime = DateTimeOffset.Now.Date.AddDays(-2),
                FinishTime = DateTimeOffset.Now.Date,
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                },
                CreatedOn = DateTimeOffset.Now.Date.AddDays(-3),
                CreatedBy = new Developer{
                    Id = 1,
                    Name = "CreatedBy1",
                },
                LastChangedOn = DateTimeOffset.Now.Date,
                LastChangedBy = new Developer{
                    Id = 1,
                    Name = "LastChangedBy1",
                },
                CurrentBoardColumn = BoardColumn.Backlog,
                State = TaskItemState.Backlog,
                NumRevisions = 3,
                Release = new Release
                {
                    Id = 1,
                    State = "Status1",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "N1me1"
                    },
                    StartTime = DateTimeOffset.Now.Date.AddDays(-2),
                    FinishTime = DateTimeOffset.Now.Date,
                    Name = "Release1",
                    Attempts = 3
                }
            };
            var taskItem2 = new TaskItem
            {
                Id = 2,
                Title = "Title2",
                StartTime = DateTimeOffset.Now.Date.AddDays(-2),
                FinishTime = null,
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Enterprise"
                },
                CreatedOn = DateTimeOffset.Now.Date.AddDays(-3),
                CreatedBy = new Developer{
                    Id = 1,
                    Name = "CreatedBy2",
                },
                LastChangedOn = DateTimeOffset.Now.Date,
                LastChangedBy = new Developer{
                    Id = 1,
                    Name = "LastChangedBy2",
                },
                CurrentBoardColumn = BoardColumn.TopPriority,
                State = TaskItemState.TopPriority,
                NumRevisions = 3,
                Release = new Release
                {
                    Id = 2,
                    State = "Status2",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 2,
                        Name = "N2me2"
                    },
                    StartTime = DateTimeOffset.Now.Date.AddDays(-2),
                    FinishTime = DateTimeOffset.Now.Date,
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
            mockTaskItemRepository.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(taskItemList);

            var taskItemHelper = new TaskItemHelper(mockTaskItemRepository.Object);

            var result = await taskItemHelper.GetTaskItems(DateTimeOffset.Now.Date, DateTimeOffset.Now.Date);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(taskItem1));
        }

        [Test]
        public async Task When_getting_work_item_card_and_dates_are_invalid()
        {
            var taskItem1 = new TaskItem
            {
                Id = 1,
                Title = "Title1",
                StartTime = DateTimeOffset.Now.Date.AddDays(-2),
                FinishTime = DateTimeOffset.Now.Date,
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                },
                CreatedOn = DateTimeOffset.Now.Date.AddDays(-3),
                CreatedBy = new Developer{
                    Id = 1,
                    Name = "CreatedBy3"
                },
                LastChangedOn = DateTimeOffset.Now.Date,
                LastChangedBy = new Developer{
                    Id = 1,
                    Name = "LastChangedBy3"
                },
                CurrentBoardColumn = BoardColumn.Backlog,
                State = TaskItemState.Backlog,
                NumRevisions = 3,
                Release = new Release
                {
                    Id = 1,
                    State = "Status1",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "N1me1"
                    },
                    StartTime = DateTimeOffset.Now.Date.AddDays(-2),
                    FinishTime = DateTimeOffset.Now.Date,
                    Name = "Release1",
                    Attempts = 3
                }
            };
            var taskItem2 = new TaskItem
            {
                Id = 2,
                Title = "Title2",
                StartTime = DateTimeOffset.Now.Date.AddDays(-2),
                FinishTime = null,
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Enterprise"
                },
                CreatedOn = DateTimeOffset.Now.Date.AddDays(-3),
                CreatedBy = new Developer{
                    Id = 1,
                    Name = "CreatedBy4"
                },
                LastChangedOn = DateTimeOffset.Now.Date,
                LastChangedBy = new Developer{
                    Id = 1,
                    Name = "LastChangedBy4"
                },
                CurrentBoardColumn = BoardColumn.InProcess,
                State = TaskItemState.InProcess,
                NumRevisions = 3,
                Release = new Release
                {
                    Id = 2,
                    State = "Status2",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 2,
                        Name = "N2me2"
                    },
                    StartTime = DateTimeOffset.Now.Date.AddDays(-2),
                    FinishTime = DateTimeOffset.Now.Date,
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
            mockTaskItemDataAccess.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(taskItemList);

            var taskItemHelper = new TaskItemHelper(mockTaskItemDataAccess.Object);

            var result = await taskItemHelper.GetTaskItems(DateTimeOffset.Now.Date, DateTimeOffset.Now.Date);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(taskItem1));
        }
    }
}

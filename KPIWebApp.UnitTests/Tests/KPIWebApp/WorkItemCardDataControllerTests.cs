using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DataAccess.DatabaseAccess;
using DataObjects.Objects;
using KPIWebApp.Controllers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp
{
    public class TaskItemDataControllerTests
    {
        // [Test]
        // public void When_getting_work_item_card_data()
        // {
        //     var TaskItem1 = new TaskItem
        //     {
        //         Id = 1,
        //         Title = "Title1",
        //         StartTime = DateTime.Today.AddDays(-2),
        //         FinishTime = DateTime.Today,
        //         Type = TaskItemType.Unanticipated,
        //         DevelopmentTeamName = "DevelopmentTeam1",
        //         CreatedOn = DateTime.Today.AddDays(-3),
        //         CreatedBy = "CreatedBy1",
        //         LastChangedOn = DateTime.Today,
        //         LastChangedBy = "LastChangedBy1",
        //         CurrentBoardColumn = "CurrentBoardColumn1",
        //         Impact = "Impact1",
        //         CardState = "CardState1",
        //         CommentCount = 1,
        //         NumRevisions = 3,
        //         Release = new Release
        //         {
        //             Id = 1,
        //             Status = "Status1",
        //             ReleaseEnvironment = new ReleaseEnvironment
        //             {
        //                 Id = 1,
        //                 Name = "N1me1"
        //             },
        //             StartTime = DateTime.Today.AddDays(-2),
        //             FinishTime = DateTime.Today,
        //             Name = "Release1",
        //             Attempts = 3
        //         }
        //     };
        //     var TaskItem2 = new TaskItem
        //     {
        //         Id = 2,
        //         Title = "Title2",
        //         StartTime = DateTime.Today.AddDays(-2),
        //         FinishTime = DateTime.MaxValue,
        //         Type = TaskItemType.Unanticipated,
        //         DevelopmentTeamName = "DevelopmentTeam2",
        //         CreatedOn = DateTime.Today.AddDays(-3),
        //         CreatedBy = "CreatedBy2",
        //         LastChangedOn = DateTime.Today,
        //         LastChangedBy = "LastChangedBy2",
        //         CurrentBoardColumn = "CurrentBoardColumn2",
        //         Impact = "Impact2",
        //         CardState = "CardState2",
        //         CommentCount = 2,
        //         NumRevisions = 3,
        //         Release = new Release
        //         {
        //             Id = 2,
        //             Status = "Status2",
        //             ReleaseEnvironment = new ReleaseEnvironment
        //             {
        //                 Id = 2,
        //                 Name = "N2me2"
        //             },
        //             StartTime = DateTime.Today.AddDays(-2),
        //             FinishTime = DateTime.Today,
        //             Name = "Release2",
        //             Attempts = 3
        //         }
        //     };
        //     var TaskItemList = new List<TaskItem>
        //     {
        //         TaskItem1,
        //         TaskItem2
        //     };
        //
        //     var mockTaskItemDataAccess = new Mock<TaskItemDataAccess>();
        //     mockTaskItemDataAccess.Setup(x => x.GetTaskItemList(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //         .Returns(TaskItemList);
        //
        //     var TaskItemDataController = new TaskItemDataController(mockTaskItemDataAccess.Object);
        //
        //     var result = TaskItemDataController.Get(DateTime.Now.ToString(), DateTime.Now.ToString());
        //
        //     Assert.That(result.Length, Is.EqualTo(1));
        //     Assert.That(result[0], Is.EqualTo(TaskItem1));
        // }
        //
        // [Test]
        // public void When_getting_work_item_card_and_dates_are_invalid()
        // {
        //     var TaskItem1 = new TaskItem
        //     {
        //         Id = 1,
        //         Title = "Title1",
        //         StartTime = DateTime.Today.AddDays(-2),
        //         FinishTime = DateTime.Today,
        //         Type = TaskItemType.Unanticipated,
        //         DevelopmentTeamName = "DevelopmentTeam1",
        //         CreatedOn = DateTime.Today.AddDays(-3),
        //         CreatedBy = "CreatedBy1",
        //         LastChangedOn = DateTime.Today,
        //         LastChangedBy = "LastChangedBy1",
        //         CurrentBoardColumn = "CurrentBoardColumn1",
        //         Impact = "Impact1",
        //         CardState = "CardState1",
        //         CommentCount = 1,
        //         NumRevisions = 3,
        //         Release = new Release
        //         {
        //             Id = 1,
        //             Status = "Status1",
        //             ReleaseEnvironment = new ReleaseEnvironment
        //             {
        //                 Id = 1,
        //                 Name = "N1me1"
        //             },
        //             StartTime = DateTime.Today.AddDays(-2),
        //             FinishTime = DateTime.Today,
        //             Name = "Release1",
        //             Attempts = 3
        //         }
        //     };
        //     var TaskItem2 = new TaskItem
        //     {
        //         Id = 2,
        //         Title = "Title2",
        //         StartTime = DateTime.Today.AddDays(-2),
        //         FinishTime = DateTime.MaxValue,
        //         Type = TaskItemType.Unanticipated,
        //         DevelopmentTeamName = "DevelopmentTeam2",
        //         CreatedOn = DateTime.Today.AddDays(-3),
        //         CreatedBy = "CreatedBy2",
        //         LastChangedOn = DateTime.Today,
        //         LastChangedBy = "LastChangedBy2",
        //         CurrentBoardColumn = "CurrentBoardColumn2",
        //         Impact = "Impact2",
        //         CardState = "CardState2",
        //         CommentCount = 2,
        //         NumRevisions = 3,
        //         Release = new Release
        //         {
        //             Id = 2,
        //             Status = "Status2",
        //             ReleaseEnvironment = new ReleaseEnvironment
        //             {
        //                 Id = 2,
        //                 Name = "N2me2"
        //             },
        //             StartTime = DateTime.Today.AddDays(-2),
        //             FinishTime = DateTime.Today,
        //             Name = "Release2",
        //             Attempts = 3
        //         }
        //     };
        //     var TaskItemList = new List<TaskItem>
        //     {
        //         TaskItem1,
        //         TaskItem2
        //     };
        //
        //     var mockTaskItemDataAccess = new Mock<TaskItemDataAccess>();
        //     mockTaskItemDataAccess.Setup(x => x.GetTaskItemList(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //         .Returns(TaskItemList);
        //
        //     var TaskItemDataController = new TaskItemDataController(mockTaskItemDataAccess.Object);
        //
        //     var result = TaskItemDataController.Get("Hahaha", "Teeheehee");
        //
        //     Assert.That(result.Length, Is.EqualTo(1));
        //     Assert.That(result[0], Is.EqualTo(TaskItem1));
        // }
    }
}

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
    public class WorkItemCardDataControllerTests
    {
        // [Test]
        // public void When_getting_work_item_card_data()
        // {
        //     var workItemCard1 = new WorkItemCard
        //     {
        //         Id = 1,
        //         Title = "Title1",
        //         StartTime = DateTime.Today.AddDays(-2),
        //         FinishTime = DateTime.Today,
        //         Type = WorkItemCardType.Unanticipated,
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
        //     var workItemCard2 = new WorkItemCard
        //     {
        //         Id = 2,
        //         Title = "Title2",
        //         StartTime = DateTime.Today.AddDays(-2),
        //         FinishTime = DateTime.MaxValue,
        //         Type = WorkItemCardType.Unanticipated,
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
        //     var workItemCardList = new List<WorkItemCard>
        //     {
        //         workItemCard1,
        //         workItemCard2
        //     };
        //
        //     var mockWorkItemCardDataAccess = new Mock<WorkItemCardDataAccess>();
        //     mockWorkItemCardDataAccess.Setup(x => x.GetWorkItemCardList(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //         .Returns(workItemCardList);
        //
        //     var workItemCardDataController = new WorkItemCardDataController(mockWorkItemCardDataAccess.Object);
        //
        //     var result = workItemCardDataController.Get(DateTime.Now.ToString(), DateTime.Now.ToString());
        //
        //     Assert.That(result.Length, Is.EqualTo(1));
        //     Assert.That(result[0], Is.EqualTo(workItemCard1));
        // }
        //
        // [Test]
        // public void When_getting_work_item_card_and_dates_are_invalid()
        // {
        //     var workItemCard1 = new WorkItemCard
        //     {
        //         Id = 1,
        //         Title = "Title1",
        //         StartTime = DateTime.Today.AddDays(-2),
        //         FinishTime = DateTime.Today,
        //         Type = WorkItemCardType.Unanticipated,
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
        //     var workItemCard2 = new WorkItemCard
        //     {
        //         Id = 2,
        //         Title = "Title2",
        //         StartTime = DateTime.Today.AddDays(-2),
        //         FinishTime = DateTime.MaxValue,
        //         Type = WorkItemCardType.Unanticipated,
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
        //     var workItemCardList = new List<WorkItemCard>
        //     {
        //         workItemCard1,
        //         workItemCard2
        //     };
        //
        //     var mockWorkItemCardDataAccess = new Mock<WorkItemCardDataAccess>();
        //     mockWorkItemCardDataAccess.Setup(x => x.GetWorkItemCardList(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
        //         .Returns(workItemCardList);
        //
        //     var workItemCardDataController = new WorkItemCardDataController(mockWorkItemCardDataAccess.Object);
        //
        //     var result = workItemCardDataController.Get("Hahaha", "Teeheehee");
        //
        //     Assert.That(result.Length, Is.EqualTo(1));
        //     Assert.That(result[0], Is.EqualTo(workItemCard1));
        // }
    }
}

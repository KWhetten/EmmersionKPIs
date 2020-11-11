using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp
{
    public class ScatterPlotHelperTests
    {
        [Test]
        public async Task When_getting_lead_time_scatter_plot_data()
        {
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
                    StartTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-5)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-4)),
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
                    StartTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-7)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 10, 27)),
                    Name = "Name2",
                    Attempts = 2
                }
            };
            var engineeringTaskItem1 = new TaskItem
            {
                Id = 1,
                Title = "Title1",
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-10)),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-5)),
                Type = TaskItemType.Engineering,
                DevelopmentTeamName = "Team1",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-8)),
                CreatedBy = "CreatedBy1",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy1",
                CurrentBoardColumn = "BoardColumn1",
                State = "State1",
                Impact = "Impact 1",
                CommentCount = 1,
                NumRevisions = 1,
                Release = releaseList.First()
            };
            var productTaskItem1 = new TaskItem
            {
                Id = 2,
                Title = "Title2",
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-5)),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-4)),
                Type = TaskItemType.Product,
                DevelopmentTeamName = "Team2",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-1)),
                CreatedBy = "CreatedBy2",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy2",
                CurrentBoardColumn = "BoardColumn2",
                State = "State2",
                Impact = "Impact 2",
                CommentCount = 2,
                NumRevisions = 2,
                Release = releaseList.Last()
            };
            var unanticipatedTaskItem1 = new TaskItem
            {
                Id = 3,
                Title = "Title3",
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-6)),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-3)),
                Type = TaskItemType.Unanticipated,
                DevelopmentTeamName = "Team3",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-1)),
                CreatedBy = "CreatedBy3",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy3",
                CurrentBoardColumn = "BoardColumn3",
                State = "State3",
                Impact = "Impact 3",
                CommentCount = 3,
                NumRevisions = 3,
                Release = releaseList.Last()
            };
            var unanticipatedTaskItem2 = new TaskItem
            {
                Id = 4,
                Title = "Title4",
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-7)),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-2)),
                Type = TaskItemType.Unanticipated,
                DevelopmentTeamName = "Team4",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-1)),
                CreatedBy = "CreatedBy4",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy4",
                CurrentBoardColumn = "BoardColumn4",
                State = "State4",
                Impact = "Impact 4",
                CommentCount = 4,
                NumRevisions = 4,
                Release = releaseList.Last()
            };
            var engineeringTaskItem2 = new TaskItem
            {
                Id = 5,
                Title = "Title5",
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-5)),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-1)),
                Type = TaskItemType.Engineering,
                DevelopmentTeamName = "Team5",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-1)),
                CreatedBy = "CreatedBy5",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy5",
                CurrentBoardColumn = "BoardColumn5",
                State = "State5",
                Impact = "Impact 5",
                CommentCount = 5,
                NumRevisions = 5,
                Release = releaseList.Last()
            };
            var taskItemList = new List<TaskItem>
            {
                engineeringTaskItem1,
                productTaskItem1,
                unanticipatedTaskItem1,
                unanticipatedTaskItem2,
                engineeringTaskItem2
            };

            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(taskItemList);
            mockTaskItemRepository.Setup(x => x.GetTaskItemTypes()).Returns(new []
            {
                TaskItemType.Product,
                TaskItemType.Engineering,
                TaskItemType.Unanticipated
            });

            var scatterPlotHelper = new ScatterPlotHelper(mockTaskItemRepository.Object);
            var result = scatterPlotHelper.GetLeadTimeScatterPlotData(new DateTimeOffset(new DateTime(2020, 10, 12), TimeSpan.Zero), new DateTimeOffset(new DateTime(2020, 10, 24), TimeSpan.Zero));

            Console.Write("");

            Assert.That(result[0].name, Is.EqualTo("Product"));
            Assert.That(result[0].turboThreshold, Is.EqualTo(500000));
            Assert.That(result[0].data.Count, Is.EqualTo(1));
            Assert.That(result[0].data[0].x, Is.EqualTo(productTaskItem1.FinishTime));
            Assert.That(result[0].data[0].y, Is.EqualTo(8m));

            Assert.That(result[1].name, Is.EqualTo("Engineering"));
            Assert.That(result[1].turboThreshold, Is.EqualTo(500000));
            Assert.That(result[1].data.Count, Is.EqualTo(2));
            Assert.That(result[1].data[0].x, Is.EqualTo(engineeringTaskItem1.FinishTime));
            Assert.That(result[1].data[0].y, Is.EqualTo(32m));
            Assert.That(result[1].data[1].x, Is.EqualTo(engineeringTaskItem2.FinishTime));
            Assert.That(result[1].data[1].y, Is.EqualTo(16m));

            Assert.That(result[2].name, Is.EqualTo("Unanticipated"));
            Assert.That(result[2].turboThreshold, Is.EqualTo(500000));
            Assert.That(result[2].data.Count, Is.EqualTo(2));
            Assert.That(result[2].data[0].x, Is.EqualTo(unanticipatedTaskItem1.FinishTime));
            Assert.That(result[2].data[0].y, Is.EqualTo(24m));
            Assert.That(result[2].data[1].x, Is.EqualTo(unanticipatedTaskItem2.FinishTime));
            Assert.That(result[2].data[1].y, Is.EqualTo(32m));
        }
    }
}

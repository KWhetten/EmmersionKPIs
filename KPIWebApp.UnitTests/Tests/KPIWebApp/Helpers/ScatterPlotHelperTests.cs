using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
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
                DevelopmentTeam = "Team1",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-8)),
                CreatedBy = "CreatedBy1",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy1",
                CurrentBoardColumn = "BoardColumn1",
                State = TaskItemState.Backlog,
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
                DevelopmentTeam = "Team2",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-1)),
                CreatedBy = "CreatedBy2",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy2",
                CurrentBoardColumn = "BoardColumn2",
                State = TaskItemState.TopPriority,
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
                DevelopmentTeam = "Team3",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-1)),
                CreatedBy = "CreatedBy3",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy3",
                CurrentBoardColumn = "BoardColumn3",
                State = TaskItemState.InProcess,
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
                DevelopmentTeam = "Team4",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-1)),
                CreatedBy = "CreatedBy4",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy4",
                CurrentBoardColumn = "BoardColumn4",
                State = TaskItemState.Released,
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
                DevelopmentTeam = "Team5",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddDays(-1)),
                CreatedBy = "CreatedBy5",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy5",
                CurrentBoardColumn = "BoardColumn5",
                State = TaskItemState.Backlog,
                NumRevisions = 5,
                Release = releaseList.Last()
            };
            var taskFrom2YearsAgo = new TaskItem
            {
                Id = 6,
                Title = "Title6",
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddYears(-6)),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 27).AddYears(-2)),
                Type = TaskItemType.Engineering,
                DevelopmentTeam = "Team6",
                CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 27).AddYears(-2)),
                CreatedBy = "CreatedBy6",
                LastChangedOn = new DateTimeOffset(new DateTime(2020, 10, 27)),
                LastChangedBy = "ChangedBy6",
                CurrentBoardColumn = "BoardColumn6",
                State = TaskItemState.TopPriority,
                NumRevisions = 6,
                Release = releaseList.Last()
            };
            var taskItemList = new List<TaskItem>
            {
                engineeringTaskItem1,
                productTaskItem1,
                unanticipatedTaskItem1,
                unanticipatedTaskItem2,
                engineeringTaskItem2,
                taskFrom2YearsAgo
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
            var result = await scatterPlotHelper.GetLeadTimeScatterPlotData(new DateTimeOffset(new DateTime(2020, 10, 12), TimeSpan.Zero), new DateTimeOffset(new DateTime(2020, 10, 24), TimeSpan.Zero), true, true, true);

            Console.Write("");

            Assert.That(result[TaskItemType.Product].name, Is.EqualTo("Product"));
            Assert.That(result[TaskItemType.Product].turboThreshold, Is.EqualTo(500000));
            Assert.That(result[TaskItemType.Product].data.Count, Is.EqualTo(1));
            Assert.That(result[TaskItemType.Product].data[0].x, Is.EqualTo(productTaskItem1.FinishTime));
            Assert.That(result[TaskItemType.Product].data[0].y, Is.EqualTo(8m));

            Assert.That(result[TaskItemType.Engineering].name, Is.EqualTo("Engineering"));
            Assert.That(result[TaskItemType.Engineering].turboThreshold, Is.EqualTo(500000));
            Assert.That(result[TaskItemType.Engineering].data.Count, Is.EqualTo(2));
            Assert.That(result[TaskItemType.Engineering].data[0].x, Is.EqualTo(engineeringTaskItem1.FinishTime));
            Assert.That(result[TaskItemType.Engineering].data[0].y, Is.EqualTo(32m));
            Assert.That(result[TaskItemType.Engineering].data[1].x, Is.EqualTo(engineeringTaskItem2.FinishTime));
            Assert.That(result[TaskItemType.Engineering].data[1].y, Is.EqualTo(16m));

            Assert.That(result[TaskItemType.Unanticipated].name, Is.EqualTo("Unanticipated"));
            Assert.That(result[TaskItemType.Unanticipated].turboThreshold, Is.EqualTo(500000));
            Assert.That(result[TaskItemType.Unanticipated].data.Count, Is.EqualTo(2));
            Assert.That(result[TaskItemType.Unanticipated].data[0].x, Is.EqualTo(unanticipatedTaskItem1.FinishTime));
            Assert.That(result[TaskItemType.Unanticipated].data[0].y, Is.EqualTo(24m));
            Assert.That(result[TaskItemType.Unanticipated].data[1].x, Is.EqualTo(unanticipatedTaskItem2.FinishTime));
            Assert.That(result[TaskItemType.Unanticipated].data[1].y, Is.EqualTo(32m));
        }
    }
}

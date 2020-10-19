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
            var engineeringTaskItem1 = new TaskItem
            {
                Id = 1,
                Title = "Title1",
                StartTime = DateTime.Today.AddDays(-10),
                FinishTime = DateTime.Today.AddDays(-5),
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
            };
            var productTaskItem1 = new TaskItem
            {
                Id = 2,
                Title = "Title2",
                StartTime = DateTime.Today.AddDays(-5),
                FinishTime = DateTime.Today.AddDays(-4),
                Type = TaskItemType.Product,
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
            };
            var unanticipatedTaskItem1 = new TaskItem
            {
                Id = 3,
                Title = "Title3",
                StartTime = DateTime.Today.AddDays(-6),
                FinishTime = DateTime.Today.AddDays(-3),
                Type = TaskItemType.Unanticipated,
                DevelopmentTeamName = "Team3",
                CreatedOn = DateTime.Today.AddDays(-1),
                CreatedBy = "CreatedBy3",
                LastChangedOn = DateTime.Today,
                LastChangedBy = "ChangedBy3",
                CurrentBoardColumn = "BoardColumn3",
                CardState = "State3",
                Impact = "Impact 3",
                CommentCount = 3,
                NumRevisions = 3,
                Release = releaseList.Last()
            };
            var unanticipatedTaskItem2 = new TaskItem
            {
                Id = 4,
                Title = "Title4",
                StartTime = DateTime.Today.AddDays(-7),
                FinishTime = DateTime.Today.AddDays(-2),
                Type = TaskItemType.Unanticipated,
                DevelopmentTeamName = "Team4",
                CreatedOn = DateTime.Today.AddDays(-1),
                CreatedBy = "CreatedBy4",
                LastChangedOn = DateTime.Today,
                LastChangedBy = "ChangedBy4",
                CurrentBoardColumn = "BoardColumn4",
                CardState = "State4",
                Impact = "Impact 4",
                CommentCount = 4,
                NumRevisions = 4,
                Release = releaseList.Last()
            };
            var engineeringTaskItem2 = new TaskItem
            {
                Id = 5,
                Title = "Title5",
                StartTime = DateTime.Today.AddDays(-5),
                FinishTime = DateTime.Today.AddDays(-1),
                Type = TaskItemType.Engineering,
                DevelopmentTeamName = "Team5",
                CreatedOn = DateTime.Today.AddDays(-1),
                CreatedBy = "CreatedBy5",
                LastChangedOn = DateTime.Today,
                LastChangedBy = "ChangedBy5",
                CurrentBoardColumn = "BoardColumn5",
                CardState = "State5",
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
            mockTaskItemRepository.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(taskItemList);
            mockTaskItemRepository.Setup(x => x.GetTaskItemTypesAsync()).ReturnsAsync(new []
            {
                TaskItemType.Product,
                TaskItemType.Engineering,
                TaskItemType.Unanticipated
            });

            var scatterPlotHelper = new ScatterPlotHelper(mockTaskItemRepository.Object);
            var result = await scatterPlotHelper.GetLeadTimeScatterPlotData(DateTime.Today.AddDays(-10), DateTime.Today);

            Console.Write("");

            Assert.That(result[0].name, Is.EqualTo("Product"));
            Assert.That(result[0].turboThreshold, Is.EqualTo(500000));
            Assert.That(result[0].data.Count, Is.EqualTo(1));
            Assert.That(result[0].data[0].x, Is.EqualTo(productTaskItem1.FinishTime));
            Assert.That(result[0].data[0].y, Is.EqualTo((productTaskItem1.FinishTime - productTaskItem1.StartTime).TotalHours / 24));

            Assert.That(result[1].name, Is.EqualTo("Engineering"));
            Assert.That(result[1].turboThreshold, Is.EqualTo(500000));
            Assert.That(result[1].data.Count, Is.EqualTo(2));
            Assert.That(result[1].data[0].x, Is.EqualTo(engineeringTaskItem1.FinishTime));
            Assert.That(result[1].data[0].y, Is.EqualTo((engineeringTaskItem1.FinishTime - engineeringTaskItem1.StartTime).TotalHours / 24));
            Assert.That(result[1].data[1].x, Is.EqualTo(engineeringTaskItem2.FinishTime));
            Assert.That(result[1].data[1].y, Is.EqualTo((engineeringTaskItem2.FinishTime - engineeringTaskItem2.StartTime).TotalHours / 24));

            Assert.That(result[2].name, Is.EqualTo("Unanticipated"));
            Assert.That(result[2].turboThreshold, Is.EqualTo(500000));
            Assert.That(result[2].data.Count, Is.EqualTo(2));
            Assert.That(result[2].data[0].x, Is.EqualTo(unanticipatedTaskItem1.FinishTime));
            Assert.That(result[2].data[0].y, Is.EqualTo((unanticipatedTaskItem1.FinishTime - unanticipatedTaskItem1.StartTime).TotalHours / 24));
            Assert.That(result[2].data[1].x, Is.EqualTo(unanticipatedTaskItem2.FinishTime));
            Assert.That(result[2].data[1].y, Is.EqualTo((unanticipatedTaskItem2.FinishTime - unanticipatedTaskItem2.StartTime).TotalHours / 24));
        }
    }
}

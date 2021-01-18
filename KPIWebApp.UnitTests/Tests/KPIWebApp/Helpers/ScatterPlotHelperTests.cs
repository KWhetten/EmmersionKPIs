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
        private List<TaskItem> taskItems;

        [SetUp]
        public void Setup()
        {
            var productTaskItem1 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 15, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Product,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
            };
            var productTaskItem2 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 14, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Product,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
            };
            var productTaskItem3 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 13, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Product,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
            };
            var productTaskItem4 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 12, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Product,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 5,
                    Name = "Enterprise"
                }
            };
            var productTaskItem5 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 9, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Product,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 5,
                    Name = "Enterprise"
                }
            };

            var engineeringTaskItem1 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 9, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Engineering,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
            };
            var engineeringTaskItem2 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 12, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Engineering,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
            };
            var engineeringTaskItem3 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 13, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 14, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Engineering,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
            };
            var engineeringTaskItem4 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 14, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Engineering,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 5,
                    Name = "Enterprise"
                }
            };
            var engineeringTaskItem5 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 15, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 17, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Engineering,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 5,
                    Name = "Enterprise"
                }
            };
            var unanticipatedTaskItem1 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 9, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
            };
            var unanticipatedTaskItem2 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 12, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 18, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
            };
            var unanticipatedTaskItem3 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 13, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                }
            };
            var unanticipatedTaskItem4 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 14, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 15, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 5,
                    Name = "Enterprise"
                }
            };
            var unanticipatedTaskItem5 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 15, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 5,
                    Name = "Enterprise"
                }
            };
            var unanticipatedTaskItem6 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 1, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 17, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 5,
                    Name = "Enterprise"
                }
            };

           taskItems = new List<TaskItem>
            {
                productTaskItem1,
                productTaskItem2,
                productTaskItem3,
                productTaskItem4,
                productTaskItem5,
                engineeringTaskItem1,
                engineeringTaskItem2,
                engineeringTaskItem3,
                engineeringTaskItem4,
                engineeringTaskItem5,
                unanticipatedTaskItem1,
                unanticipatedTaskItem2,
                unanticipatedTaskItem3,
                unanticipatedTaskItem4,
                unanticipatedTaskItem5,
                unanticipatedTaskItem6
            };
        }
        [Test]
        public async Task When_getting_scatter_plot_data()
        {
            var mockTaskItemRepository = new Mock<TaskItemRepository>();
            mockTaskItemRepository
                .Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(taskItems);

            var scatterPlotHelper = new ScatterPlotHelper(mockTaskItemRepository.Object);

            var result = await scatterPlotHelper.GetLeadTimeScatterPlotDataAsync(new DateTimeOffset(new DateTime(2021, 1, 11)),
                new DateTimeOffset(new DateTime(2021, 1, 16)),
                true, true, true, true, true);

            Console.WriteLine();

            Assert.That(result.Values.First().data[0].y, Is.EqualTo(8));
            Assert.That(result.Values.First().data[1].y, Is.EqualTo(16));
            Assert.That(result.Values.First().data[2].y, Is.EqualTo(24));
            Assert.That(result.Values.First().data[3].y, Is.EqualTo(32));
            Assert.That(result.Values.First().data[4].y, Is.EqualTo(40));

            Assert.That(result.Values.Last().data[0].y, Is.EqualTo(40));
            Assert.That(result.Values.Last().data[1].y, Is.EqualTo(40));
            Assert.That(result.Values.Last().data[2].y, Is.EqualTo(24));
            Assert.That(result.Values.Last().data[3].y, Is.EqualTo(8));
            Assert.That(result.Values.Last().data[4].y, Is.EqualTo(8));
            Assert.That(result.Values.Last().data[5].y, Is.EqualTo(96));
        }
    }
}

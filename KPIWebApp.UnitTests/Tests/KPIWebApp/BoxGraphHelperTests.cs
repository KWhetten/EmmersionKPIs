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
    public class BoxGraphHelperTests
    {
        [Test]
        public async Task When_getting_lead_time_box_graph_data()
        {
            var productTaskItem1 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 15, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Product
            };
            var productTaskItem2 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 14, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Product
            };
            var productTaskItem3 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 13, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Product
            };
            var productTaskItem4 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 12, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Product
            };
            var productTaskItem5 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 9, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Product
            };
            
            var engineeringTaskItem1 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 9, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Engineering
            };
            var engineeringTaskItem2 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 12, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Engineering
            };
            var engineeringTaskItem3 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 13, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Engineering
            };
            var engineeringTaskItem4 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 14, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Engineering
            };
            var engineeringTaskItem5 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 15, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Engineering
            };
            var unanticipatedTaskItem1 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 9, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated
            };
            var unanticipatedTaskItem2 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 12, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated
            };
            var unanticipatedTaskItem3 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 13, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated
            };
            var unanticipatedTaskItem4 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 14, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated
            };
            var unanticipatedTaskItem5 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 15, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated
            };
            var unanticipatedTaskItem6 = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2020, 10, 1, 5, 30, 0), TimeSpan.Zero),
                FinishTime = new DateTimeOffset(new DateTime(2020, 10, 16, 5, 30, 0), TimeSpan.Zero),
                Type = TaskItemType.Unanticipated
            };

            var taskItemList = new List<TaskItem>
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

            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(taskItemList);

            var boxGraphHelper = new BoxGraphHelper(mockTaskItemRepository.Object);
           var result = await  boxGraphHelper.GetLeadTimeBoxGraphData(new DateTimeOffset(new DateTime(2020, 10, 12), TimeSpan.Zero), new DateTimeOffset(new DateTime(2020, 10, 24 ), TimeSpan.Zero));

            Assert.IsNotNull(result);
            Assert.That(result.Entries.Count, Is.EqualTo(4));
            Assert.That(result.Entries.ElementAt(0).Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries.ElementAt(0).LowerQuartile, Is.EqualTo(16m));
            Assert.That(result.Entries.ElementAt(0).Median, Is.EqualTo(24m));
            Assert.That(result.Entries.ElementAt(0).UpperQuartile, Is.EqualTo(32m));
            Assert.That(result.Entries.ElementAt(0).Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries.ElementAt(0).TaskItemType, Is.EqualTo("Product"));

            Assert.That(result.Entries.ElementAt(1).Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries.ElementAt(1).LowerQuartile, Is.EqualTo(16m));
            Assert.That(result.Entries.ElementAt(1).Median, Is.EqualTo(24m));
            Assert.That(result.Entries.ElementAt(1).UpperQuartile, Is.EqualTo(32m));
            Assert.That(result.Entries.ElementAt(1).Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries.ElementAt(1).TaskItemType, Is.EqualTo("Engineering"));

            Assert.That(result.Entries.ElementAt(2).Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries.ElementAt(2).LowerQuartile, Is.EqualTo(16m));
            Assert.That(result.Entries.ElementAt(2).Median, Is.EqualTo(24m));
            Assert.That(result.Entries.ElementAt(2).UpperQuartile, Is.EqualTo(32m));
            Assert.That(result.Entries.ElementAt(2).Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries.ElementAt(2).TaskItemType, Is.EqualTo("Unanticipated"));

            Assert.That(result.Outliers.Count, Is.EqualTo(2));
            Assert.That(result.Outliers[0], Is.EqualTo(new object[] {2, 88m}));
            Assert.That(result.Outliers[1], Is.EqualTo(new object[] {3, 88m}));
        }
    }
}

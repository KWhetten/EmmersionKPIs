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
    public class BoxGraphHelperTests
    {
        private List<TaskItem> taskItemList;

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
                    Id = 4,
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
                    Id = 4,
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
                    Id = 4,
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
                    Id = 4,
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
                    Id = 4,
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
                    Id = 4,
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
                    Id = 4,
                    Name = "Enterprise"
                }
            };

            taskItemList = new List<TaskItem>
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
        public async Task When_getting_lead_time_box_graph_data()
        {
            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository.Setup(x =>
                    x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(taskItemList);

            var boxGraphHelper = new BoxGraphHelper(mockTaskItemRepository.Object, new TaskItemHelper());
            var result = await boxGraphHelper.GetLeadTimeBoxGraphData(
                new DateTimeOffset(new DateTime(2020, 10, 12), TimeSpan.Zero),
                new DateTimeOffset(new DateTime(2020, 10, 24), TimeSpan.Zero),
                true, true, true, true, true);

            Assert.IsNotNull(result);
            Assert.That(result.Entries.Count, Is.EqualTo(4));
            Assert.That(result.Entries[0].Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries[0].LowerQuartile, Is.EqualTo(16m));
            Assert.That(result.Entries[0].Median, Is.EqualTo(24m));
            Assert.That(result.Entries[0].UpperQuartile, Is.EqualTo(32m));
            Assert.That(result.Entries[0].Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries[0].TaskItemType, Is.EqualTo("Product"));

            Assert.That(result.Entries[1].Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries[1].LowerQuartile, Is.EqualTo(16m));
            Assert.That(result.Entries[1].Median, Is.EqualTo(16m));
            Assert.That(result.Entries[1].UpperQuartile, Is.EqualTo(32m));
            Assert.That(result.Entries[1].Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries[1].TaskItemType, Is.EqualTo("Engineering"));

            Assert.That(result.Entries[2].Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries[2].LowerQuartile, Is.EqualTo(8m));
            Assert.That(result.Entries[2].Median, Is.EqualTo(24m));
            Assert.That(result.Entries[2].UpperQuartile, Is.EqualTo(40m));
            Assert.That(result.Entries[2].Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries[2].TaskItemType, Is.EqualTo("Unanticipated"));

            Assert.That(result.Entries[3].Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries[3].LowerQuartile, Is.EqualTo(8m));
            Assert.That(result.Entries[3].Median, Is.EqualTo(24m));
            Assert.That(result.Entries[3].UpperQuartile, Is.EqualTo(40m));
            Assert.That(result.Entries[3].Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries[3].TaskItemType, Is.EqualTo("Selected Task Items"));

            Assert.That(result.Outliers.Count, Is.EqualTo(2));
            Assert.That(result.Outliers[0], Is.EqualTo(new object[] {2, 96m}));
            Assert.That(result.Outliers[1], Is.EqualTo(new object[] {3, 96m}));
        }

        [Test]
        public async Task When_getting_box_graph_data_for_only_product_task_items()
        {
            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository.Setup(x =>
                    x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(taskItemList);

            var boxGraphHelper = new BoxGraphHelper(mockTaskItemRepository.Object, new TaskItemHelper());
            var result = await boxGraphHelper.GetLeadTimeBoxGraphData(
                new DateTimeOffset(new DateTime(2020, 10, 12), TimeSpan.Zero),
                new DateTimeOffset(new DateTime(2020, 10, 24), TimeSpan.Zero),
                true, false, false, true, true);

            Assert.IsNotNull(result);
            Assert.That(result.Entries.Count, Is.EqualTo(4));
            Assert.That(result.Entries[0].Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries[0].LowerQuartile, Is.EqualTo(16m));
            Assert.That(result.Entries[0].Median, Is.EqualTo(24m));
            Assert.That(result.Entries[0].UpperQuartile, Is.EqualTo(32m));
            Assert.That(result.Entries[0].Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries[0].TaskItemType, Is.EqualTo("Product"));

            Assert.That(result.Entries[1].Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries[1].LowerQuartile, Is.EqualTo(16m));
            Assert.That(result.Entries[1].Median, Is.EqualTo(16m));
            Assert.That(result.Entries[1].UpperQuartile, Is.EqualTo(32m));
            Assert.That(result.Entries[1].Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries[1].TaskItemType, Is.EqualTo("Engineering"));

            Assert.That(result.Entries[2].Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries[2].LowerQuartile, Is.EqualTo(8m));
            Assert.That(result.Entries[2].Median, Is.EqualTo(24m));
            Assert.That(result.Entries[2].UpperQuartile, Is.EqualTo(40m));
            Assert.That(result.Entries[2].Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries[2].TaskItemType, Is.EqualTo("Unanticipated"));

            Assert.That(result.Entries[3].Minimum, Is.EqualTo(8m));
            Assert.That(result.Entries[3].LowerQuartile, Is.EqualTo(16m));
            Assert.That(result.Entries[3].Median, Is.EqualTo(24m));
            Assert.That(result.Entries[3].UpperQuartile, Is.EqualTo(32m));
            Assert.That(result.Entries[3].Maximum, Is.EqualTo(40m));
            Assert.That(result.Entries[3].TaskItemType, Is.EqualTo("Selected Task Items"));

            Assert.That(result.Outliers.Count, Is.EqualTo(1));
            Assert.That(result.Outliers[0], Is.EqualTo(new object[] {2, 96m}));
        }

        [Test]
        public void When_getting_minimum_outliers()
        {
            var taskList = new List<TaskItem>
            {
                new TaskItem
                {
                    LeadTimeHours = .01m
                },
                new TaskItem
                {
                    LeadTimeHours = 1.0m
                },
                new TaskItem
                {
                    LeadTimeHours = 3.5m
                },
                new TaskItem
                {
                    LeadTimeHours = 4.0m
                },
                new TaskItem
                {
                    LeadTimeHours = 5.2m
                },
                new TaskItem
                {
                    LeadTimeHours = 100.65m
                }
            };

            var boxGraphHelper = new BoxGraphHelper();
            var result = boxGraphHelper.GetMinimumAndOutliers(taskList, .5m, new List<object[]>(), 1);

            Assert.That(result.Item2.Count, Is.EqualTo(1));
            Assert.That(result.Item2[0][1], Is.EqualTo(.01m));
        }

        [Test]
        public void When_getting_lead_time_and_list_is_empty()
        {
            var boxGraphHelper = new BoxGraphHelper();
            var result = boxGraphHelper.CalculateBoxGraphData(new List<TaskItem>(), new BoxGraphData(), 0);

            Assert.That(result, Is.EqualTo(null));
        }
    }
}

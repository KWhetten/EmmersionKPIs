using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class CumulativeFlowDiagramHelperTests
    {
        [Test]
        public async Task When_getting_cumulative_flow_data()
        {
            var taskItem = new TaskItem
            {
                Id = 1,
                DevelopmentTeam = "Assessments",
                StartTime = DateTimeOffset.Now.Date.AddDays(-4),
                Type = TaskItemType.Product,
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-5),
                        TaskItemState = TaskItemState.Backlog
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-4),
                        TaskItemState = TaskItemState.TopPriority
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-3),
                        TaskItemState = TaskItemState.InProcess
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-2),
                        TaskItemState = TaskItemState.Released
                    }
                }
            };

            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository
                .Setup(x
                    => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<TaskItem> {taskItem});

            mockTaskItemRepository.Setup(x => x.GetHistoryEventsByTaskIdAsync(It.IsAny<int>()))
                .ReturnsAsync(taskItem.HistoryEvents);

            var cumulativeFlowDiagramHelper = new CumulativeFlowDiagramHelper(mockTaskItemRepository.Object);

            var result = await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(DateTime.Now.Date.AddDays(-5), DateTime.Now.Date,
                true, true, true, true, true);

            Assert.IsNotNull(result);
            Assert.That(result.data[0].data, Is.EqualTo(new List<int>{1,0,0,0}));
            Assert.That(result.data[1].data, Is.EqualTo(new List<int>{0,1,0,0}));
            Assert.That(result.data[2].data, Is.EqualTo(new List<int>{0,0,1,0}));
            Assert.That(result.data[3].data, Is.EqualTo(new List<int>{0,0,0,1}));
        }

        [Test]
        public async Task When_getting_cumulative_flow_data_with_specified_task_item_types()
        {

            var taskItem = new TaskItem
            {
                Id = 1,
                DevelopmentTeam = "Assessments",
                Type = TaskItemType.Product,
                StartTime = DateTimeOffset.Now.Date.AddDays(-4),
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-5),
                        TaskItemState = TaskItemState.Backlog
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-4),
                        TaskItemState = TaskItemState.TopPriority
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-3),
                        TaskItemState = TaskItemState.InProcess
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-2),
                        TaskItemState = TaskItemState.Released
                    }
                }
            };
            var taskItem2 = new TaskItem
            {
                Id = 2,
                DevelopmentTeam = "Assessments",
                Type = TaskItemType.Engineering,
                StartTime = DateTimeOffset.Now.Date.AddDays(-4),
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-4),
                        TaskItemState = TaskItemState.Backlog
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-3),
                        TaskItemState = TaskItemState.TopPriority
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-2),
                        TaskItemState = TaskItemState.InProcess
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-1),
                        TaskItemState = TaskItemState.Released
                    }
                }
            };
            var taskItem3 = new TaskItem
            {
                Id = 3,
                DevelopmentTeam = "Assessments",
                Type = TaskItemType.Unanticipated,
                StartTime = DateTimeOffset.Now.Date.AddDays(-4),
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-3),
                        TaskItemState = TaskItemState.Backlog
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-2),
                        TaskItemState = TaskItemState.TopPriority
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-1),
                        TaskItemState = TaskItemState.InProcess
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date,
                        TaskItemState = TaskItemState.Released
                    }
                }
            };


            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository
                .Setup(x
                    => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<TaskItem> {taskItem, taskItem2, taskItem3});

            mockTaskItemRepository.SetupSequence(x => x.GetHistoryEventsByTaskIdAsync(It.IsAny<int>()))
                .ReturnsAsync(taskItem.HistoryEvents)
                .ReturnsAsync(taskItem2.HistoryEvents)
                .ReturnsAsync(taskItem3.HistoryEvents);

            var cumulativeFlowDiagramHelper = new CumulativeFlowDiagramHelper(mockTaskItemRepository.Object);

            var result = await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(DateTime.Now.Date.AddDays(-5), DateTime.Now.Date,
                true, true, false, true, true);

            Assert.IsNotNull(result);
            Assert.That(result.data[0].data, Is.EqualTo(new List<int>{1,1,0,0,0,0}));
            Assert.That(result.data[1].data, Is.EqualTo(new List<int>{0,1,1,0,0,0}));
            Assert.That(result.data[2].data, Is.EqualTo(new List<int>{0,0,1,1,0,0}));
            Assert.That(result.data[3].data, Is.EqualTo(new List<int>{0,0,0,1,2,2}));
        }

        [Test]
        public async Task When_getting_cumulative_flow_data_with_specified_development_teams()
        {

            var taskItem = new TaskItem
            {
                Id = 1,
                DevelopmentTeam = "Assessments",
                Type = TaskItemType.Product,
                StartTime = DateTimeOffset.Now.Date.AddDays(-4),
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-5),
                        TaskItemState = TaskItemState.Backlog
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-4),
                        TaskItemState = TaskItemState.TopPriority
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-3),
                        TaskItemState = TaskItemState.InProcess
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-2),
                        TaskItemState = TaskItemState.Released
                    }
                }
            };
            var taskItem2 = new TaskItem
            {
                Id = 2,
                DevelopmentTeam = "Enterprise",
                Type = TaskItemType.Engineering,
                StartTime = DateTimeOffset.Now.Date.AddDays(-4),
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-4),
                        TaskItemState = TaskItemState.Backlog
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-3),
                        TaskItemState = TaskItemState.TopPriority
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-2),
                        TaskItemState = TaskItemState.InProcess
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-1),
                        TaskItemState = TaskItemState.Released
                    }
                }
            };
            var taskItem3 = new TaskItem
            {
                Id = 3,
                DevelopmentTeam = "Assessments",
                Type = TaskItemType.Unanticipated,
                StartTime = DateTimeOffset.Now.Date.AddDays(-4),
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-3),
                        TaskItemState = TaskItemState.Backlog
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-2),
                        TaskItemState = TaskItemState.TopPriority
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date.AddDays(-1),
                        TaskItemState = TaskItemState.InProcess
                    },
                    new HistoryEvent
                    {
                        EventDate = DateTimeOffset.Now.Date,
                        TaskItemState = TaskItemState.Released
                    }
                }
            };


            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository
                .Setup(x
                    => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<TaskItem> {taskItem, taskItem2, taskItem3});

            mockTaskItemRepository.SetupSequence(x => x.GetHistoryEventsByTaskIdAsync(It.IsAny<int>()))
                .ReturnsAsync(taskItem.HistoryEvents)
                .ReturnsAsync(taskItem2.HistoryEvents)
                .ReturnsAsync(taskItem3.HistoryEvents);

            var cumulativeFlowDiagramHelper = new CumulativeFlowDiagramHelper(mockTaskItemRepository.Object);

            var result = await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(DateTime.Now.Date.AddDays(-5), DateTime.Now.Date,
                true, true, true, true, false);

            Assert.IsNotNull(result);
            Assert.That(result.data[0].data, Is.EqualTo(new List<int>{1,0,1,0,0,0}));
            Assert.That(result.data[1].data, Is.EqualTo(new List<int>{0,1,0,1,0,0}));
            Assert.That(result.data[2].data, Is.EqualTo(new List<int>{0,0,1,0,1,0}));
            Assert.That(result.data[3].data, Is.EqualTo(new List<int>{0,0,0,1,1,2}));
        }
    }
}

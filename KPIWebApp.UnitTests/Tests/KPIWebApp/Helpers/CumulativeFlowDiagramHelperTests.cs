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
        private List<TaskItem> taskItems = new List<TaskItem>();

        [SetUp]
        public void SetUp()
        {
            taskItems = new List<TaskItem>{
                new TaskItem
                {
                    Id = 1,
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                    Type = TaskItemType.Product,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                },
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            EventType = "Created on",
                            TaskItemState = TaskItemState.Backlog,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 11))
                        },
                        new HistoryEvent
                        {
                            EventType = "Task moved",
                            TaskItemState = TaskItemState.TopPriority,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 12))
                        },
                        new HistoryEvent
                        {
                            EventType = "Task moved",
                            TaskItemState = TaskItemState.InProcess,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 13))
                        },
                        new HistoryEvent
                        {
                            EventType = "Task moved",
                            TaskItemState = TaskItemState.Released,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 15))
                        }
                    }
                },
                new TaskItem
                {
                    Id = 2,
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 12)),
                    Type = TaskItemType.Engineering,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Enterprise"
                },
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            EventType = "Created on",
                            TaskItemState = TaskItemState.Backlog,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 12))
                        },
                        new HistoryEvent
                        {
                            EventType = "Task moved",
                            TaskItemState = TaskItemState.TopPriority,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 13))
                        },
                        new HistoryEvent
                        {
                            EventType = "Task moved",
                            TaskItemState = TaskItemState.InProcess,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 15))
                        },
                        new HistoryEvent
                        {
                            EventType = "Task moved",
                            TaskItemState = TaskItemState.Released,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 17))
                        }
                    }
                },
                new TaskItem
                {
                    Id = 3,
                    StartTime = new DateTimeOffset(new DateTime(2021, 1, 14)),
                    Type = TaskItemType.Unanticipated,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                },
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            EventType = "Created on",
                            TaskItemState = TaskItemState.Backlog,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 14))
                        },
                        new HistoryEvent
                        {
                            EventType = "Task moved",
                            TaskItemState = TaskItemState.TopPriority,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 14))
                        },
                        new HistoryEvent
                        {
                            EventType = "Task moved",
                            TaskItemState = TaskItemState.InProcess,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 17))
                        },
                        new HistoryEvent
                        {
                            EventType = "Task moved",
                            TaskItemState = TaskItemState.Released,
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 18))
                        }
                    }
                },
                new TaskItem
                {
                    Id = 4,
                    Type = TaskItemType.Unanticipated,
                    DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 4,
                    Name = "Assessments"
                },
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            EventDate = new DateTimeOffset(new DateTime(2021, 1, 18))
                        }
                    }
                }
            }
            ;
        }

        [Test]
        public async Task When_getting_cumulative_flow_data()
        {
            var mockTaskItemRepository = new Mock<TaskItemRepository>();
            mockTaskItemRepository
                .Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(taskItems);

            var mockHistoryEventRepository = new Mock<HistoryEventRepository>();
            mockHistoryEventRepository.SetupSequence(x => x.GetHistoryEventsByTaskIdAsync(It.IsAny<int>()))
                .ReturnsAsync(taskItems[0].HistoryEvents)
                .ReturnsAsync(taskItems[1].HistoryEvents)
                .ReturnsAsync(taskItems[2].HistoryEvents)
                .ReturnsAsync(taskItems[3].HistoryEvents);

            var cumulativeFlowDiagramHelper =
                new CumulativeFlowDiagramHelper(mockTaskItemRepository.Object, mockHistoryEventRepository.Object);

            var result = await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(
                new DateTimeOffset(new DateTime(2021, 1, 11)),
                new DateTimeOffset(new DateTime(2021, 1, 17)),
                true, true, true, true, true);

            foreach (var res in result.data)
            {
                foreach (var datum in res.data)
                {
                    Console.Write($"{datum}, ");
                }
                Console.Write("\n");
            }

            Assert.That(result.data[0].data, Is.EqualTo(new List<int> {1, 1, 0, 0, 0, 0, 0}));
            Assert.That(result.data[1].data, Is.EqualTo(new List<int> {0, 1, 1, 2, 1, 1, 0}));
            Assert.That(result.data[2].data, Is.EqualTo(new List<int> {0, 0, 1, 1, 1, 1, 1}));
            Assert.That(result.data[3].data, Is.EqualTo(new List<int> {0, 0, 0, 0, 1, 1, 2}));
        }

        [Test]
        public async Task When_getting_cumulative_flow_data_for_specific_task_items()
        {
            var mockTaskItemRepository = new Mock<TaskItemRepository>();
            mockTaskItemRepository
                .Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(taskItems);

            var mockHistoryEventRepository = new Mock<HistoryEventRepository>();
            mockHistoryEventRepository.SetupSequence(x => x.GetHistoryEventsByTaskIdAsync(It.IsAny<int>()))
                .ReturnsAsync(taskItems[0].HistoryEvents)
                .ReturnsAsync(taskItems[1].HistoryEvents)
                .ReturnsAsync(taskItems[2].HistoryEvents)
                .ReturnsAsync(taskItems[3].HistoryEvents);

            var cumulativeFlowDiagramHelper =
                new CumulativeFlowDiagramHelper(mockTaskItemRepository.Object, mockHistoryEventRepository.Object);

            var result = await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(
                new DateTimeOffset(new DateTime(2021, 1, 11)),
                new DateTimeOffset(new DateTime(2021, 1, 17)),
                true, false, false, true, true);

            Assert.That(result.data[0].data, Is.EqualTo(new List<int> {1, 0, 0, 0, 0, 0, 0}));
            Assert.That(result.data[1].data, Is.EqualTo(new List<int> {0, 1, 0, 0, 0, 0, 0}));
            Assert.That(result.data[2].data, Is.EqualTo(new List<int> {0, 0, 1, 1, 0, 0, 0}));
            Assert.That(result.data[3].data, Is.EqualTo(new List<int> {0, 0, 0, 0, 1, 1, 1}));
        }

        [Test]
        public async Task When_getting_cumulative_flow_data_for_specific_development_teams()
        {
            var mockTaskItemRepository = new Mock<TaskItemRepository>();
            mockTaskItemRepository
                .Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(taskItems);

            var mockHistoryEventRepository = new Mock<HistoryEventRepository>();
            mockHistoryEventRepository.SetupSequence(x => x.GetHistoryEventsByTaskIdAsync(It.IsAny<int>()))
                .ReturnsAsync(taskItems[0].HistoryEvents)
                .ReturnsAsync(taskItems[1].HistoryEvents)
                .ReturnsAsync(taskItems[2].HistoryEvents)
                .ReturnsAsync(taskItems[3].HistoryEvents);

            var cumulativeFlowDiagramHelper =
                new CumulativeFlowDiagramHelper(mockTaskItemRepository.Object, mockHistoryEventRepository.Object);

            var result = await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(
                new DateTimeOffset(new DateTime(2021, 1, 11)),
                new DateTimeOffset(new DateTime(2021, 1, 17)),
                true, false, false, true, true);

            Assert.That(result.data[0].data, Is.EqualTo(new List<int> {1, 0, 0, 0, 0, 0, 0}));
            Assert.That(result.data[1].data, Is.EqualTo(new List<int> {0, 1, 0, 0, 0, 0, 0}));
            Assert.That(result.data[2].data, Is.EqualTo(new List<int> {0, 0, 1, 1, 0, 0, 0}));
            Assert.That(result.data[3].data, Is.EqualTo(new List<int> {0, 0, 0, 0, 1, 1, 1}));
        }
    }
}

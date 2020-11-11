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
    public class CumulativeFlowDiagramHelperTests
    {
        [Test]
        public async Task When_getting_cumulative_flow_diagram_data()
        {
            var historyEvents1 = new List<HistoryEvent>
            {
                new HistoryEvent
                {
                    Id = 1,
                    EventType = "Task created",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 8)),
                    TaskItemColumn = null,
                    TaskItemState = "Backlog",
                    Author = "Author1"
                },

                new HistoryEvent
                {
                    Id = 2,
                    EventType = "Task moved",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 12)),
                    TaskItemColumn = "Top Priority",
                    TaskItemState = "Top Priority",
                    Author = "Author2"
                },
                new HistoryEvent
                {
                    Id = 3,
                    EventType = "Task moved",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 14)),
                    TaskItemColumn = "In Process.Working",
                    TaskItemState = "In Process",
                    Author = "Author3"
                },
                new HistoryEvent
                {
                    Id = 3,
                    EventType = "Task moved",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 15)),
                    TaskItemColumn = "Released to Prod this week",
                    TaskItemState = "Released",
                    Author = "Author3"
                }
            };
            var historyEvents2 = new List<HistoryEvent>
            {
                new HistoryEvent
                {
                    Id = 1,
                    EventType = "Task created",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 9)),
                    TaskItemColumn = null,
                    TaskItemState = "Backlog",
                    Author = "Author1"
                },

                new HistoryEvent
                {
                    Id = 2,
                    EventType = "Task moved",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 13)),
                    TaskItemColumn = "Top Priority",
                    TaskItemState = "Top Priority",
                    Author = "Author2"
                },
                new HistoryEvent
                {
                    Id = 3,
                    EventType = "Task moved",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 15)),
                    TaskItemColumn = "In Process.Working",
                    TaskItemState = "In Process",
                    Author = "Author3"
                },
                new HistoryEvent
                {
                    Id = 3,
                    EventType = "Task moved",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 20)),
                    TaskItemColumn = "Released to Prod this week",
                    TaskItemState = "Released",
                    Author = "Author3"
                }
            };
            var historyEvents3 = new List<HistoryEvent>
            {
                new HistoryEvent
                {
                    Id = 1,
                    EventType = "Task created",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 15)),
                    TaskItemColumn = null,
                    TaskItemState = "Backlog",
                    Author = "Author1"
                },

                new HistoryEvent
                {
                    Id = 2,
                    EventType = "Task moved",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 20)),
                    TaskItemColumn = "Top Priority",
                    TaskItemState = "Top Priority",
                    Author = "Author2"
                },
                new HistoryEvent
                {
                    Id = 3,
                    EventType = "Task moved",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 22)),
                    TaskItemColumn = "In Process.Working",
                    TaskItemState = "In Process",
                    Author = "Author3"
                },
                new HistoryEvent
                {
                    Id = 3,
                    EventType = "Task moved",
                    EventDate = new DateTimeOffset(new DateTime(2020, 10, 23)),
                    TaskItemColumn = "Released to Prod this week",
                    TaskItemState = "Released",
                    Author = "Author3"
                }
            };
            var taskItems = new List<TaskItem>
            {
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 8))
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 9))
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 10, 15))
                }
            };

            var mockTaskItemRepository = new Mock<TaskItemRepository>();
            mockTaskItemRepository.SetupSequence(x => x.GetHistoryEventsByTaskIdAsync(It.IsAny<int>()))
                .ReturnsAsync(historyEvents1)
                .ReturnsAsync(historyEvents2)
                .ReturnsAsync(historyEvents3);
            mockTaskItemRepository.Setup(x => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(taskItems);

            var cumulativeFlowHelper = new CumulativeFlowDiagramHelper(mockTaskItemRepository.Object);
            var result = await cumulativeFlowHelper.GetCumulativeFlowData(new DateTimeOffset(new DateTime(2020, 10, 8), TimeSpan.Zero), new DateTimeOffset(new DateTime(2020, 10, 23), TimeSpan.Zero));

            Assert.That(result.data.Count, Is.EqualTo(4));

            Assert.That(result.data.ElementAt(0).data.Count, Is.EqualTo(12));
            Assert.That(result.data.ElementAt(0).data, Is.EqualTo(new List<int> {1, 2, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0}));

            Assert.That(result.data.ElementAt(1).data.Count, Is.EqualTo(12));
            Assert.That(result.data.ElementAt(1).data, Is.EqualTo(new List<int> {0, 0, 1, 2, 1, 0, 0, 0, 1, 1, 0, 0}));

            Assert.That(result.data.ElementAt(2).data.Count, Is.EqualTo(12));
            Assert.That(result.data.ElementAt(2).data, Is.EqualTo(new List<int> {0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0}));

            Assert.That(result.data.ElementAt(3).data.Count, Is.EqualTo(12));
            Assert.That(result.data.ElementAt(3).data, Is.EqualTo(new List<int> {0, 0, 0, 0, 0, 1, 1, 1, 2, 2, 2, 3}));
        }
    }
}

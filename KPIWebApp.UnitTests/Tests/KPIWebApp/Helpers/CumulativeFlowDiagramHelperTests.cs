using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Accord.Collections;
using DataAccess.DataRepositories;
using DataAccess.Deserialize.Kanbanize;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class CumulativeFlowDiagramHelperTests
    {
        private readonly OrderedDictionary<DateTime, List<int>> data = new OrderedDictionary<DateTime, List<int>>
        {
            {new DateTime(2020, 11, 9), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 10), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 11), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 12), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 13), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 14), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 15), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 16), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 17), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 18), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 19), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 20), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 21), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 22), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 23), new List<int> {0, 0, 0, 0}},
            {new DateTime(2020, 11, 24), new List<int> {0, 0, 0, 0}}
        };

        private CumulativeFlowDiagramHelper cumulativeFlowDiagramHelper = new CumulativeFlowDiagramHelper();

        [Test]
        public void When_formatting_data()
        {
            var result =
                cumulativeFlowDiagramHelper.FormatData(data, new DateTime(2020, 11, 7), new DateTime(2020, 11, 24));

            var backlog = new List<int>();
            var topPriority = new List<int>();
            var inProcess = new List<int>();
            var released = new List<int>();

            foreach (var datum in data)
            {
                backlog.Add(datum.Value[0]);
                topPriority.Add(datum.Value[1]);
                inProcess.Add(datum.Value[2]);
                released.Add(datum.Value[3]);
            }

            Assert.That(result.data[0].data, Is.EqualTo(backlog));
            Assert.That(result.data[1].data, Is.EqualTo(topPriority));
            Assert.That(result.data[2].data, Is.EqualTo(inProcess));
            Assert.That(result.data[3].data, Is.EqualTo(released));
        }

        [Test]
        public void When_processing_task_item_history()
        {
            var taskItem1 = new TaskItem
            {
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        Id = 1,
                        EventDate = new DateTimeOffset(new DateTime(2020, 11, 9)),
                        EventType = "Task created",
                        TaskItemColumn = BoardColumn.Backlog,
                        TaskItemState = TaskItemState.Backlog,
                        Author = "Author1",
                        TaskId = 1
                    },
                    new HistoryEvent
                    {
                        Id = 2,
                        EventDate = new DateTimeOffset(new DateTime(2020, 11, 13)),
                        EventType = "Task moved",
                        TaskItemColumn = BoardColumn.TopPriority,
                        TaskItemState = TaskItemState.TopPriority,
                        Author = "Author2",
                        TaskId = 1
                    },
                    new HistoryEvent
                    {
                        Id = 3,
                        EventDate = new DateTimeOffset(new DateTime(2020, 11, 17)),
                        EventType = "Task moved",
                        TaskItemColumn = BoardColumn.InProcessWorking,
                        TaskItemState = TaskItemState.InProcess,
                        Author = "Author3",
                        TaskId = 1
                    },
                    new HistoryEvent
                    {
                        Id = 4,
                        EventDate = new DateTimeOffset(new DateTime(2020, 11, 22)),
                        EventType = "Task moved",
                        TaskItemColumn = BoardColumn.ReadyToArchive,
                        TaskItemState = TaskItemState.Released,
                        Author = "Author4",
                        TaskId = 1
                    }
                }
            };

            var result = cumulativeFlowDiagramHelper.ProcessTaskItemHistory(taskItem1, data);

            Assert.That(result[taskItem1.HistoryEvents[0].EventDate.Date][0], Is.EqualTo(1));
            Assert.That(result[taskItem1.HistoryEvents[1].EventDate.Date][1], Is.EqualTo(1));
            Assert.That(result[taskItem1.HistoryEvents[1].EventDate.AddDays(-1).Date][1], Is.EqualTo(0));
            Assert.That(result[taskItem1.HistoryEvents[2].EventDate.Date][2], Is.EqualTo(1));
            Assert.That(result[taskItem1.HistoryEvents[2].EventDate.AddDays(-1).Date][2], Is.EqualTo(0));
            Assert.That(result[taskItem1.HistoryEvents[3].EventDate.Date][3], Is.EqualTo(1));
            Assert.That(result[taskItem1.HistoryEvents[3].EventDate.AddDays(-1).Date][3], Is.EqualTo(0));
        }

        [Test]
        public void When_updating_cumulative_flow_data()
        {
            var historyEvent = new HistoryEvent
            {
                Id = 1,
                EventDate = new DateTimeOffset(new DateTime(2020, 11, 20)),
                EventType = "Task moved",
                TaskItemColumn = BoardColumn.InProcessWorking,
                TaskItemState = TaskItemState.InProcess,
                Author = "Author1",
                TaskId = 1,
            };

            var result =
                cumulativeFlowDiagramHelper.UpdateCumulativeFlowData(historyEvent, new DateTime(2020, 11, 9), data);

            Assert.That(result, Is.EqualTo(historyEvent.EventDate.Date));
        }

        [Test]
        public async Task When_getting_cumulative_flow_data()
        {
            var taskItemList = new List<TaskItem>
            {
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 9)),
                    StartTime = new DateTimeOffset(new DateTime(2020, 11, 10)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 11, 12)),
                    Type = TaskItemType.Product,
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            Id = 1,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 9)),
                            EventType = "Task created",
                            TaskItemColumn = BoardColumn.Backlog,
                            TaskItemState = TaskItemState.Backlog,
                            Author = "Author1",
                            TaskId = 1
                        },
                        new HistoryEvent
                        {
                            Id = 2,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 10)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.TopPriority,
                            TaskItemState = TaskItemState.TopPriority,
                            Author = "Author2",
                            TaskId = 1
                        },
                        new HistoryEvent
                        {
                            Id = 3,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 11)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.InProcessWorking,
                            TaskItemState = TaskItemState.InProcess,
                            Author = "Author3",
                            TaskId = 1
                        },
                        new HistoryEvent
                        {
                            Id = 4,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 12)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.ReleasedToProdThisWeek,
                            TaskItemState = TaskItemState.Released,
                            Author = "Author4",
                            TaskId = 1
                        }
                    }
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 10)),
                    StartTime = new DateTimeOffset(new DateTime(2020, 11, 11)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 11, 13)),
                    Type = TaskItemType.Engineering,
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            Id = 5,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 10)),
                            EventType = "Task created",
                            TaskItemColumn = BoardColumn.Backlog,
                            TaskItemState = TaskItemState.Backlog,
                            Author = "Author5",
                            TaskId = 2
                        },
                        new HistoryEvent
                        {
                            Id = 6,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 11)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.TopPriority,
                            TaskItemState = TaskItemState.TopPriority,
                            Author = "Author6",
                            TaskId = 2
                        },
                        new HistoryEvent
                        {
                            Id = 7,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 12)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.InProcessWorking,
                            TaskItemState = TaskItemState.InProcess,
                            Author = "Author7",
                            TaskId = 2
                        },
                        new HistoryEvent
                        {
                            Id = 8,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 13)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.ReleasedToProdThisWeek,
                            TaskItemState = TaskItemState.Released,
                            Author = "Author8",
                            TaskId = 2
                        }
                    }
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 11)),
                    StartTime = new DateTimeOffset(new DateTime(2020, 11, 12)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 11, 16)),
                    Type = TaskItemType.Unanticipated,
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            Id = 9,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 11)),
                            EventType = "Task created",
                            TaskItemColumn = BoardColumn.Backlog,
                            TaskItemState = TaskItemState.Backlog,
                            Author = "Author9",
                            TaskId = 3
                        },
                        new HistoryEvent
                        {
                            Id = 10,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 12)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.TopPriority,
                            TaskItemState = TaskItemState.TopPriority,
                            Author = "Author0",
                            TaskId = 3
                        },
                        new HistoryEvent
                        {
                            Id = 11,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 13)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.InProcessWorking,
                            TaskItemState = TaskItemState.InProcess,
                            Author = "Author1",
                            TaskId = 3
                        },
                        new HistoryEvent
                        {
                            Id = 12,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 16)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.ReleasedToProdThisWeek,
                            TaskItemState = TaskItemState.Released,
                            Author = "Author2",
                            TaskId = 3
                        }
                    }
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 12)),
                    StartTime = new DateTimeOffset(new DateTime(2020, 11, 13)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 11, 17)),
                    Type = TaskItemType.Product,
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            Id = 13,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 12)),
                            EventType = "Task created",
                            TaskItemColumn = BoardColumn.Backlog,
                            TaskItemState = TaskItemState.Backlog,
                            Author = "Author3",
                            TaskId = 4
                        },
                        new HistoryEvent
                        {
                            Id = 14,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 13)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.TopPriority,
                            TaskItemState = TaskItemState.TopPriority,
                            Author = "Author4",
                            TaskId = 4
                        },
                        new HistoryEvent
                        {
                            Id = 15,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 16)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.InProcessWorking,
                            TaskItemState = TaskItemState.InProcess,
                            Author = "Author5",
                            TaskId = 4
                        },
                        new HistoryEvent
                        {
                            Id = 16,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 17)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.ReleasedToProdThisWeek,
                            TaskItemState = TaskItemState.Released,
                            Author = "Author6",
                            TaskId = 4
                        }
                    }
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 13)),
                    StartTime = new DateTimeOffset(new DateTime(2020, 11, 16)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 11, 18)),
                    Type = TaskItemType.Engineering,
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            Id = 17,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 13)),
                            EventType = "Task created",
                            TaskItemColumn = BoardColumn.Backlog,
                            TaskItemState = TaskItemState.Backlog,
                            Author = "Author7",
                            TaskId = 5
                        },
                        new HistoryEvent
                        {
                            Id = 18,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 16)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.TopPriority,
                            TaskItemState = TaskItemState.TopPriority,
                            Author = "Author8",
                            TaskId = 5
                        },
                        new HistoryEvent
                        {
                            Id = 19,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 17)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.InProcessWorking,
                            TaskItemState = TaskItemState.InProcess,
                            Author = "Author9",
                            TaskId = 5
                        },
                        new HistoryEvent
                        {
                            Id = 20,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 18)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.ReleasedToProdThisWeek,
                            TaskItemState = TaskItemState.Released,
                            Author = "Author0",
                            TaskId = 5
                        }
                    }
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 16)),
                    StartTime = new DateTimeOffset(new DateTime(2020, 11, 17)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 11, 19)),
                    Type = TaskItemType.Unanticipated,
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            Id = 21,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 16)),
                            EventType = "Task created",
                            TaskItemColumn = BoardColumn.Backlog,
                            TaskItemState = TaskItemState.Backlog,
                            Author = "Author1",
                            TaskId = 6
                        },
                        new HistoryEvent
                        {
                            Id = 22,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 17)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.TopPriority,
                            TaskItemState = TaskItemState.TopPriority,
                            Author = "Author2",
                            TaskId = 6
                        },
                        new HistoryEvent
                        {
                            Id = 23,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 18)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.InProcessWorking,
                            TaskItemState = TaskItemState.InProcess,
                            Author = "Author3",
                            TaskId = 6
                        },
                        new HistoryEvent
                        {
                            Id = 24,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 19)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.ReleasedToProdThisWeek,
                            TaskItemState = TaskItemState.Released,
                            Author = "Author4",
                            TaskId = 6
                        }
                    }
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 17)),
                    StartTime = new DateTimeOffset(new DateTime(2020, 11, 18)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 11, 20)),
                    Type = TaskItemType.Product,
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            Id = 25,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 17)),
                            EventType = "Task created",
                            TaskItemColumn = BoardColumn.Backlog,
                            TaskItemState = TaskItemState.Backlog,
                            Author = "Author5",
                            TaskId = 7
                        },
                        new HistoryEvent
                        {
                            Id = 26,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 18)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.TopPriority,
                            TaskItemState = TaskItemState.TopPriority,
                            Author = "Author6",
                            TaskId = 7
                        },
                        new HistoryEvent
                        {
                            Id = 27,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 19)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.InProcessWorking,
                            TaskItemState = TaskItemState.InProcess,
                            Author = "Author7",
                            TaskId = 7
                        },
                        new HistoryEvent
                        {
                            Id = 28,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 20)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.ReleasedToProdThisWeek,
                            TaskItemState = TaskItemState.Released,
                            Author = "Author8",
                            TaskId = 7
                        }
                    }
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 18)),
                    StartTime = new DateTimeOffset(new DateTime(2020, 11, 19)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 11, 23)),
                    Type = TaskItemType.Engineering,
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            Id = 29,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 18)),
                            EventType = "Task created",
                            TaskItemColumn = BoardColumn.Backlog,
                            TaskItemState = TaskItemState.Backlog,
                            Author = "Author9",
                            TaskId = 8
                        },
                        new HistoryEvent
                        {
                            Id = 30,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 19)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.TopPriority,
                            TaskItemState = TaskItemState.TopPriority,
                            Author = "Author0",
                            TaskId = 8
                        },
                        new HistoryEvent
                        {
                            Id = 31,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 20)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.InProcessWorking,
                            TaskItemState = TaskItemState.InProcess,
                            Author = "Author1",
                            TaskId = 8
                        },
                        new HistoryEvent
                        {
                            Id = 32,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 23)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.ReleasedToProdThisWeek,
                            TaskItemState = TaskItemState.Released,
                            Author = "Author2",
                            TaskId = 8
                        }
                    }
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 19)),
                    StartTime = new DateTimeOffset(new DateTime(2020, 11, 20)),
                    FinishTime = new DateTimeOffset(new DateTime(2020, 11, 24)),
                    Type = TaskItemType.Unanticipated,
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            Id = 33,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 19)),
                            EventType = "Task created",
                            TaskItemColumn = BoardColumn.Backlog,
                            TaskItemState = TaskItemState.Backlog,
                            Author = "Author3",
                            TaskId = 9
                        },
                        new HistoryEvent
                        {
                            Id = 34,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 20)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.TopPriority,
                            TaskItemState = TaskItemState.TopPriority,
                            Author = "Author4",
                            TaskId = 9
                        },
                        new HistoryEvent
                        {
                            Id = 35,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 23)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.InProcessWorking,
                            TaskItemState = TaskItemState.InProcess,
                            Author = "Author5",
                            TaskId = 9
                        },
                        new HistoryEvent
                        {
                            Id = 36,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 24)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.ReleasedToProdThisWeek,
                            TaskItemState = TaskItemState.Released,
                            Author = "Author6",
                            TaskId = 9
                        }
                    }
                },
                new TaskItem
                {
                    CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 20)),
                    StartTime = new DateTimeOffset(new DateTime(2020, 11, 23)),
                    Type = TaskItemType.Product,
                    FinishTime = null,
                    HistoryEvents = new List<HistoryEvent>
                    {
                        new HistoryEvent
                        {
                            Id = 37,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 20)),
                            EventType = "Task created",
                            TaskItemColumn = BoardColumn.Backlog,
                            TaskItemState = TaskItemState.Backlog,
                            Author = "Author7",
                            TaskId = 10
                        },
                        new HistoryEvent
                        {
                            Id = 38,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 23)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.TopPriority,
                            TaskItemState = TaskItemState.TopPriority,
                            Author = "Author8",
                            TaskId = 10
                        },
                        new HistoryEvent
                        {
                            Id = 39,
                            EventDate = new DateTimeOffset(new DateTime(2020, 11, 24)),
                            EventType = "Task moved",
                            TaskItemColumn = BoardColumn.InProcessWorking,
                            TaskItemState = TaskItemState.InProcess,
                            Author = "Author9",
                            TaskId = 10
                        }
                    }
                }
            };

            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository.Setup(x
                    => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(taskItemList);
            mockTaskItemRepository.SetupSequence(x => x.GetHistoryEventsByTaskIdAsync(It.IsAny<int>()))
                .ReturnsAsync(taskItemList[0].HistoryEvents)
                .ReturnsAsync(taskItemList[1].HistoryEvents)
                .ReturnsAsync(taskItemList[2].HistoryEvents)
                .ReturnsAsync(taskItemList[3].HistoryEvents)
                .ReturnsAsync(taskItemList[4].HistoryEvents)
                .ReturnsAsync(taskItemList[5].HistoryEvents)
                .ReturnsAsync(taskItemList[6].HistoryEvents)
                .ReturnsAsync(taskItemList[7].HistoryEvents)
                .ReturnsAsync(taskItemList[8].HistoryEvents)
                .ReturnsAsync(taskItemList[9].HistoryEvents);

            cumulativeFlowDiagramHelper = new CumulativeFlowDiagramHelper(mockTaskItemRepository.Object);

            var result = await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(new DateTime(2020, 11, 9),
                new DateTime(2020, 11, 24),
                true, true, true);

            foreach (var res in result.data)
            {
                foreach (var item in res.data)
                {
                    Console.Write(item + ", ");
                }

                Console.Write("\n");
            }

            Assert.That(result.data[0].data,
                Is.EqualTo(new List<int> {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0}));
            Assert.That(result.data[1].data,
                Is.EqualTo(new List<int> {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0}));
            Assert.That(result.data[2].data,
                Is.EqualTo(new List<int> {0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}));
            Assert.That(result.data[3].data,
                Is.EqualTo(new List<int> {0, 0, 0, 1, 2, 2, 2, 3, 4, 5, 6, 7, 7, 7, 8, 9}));
        }

        [Test]
        public async Task When_getting_cumulative_flow_data_and_task_has_multiple_history_items_from_the_same_state()
        {
            var taskItem = new TaskItem
            {
                CreatedOn = new DateTimeOffset(new DateTime(2020, 11, 9)),
                StartTime = new DateTimeOffset(new DateTime(2020, 11, 10)),
                FinishTime = new DateTimeOffset(new DateTime(2020, 11, 13)),
                Type = TaskItemType.Product,
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        Id = 1,
                        EventDate = new DateTimeOffset(new DateTime(2020, 11, 9)),
                        EventType = "Task created",
                        TaskItemColumn = BoardColumn.Backlog,
                        TaskItemState = TaskItemState.Backlog,
                        Author = "Author1",
                        TaskId = 1
                    },
                    new HistoryEvent
                    {
                        Id = 2,
                        EventDate = new DateTimeOffset(new DateTime(2020, 11, 10)),
                        EventType = "Task moved",
                        TaskItemColumn = BoardColumn.TopPriority,
                        TaskItemState = TaskItemState.TopPriority,
                        Author = "Author2",
                        TaskId = 1
                    },
                    new HistoryEvent
                    {
                        Id = 3,
                        EventDate = new DateTimeOffset(new DateTime(2020, 11, 11)),
                        EventType = "Task moved",
                        TaskItemColumn = BoardColumn.InProcessWorking,
                        TaskItemState = TaskItemState.InProcess,
                        Author = "Author3",
                        TaskId = 1
                    },
                    new HistoryEvent
                    {
                        Id = 4,
                        EventDate = new DateTimeOffset(new DateTime(2020, 11, 12)),
                        EventType = "Task moved",
                        TaskItemColumn = BoardColumn.InProcessReadyForProdDeploy,
                        TaskItemState = TaskItemState.InProcess,
                        Author = "Author4",
                        TaskId = 1
                    },
                    new HistoryEvent
                    {
                        Id = 5,
                        EventDate = new DateTimeOffset(new DateTime(2020, 11, 13)),
                        EventType = "Task moved",
                        TaskItemColumn = BoardColumn.ReleasedToProdThisWeek,
                        TaskItemState = TaskItemState.Released,
                        Author = "Author5",
                        TaskId = 1
                    }
                }
            };

            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository.Setup(x
                    => x.GetTaskItemListAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<TaskItem> {taskItem});
            mockTaskItemRepository.SetupSequence(x => x.GetHistoryEventsByTaskIdAsync(It.IsAny<int>()))
                .ReturnsAsync(taskItem.HistoryEvents);

            cumulativeFlowDiagramHelper = new CumulativeFlowDiagramHelper(mockTaskItemRepository.Object);

            var result = await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(new DateTime(2020, 11, 9),
                new DateTime(2020, 11, 24),
                true, true, true);

            foreach (var res in result.data)
            {
                foreach (var item in res.data)
                {
                    Console.Write(item + ", ");
                }

                Console.Write("\n");
            }

            Assert.That(result.data[0].data,
                Is.EqualTo(new List<int> {1, 0, 0, 0, 0}));
            Assert.That(result.data[1].data,
                Is.EqualTo(new List<int> {0, 1, 0, 0, 0}));
            Assert.That(result.data[2].data,
                Is.EqualTo(new List<int> {0, 0, 1, 1, 0}));
            Assert.That(result.data[3].data,
                Is.EqualTo(new List<int> {0, 0, 0, 0, 1}));
        }

        [Test]
        public async Task When_getting_cumulative_flow_data_and_task_item_moves_multiple_states_on_the_same_date()
        {
            var taskItem = new TaskItem
            {
                StartTime = DateTimeOffset.Now.Date.AddDays(-1),
                Type = TaskItemType.Engineering,
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        Author = "Author1",
                        EventDate = DateTimeOffset.Now.Date.AddDays(-1),
                        EventType = "Task created",
                        Id = 1,
                        TaskId = 1,
                        TaskItemColumn = BoardColumn.Backlog,
                        TaskItemState = TaskItemState.Backlog
                    },
                    new HistoryEvent
                    {
                        Author = "Author2",
                        EventDate = DateTimeOffset.Now.Date,
                        EventType = "Task moved",
                        Id = 2,
                        TaskId = 1,
                        TaskItemColumn = BoardColumn.TopPriority,
                        TaskItemState = TaskItemState.TopPriority
                    },
                    new HistoryEvent
                    {
                        Author = "Author3",
                        EventDate = DateTimeOffset.Now.Date,
                        EventType = "Task moved",
                        Id = 3,
                        TaskId = 1,
                        TaskItemColumn = BoardColumn.InProcessWorking,
                        TaskItemState = TaskItemState.InProcess
                    }
                }
            };

            var mockTaskItemRepository = new Mock<ITaskItemRepository>();
            mockTaskItemRepository.Setup(x =>
                    x.GetTaskItemListAsync(It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(new List<TaskItem> {taskItem});
            mockTaskItemRepository.Setup(x => x.GetHistoryEventsByTaskIdAsync(It.IsAny<int>()))
                .ReturnsAsync(taskItem.HistoryEvents);

            cumulativeFlowDiagramHelper = new CumulativeFlowDiagramHelper(mockTaskItemRepository.Object);

            var result = await cumulativeFlowDiagramHelper.GetCumulativeFlowDataAsync(DateTime.MinValue, DateTime.Now,
                    true, true, true);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.data[0].data, Is.EqualTo(new List<int>{1,0}));
            Assert.That(result.data[1].data, Is.EqualTo(new List<int>{0,1}));
            Assert.That(result.data[2].data, Is.EqualTo(new List<int>{0,0}));
            Assert.That(result.data[3].data, Is.EqualTo(new List<int>{0,0}));
        }
    }
}

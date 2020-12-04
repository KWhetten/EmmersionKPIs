using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Deserialize.Kanbanize;
using DataAccess.Objects;
using KPIDataExtractor.UnitTests.TestObjects.Kanbanize;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.DataManipulation.Deserializer
{
    public class KanbanizeHistoryEventDeserializerTests
    {
        [Test]
        public async Task When_deserializing_history_event_list()
        {
            var historyEventList = new List<JsonTaskItem>
            {
                new JsonTaskItem
                {
                    taskid = 1,
                    historydetails = new HistoryDetails
                    {
                        item = new List<JsonHistoryEvent>
                        {
                            new JsonHistoryEvent
                            {
                                historyid = 1,
                                author = "Author1",
                                details = "from 'ColumnName' to 'Top Priority'",
                                entrydate = DateTimeOffset.Now.Date.AddDays(-5),
                                historyevent = "Task moved",
                                taskid = 1
                            },
                            new JsonHistoryEvent
                            {
                                historyid = 2,
                                author = "Author2",
                                details = "from 'ColumnName' to 'Released to Prod this week'",
                                entrydate = DateTimeOffset.Now.Date.AddDays(-3),
                                historyevent = "Task moved",
                                taskid = 1
                            }
                        }
                    }
                }
            };
            var taskItem1 = new TaskItem
            {
                Id = 1
            };

            var taskItemDictionary = new Dictionary<int, TaskItem>
            {
                {
                    1, taskItem1
                }
            };

            var historyEventJson = JToken.Parse(JsonConvert.SerializeObject(historyEventList));

            var mockKanbanizeTaskItemDeserializer = new Mock<KanbanizeTaskItemDeserializer>();
            mockKanbanizeTaskItemDeserializer.Setup(x =>
                    x.FillInTaskItemStateDetailsAsync(It.IsAny<HistoryEvent>(), It.IsAny<TaskItem>()))
                .ReturnsAsync(taskItem1);

            var kanbanizeHistoryEventDeserializer = new KanbanizeHistoryEventDeserializer();
            var result =
                await kanbanizeHistoryEventDeserializer.DeserializeHistoryEventsAsync(historyEventJson,
                    taskItemDictionary);

            Assert.True(true);
            Assert.That(result[1].HistoryEvents.Count, Is.EqualTo(2));
            Assert.That(result[1].Id, Is.EqualTo(1));
            Assert.That(result[1].LastChangedBy, Is.EqualTo(historyEventList[0].historydetails.item[1].author));

            Assert.That(result[1].HistoryEvents[0].Author,
                Is.EqualTo(historyEventList[0].historydetails.item[0].author));
            Assert.That(result[1].HistoryEvents[0].EventDate,
                Is.EqualTo(historyEventList[0].historydetails.item[0].entrydate));
            Assert.That(result[1].HistoryEvents[0].EventType,
                Is.EqualTo(historyEventList[0].historydetails.item[0].historyevent));
            Assert.That(result[1].HistoryEvents[0].Id,
                Is.EqualTo(historyEventList[0].historydetails.item[0].historyid));
            Assert.That(result[1].HistoryEvents[0].TaskId,
                Is.EqualTo(historyEventList[0].historydetails.item[0].taskid));
            Assert.That(result[1].HistoryEvents[0].TaskItemColumn, Is.EqualTo(BoardColumn.TopPriority));
            Assert.That(result[1].HistoryEvents[0].TaskItemState, Is.EqualTo(TaskItemState.TopPriority));

            Assert.That(result[1].HistoryEvents[1].Author,
                Is.EqualTo(historyEventList[0].historydetails.item[1].author));
            Assert.That(result[1].HistoryEvents[1].EventDate,
                Is.EqualTo(historyEventList[0].historydetails.item[1].entrydate));
            Assert.That(result[1].HistoryEvents[1].EventType,
                Is.EqualTo(historyEventList[0].historydetails.item[1].historyevent));
            Assert.That(result[1].HistoryEvents[1].Id,
                Is.EqualTo(historyEventList[0].historydetails.item[1].historyid));
            Assert.That(result[1].HistoryEvents[1].TaskId,
                Is.EqualTo(historyEventList[0].historydetails.item[1].taskid));
            Assert.That(result[1].HistoryEvents[1].TaskItemColumn, Is.EqualTo(BoardColumn.ReleasedToProdThisWeek));
            Assert.That(result[1].HistoryEvents[1].TaskItemState, Is.EqualTo(TaskItemState.Released));
        }

        [Test]
        public async Task When_getting_history_events_and_there_is_an_invalid_event()
        {
            var historyEventList = new List<JsonTaskItem>
            {
                new JsonTaskItem
                {
                    taskid = 1,
                    historydetails = new HistoryDetails
                    {
                        item = new List<JsonHistoryEvent>
                        {
                            new JsonHistoryEvent
                            {
                                historyid = 1,
                                author = "Author1",
                                entrydate = DateTimeOffset.Now.Date.AddDays(-5),
                                historyevent = "Task moved",
                            },
                            new JsonHistoryEvent
                            {
                                historyid = 2,
                                details = "from 'ColumnName' to 'Released to Prod this week'",
                                entrydate = DateTimeOffset.Now.Date.AddDays(-3),
                                taskid = 1
                            }
                        }
                    }
                }
            };
            var taskItem1 = new TaskItem
            {
                Id = 1
            };

            var taskItemDictionary = new Dictionary<int, TaskItem>
            {
                {
                    1, taskItem1
                }
            };
            var historyEventJson = JToken.Parse(JsonConvert.SerializeObject(historyEventList));

            var kanbanizeHistoryEventDeserializer = new KanbanizeHistoryEventDeserializer();
            var result =
                await kanbanizeHistoryEventDeserializer.DeserializeHistoryEventsAsync(historyEventJson,
                    taskItemDictionary);

            Assert.That(result[1].HistoryEvents.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task When_deserializing_history_events_and_the_task_item_was_not_created_in_the_backlog()
        {
var historyEventList = new List<JsonTaskItem>
            {
                new JsonTaskItem
                {
                    taskid = 1,
                    historydetails = new HistoryDetails
                    {
                        item = new List<JsonHistoryEvent>
                        {
                            new JsonHistoryEvent
                            {
                                historyid = 1,
                                author = "Author1",
                                details = "from 'ColumnName' to 'In Process.Working'",
                                entrydate = DateTimeOffset.Now.Date.AddDays(-5),
                                historyevent = "Task moved",
                                taskid = 1
                            },
                            new JsonHistoryEvent
                            {
                                historyid = 2,
                                author = "Author2",
                                details = "from 'ColumnName' to 'Released to Prod this week'",
                                entrydate = DateTimeOffset.Now.Date.AddDays(-3),
                                historyevent = "Task moved",
                                taskid = 1
                            }
                        }
                    }
                }
            };
            var taskItem1 = new TaskItem
            {
                Id = 1,
                CreatedOn = DateTimeOffset.Now.Date.AddDays(-5),
                CurrentBoardColumn = BoardColumn.InProcessWorking
            };

            var taskItemDictionary = new Dictionary<int, TaskItem>
            {
                {
                    1, taskItem1
                }
            };

            var historyEventJson = JToken.Parse(JsonConvert.SerializeObject(historyEventList));

            var mockKanbanizeTaskItemDeserializer = new Mock<KanbanizeTaskItemDeserializer>();
            mockKanbanizeTaskItemDeserializer.Setup(x =>
                    x.FillInTaskItemStateDetailsAsync(It.IsAny<HistoryEvent>(), It.IsAny<TaskItem>()))
                .ReturnsAsync(taskItem1);

            var kanbanizeHistoryEventDeserializer = new KanbanizeHistoryEventDeserializer();
            var result =
                await kanbanizeHistoryEventDeserializer.DeserializeHistoryEventsAsync(historyEventJson,
                    taskItemDictionary);

            Assert.That(result[1].CreatedOn, Is.EqualTo(historyEventList[0].historydetails.item[0].entrydate));
            Assert.That(result[1].HistoryEvents.Count, Is.EqualTo(2));
            Assert.That(result[1].Id, Is.EqualTo(1));
            Assert.That(result[1].LastChangedBy, Is.EqualTo(historyEventList[0].historydetails.item[1].author));

            Assert.That(result[1].HistoryEvents[0].Author,
                Is.EqualTo(historyEventList[0].historydetails.item[0].author));
            Assert.That(result[1].HistoryEvents[0].EventDate,
                Is.EqualTo(historyEventList[0].historydetails.item[0].entrydate));
            Assert.That(result[1].HistoryEvents[0].EventType,
                Is.EqualTo(historyEventList[0].historydetails.item[0].historyevent));
            Assert.That(result[1].HistoryEvents[0].Id,
                Is.EqualTo(historyEventList[0].historydetails.item[0].historyid));
            Assert.That(result[1].HistoryEvents[0].TaskId,
                Is.EqualTo(historyEventList[0].historydetails.item[0].taskid));
            Assert.That(result[1].HistoryEvents[0].TaskItemColumn, Is.EqualTo(BoardColumn.InProcessWorking));
            Assert.That(result[1].HistoryEvents[0].TaskItemState, Is.EqualTo(TaskItemState.InProcess));

            Assert.That(result[1].HistoryEvents[1].Author,
                Is.EqualTo(historyEventList[0].historydetails.item[1].author));
            Assert.That(result[1].HistoryEvents[1].EventDate,
                Is.EqualTo(historyEventList[0].historydetails.item[1].entrydate));
            Assert.That(result[1].HistoryEvents[1].EventType,
                Is.EqualTo(historyEventList[0].historydetails.item[1].historyevent));
            Assert.That(result[1].HistoryEvents[1].Id,
                Is.EqualTo(historyEventList[0].historydetails.item[1].historyid));
            Assert.That(result[1].HistoryEvents[1].TaskId,
                Is.EqualTo(historyEventList[0].historydetails.item[1].taskid));
            Assert.That(result[1].HistoryEvents[1].TaskItemColumn, Is.EqualTo(BoardColumn.ReleasedToProdThisWeek));
            Assert.That(result[1].HistoryEvents[1].TaskItemState, Is.EqualTo(TaskItemState.Released));
        }
    }
}

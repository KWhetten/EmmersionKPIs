using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Deserialize.Kanbanize;
using DataAccess.Objects;
using KPIDataExtractor.UnitTests.TestObjects.Kanbanize;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.DataManipulation.Deserializer
{
    public class KanbanizeTaskItemDeserializerTests
    {
        [Test]
        public async Task When_deserializing_task_item_list()
        {
            var taskItemList = new List<JsonTaskItem>
            {
                new JsonTaskItem
                {
                    taskid = 1,
                    title = "Task1",
                    type = "Engineering",
                    columnname = "In Process.Working On"
                },
                new JsonTaskItem
                {
                    taskid = 2,
                    title = "Task2",
                    type = "Product",
                    columnname = "Released to Prod this Week"
                }
            };
            var historyItemList = new Dictionary<int, TaskItem>
            {
                {
                    1, new TaskItem
                    {
                        Id = 1,
                        Title = "Task1",
                        Type = TaskItemType.Engineering,
                        CurrentBoardColumn = BoardColumn.InProcessWorking,
                        HistoryEvents = new List<HistoryEvent>()
                    }
                },
                {
                    2, new TaskItem
                    {
                        Id = 2,
                        Title = "Task2",
                        Type = TaskItemType.Product,
                        CurrentBoardColumn = BoardColumn.ReleasedToProdThisWeek,
                        HistoryEvents = new List<HistoryEvent>()
                    }
                }
            };

            var taskItemListJson = JToken.Parse(JsonConvert.SerializeObject(taskItemList));

            var mockKanbanizeHistoryEvenDeserializer = new Mock<KanbanizeHistoryEventDeserializer>();
            mockKanbanizeHistoryEvenDeserializer.Setup(x =>
                    x.DeserializeHistoryEventsAsync(It.IsAny<JToken>(), It.IsAny<Dictionary<int, TaskItem>>()))
                .ReturnsAsync(historyItemList);

            var kanbanizeTaskItemDeserializer =
                new KanbanizeTaskItemDeserializer(mockKanbanizeHistoryEvenDeserializer.Object);
            var result = await kanbanizeTaskItemDeserializer.DeserializeTaskItemListAsync(taskItemListJson, 4);

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Task1"));
            Assert.That(result[0].Type, Is.EqualTo(TaskItemType.Engineering));
            Assert.That(result[0].CurrentBoardColumn, Is.EqualTo(BoardColumn.InProcessWorking));

            Assert.That(result[1].Id, Is.EqualTo(2));
            Assert.That(result[1].Title, Is.EqualTo("Task2"));
            Assert.That(result[1].Type, Is.EqualTo(TaskItemType.Product));
            Assert.That(result[1].CurrentBoardColumn, Is.EqualTo(BoardColumn.ReleasedToProdThisWeek));
        }

        [Test]
        public async Task When_filling_in_task_item_history_details()
        {
            var historyEventList = new List<HistoryEvent>
            {
                new HistoryEvent
                {
                    Id = 1,
                    EventType = "Task created",
                    EventDate = DateTimeOffset.Now.Date.AddDays(-5),
                    Author = "Author1"
                },
                new HistoryEvent
                {
                    Id = 2,
                    EventType = "Task moved",
                    TaskItemState = TaskItemState.TopPriority,
                    EventDate = DateTimeOffset.Now.Date.AddDays(-4),
                    Author = "Author2"
                },
                new HistoryEvent
                {
                    Id = 3,
                    EventType = "Task moved",
                    TaskItemState = TaskItemState.InProcess,
                    EventDate = DateTimeOffset.Now.Date.AddDays(-3),
                    Author = "Author3"
                },
                new HistoryEvent
                {
                    Id = 4,
                    EventType = "Task moved",
                    TaskItemState = TaskItemState.Released,
                    EventDate = DateTimeOffset.Now.Date.AddDays(-2),
                    Author = "Author4"
                }
            };
            var taskItem = new TaskItem();

            var kanbanizeTaskItemDeserializer = new KanbanizeTaskItemDeserializer();
            var result = new TaskItem();

            var mockReleaseRepository = new Mock<ReleaseRepository>();
            mockReleaseRepository.Setup(x => x.GetFirstReleaseBeforeDateAsync(It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync((Release)null);

            foreach (var historyEvent in historyEventList)
            {
                result = await kanbanizeTaskItemDeserializer.FillInTaskItemStateDetailsAsync(historyEvent, taskItem);
            }

            Assert.That(result.CreatedBy, Is.EqualTo(historyEventList[0].Author));
            Assert.That(result.CreatedOn, Is.EqualTo(historyEventList[0].EventDate));
            Assert.That(result.StartTime, Is.EqualTo(historyEventList[1].EventDate));
            Assert.That(result.FinishTime, Is.EqualTo(historyEventList[3].EventDate));
        }

        [Test]
        public void When_getting_task_item_state()
        {
            var kanbanizeTaskItemDeserializer = new KanbanizeTaskItemDeserializer();
            var result = kanbanizeTaskItemDeserializer.GetCardType("Unanticipated");

            Assert.That(result, Is.EqualTo(TaskItemType.Unanticipated));

            result = kanbanizeTaskItemDeserializer.GetCardType("Product");

            Assert.That(result, Is.EqualTo(TaskItemType.Product));

            result = kanbanizeTaskItemDeserializer.GetCardType("Engineering");

            Assert.That(result, Is.EqualTo(TaskItemType.Engineering));
        }
    }
}

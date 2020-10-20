using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.ApiWrapper;
using DataAccess.DataRepositories;
using DataAccess.Deserialize;
using DataAccess.Objects;
using KPIDataExtractor.UnitTests.TestObjects.Kanbanize;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.DataManipulation.Deserializer
{
    [TestFixture]
    public class KanbanizeDeserializerTests
    {
        [Test]
        public async Task When_deserializing_work_item_card_list()
        {
            var releaseStartTime = DateTime.Now.AddHours(-2);
            var releaseFinishTime = DateTime.Now;
            var startTime = DateTime.Now.AddHours(-5);
            var finishTime = DateTime.Now.AddHours(-3);
            var taskItem1 = new JsonTaskItem
            {
                taskid = 1,
                title = "TaskItem1",
                type = "Product",
                createdat = DateTime.Now.AddHours(-3),
                reporter = "CreatedBy1",
                updatedat = DateTime.Now.AddHours(-1),
                columnid = "active",
                columnname = "Ready for Prod Deploy",
                priority = "High",
                comments = new[]
                {
                    "Comment1",
                    "Comment2",
                    "Comment3",
                    "Comment4",
                    "Comment5",
                    "Comment6",
                    "Comment7",
                    "Comment8",
                    "Comment9",
                    "Comment10"
                },
                links = new JsonTaskItemLinks
                {
                    child = 1
                }
            };
            var taskItem2 = new JsonTaskItem
            {
                taskid = 2,
                title = "TaskItem2",
                type = "Engineering",
                createdat = DateTime.Now.AddHours(-2),
                reporter = "CreatedBy2",
                updatedat = DateTime.Now.AddHours(-2),
                columnid = "working",
                columnname = "Working",
                priority = "Low",
                comments = new[]
                {
                    "Comment1",
                    "Comment2"
                },
                links = new JsonTaskItemLinks
                {
                    child = 0
                }
            };
            var taskItem3 = new JsonTaskItem
            {
                taskid = 3,
                title = "Task 3",
                type = "Unanticipated",
                createdat = DateTime.Now.AddHours(-1),
                reporter = "CreatedBy Name",
                updatedat = DateTime.Now,
                columnid = "archive",
                columnname = "Ready to Archive",
                priority = "None",
                comments = new[]
                {
                    "Comment1",
                    "Comment2",
                    "Comment3",
                    "Comment4",
                    "Comment5",
                    "Comment6",
                    "Comment7",
                    "Comment8",
                    "Comment9",
                    "Comment10"
                },
                links = new JsonTaskItemLinks
                {
                    child = 0
                }
            };

            var history1 = new JsonTaskItemHistory()
            {
                historyevent = "Task moved",
                details = "to 'Top Priority'",
                entrydate = startTime,
                author = "ChangedBy1"
            };
            var history2 = new JsonTaskItemHistory()
            {
                historyevent = "",
                details = "",
                entrydate = DateTime.Now.AddHours(-4),
                author = "ChangedBy1"
            };
            var history3 = new JsonTaskItemHistory()
            {
                historyevent = "Task moved",
                details = "to 'Released to Prod this week'",
                entrydate = finishTime,
                author = "ChangedBy1"
            };

            var taskItem1JToken = JToken.Parse(JsonConvert.SerializeObject(taskItem1));
            var taskItem2JToken = JToken.Parse(JsonConvert.SerializeObject(taskItem2));
            var taskItem3JToken = JToken.Parse(JsonConvert.SerializeObject(taskItem3));
            var jsonTaskItemList = new JArray
            {
                taskItem1JToken,
                taskItem2JToken,
                taskItem3JToken
            };

            var history1JToken = JToken.Parse(JsonConvert.SerializeObject(history1));
            var history2JToken = JToken.Parse(JsonConvert.SerializeObject(history2));
            var history3JToken = JToken.Parse(JsonConvert.SerializeObject(history3));

            var historyArray = new JArray
            {
                history1JToken,
                history2JToken,
                history3JToken
            };

            var mockKanbanizeApiWrapper = new Mock<IKanbanizeApiRepository>();
            mockKanbanizeApiWrapper.Setup(x => x.GetTaskItemHistory(It.IsAny<JToken>(), It.IsAny<int>()))
                .Returns(historyArray);
            var mockReleaseDataAccess = new Mock<IReleaseRepository>();
            mockReleaseDataAccess.Setup(x => x.GetFirstReleaseBeforeDateAsync(It.IsAny<DateTime>())).ReturnsAsync(
                    new Release
                    {
                        Id = 1,
                        Attempts = 3,
                        FinishTime = releaseFinishTime,
                        Name = "Release1",
                        ReleaseEnvironment = new ReleaseEnvironment
                        {
                            Id = 1,
                            Name = "ReleaseEnvironment1"
                        },
                        StartTime = releaseStartTime,
                        Status = "succeeded"
                    });

            var deserializer = new KanbanizeDeserializer(mockKanbanizeApiWrapper.Object);
            var results = await deserializer.TaskItemListAsync(jsonTaskItemList, 4);
            var result = results.ToList();

            Assert.That(result.ElementAt(0).Id, Is.EqualTo(1));
            Assert.That(result.ElementAt(0).Impact, Is.EqualTo("High"));
            Assert.That(result.ElementAt(0).Title, Is.EqualTo("TaskItem1"));
            Assert.That(result.ElementAt(0).Type, Is.EqualTo(TaskItemType.Product));
            Assert.That(result.ElementAt(0).CardState, Is.EqualTo("Resolved"));
            Assert.That(result.ElementAt(0).CommentCount, Is.EqualTo(10));
            Assert.That(result.ElementAt(0).CreatedBy, Is.EqualTo("CreatedBy1"));
            Assert.That(result.ElementAt(0).FinishTime, Is.EqualTo(finishTime.ToUniversalTime()));
            Assert.That(result.ElementAt(0).NumRevisions, Is.EqualTo(3));
            Assert.That(result.ElementAt(0).StartTime, Is.EqualTo(startTime.ToUniversalTime()));
            Assert.That(result.ElementAt(0).DevelopmentTeamName, Is.EqualTo("Enterprise Team"));
            Assert.That(result.ElementAt(0).CurrentBoardColumn, Is.EqualTo("Ready for Prod Deploy"));
            Assert.That(result.ElementAt(0).LastChangedBy, Is.EqualTo("ChangedBy1"));

            Assert.That(result.ElementAt(1).Id, Is.EqualTo(2));
            Assert.That(result.ElementAt(1).Impact, Is.EqualTo("Low"));
            Assert.That(result.ElementAt(1).Title, Is.EqualTo("TaskItem2"));
            Assert.That(result.ElementAt(1).Type, Is.EqualTo(TaskItemType.Engineering));
            Assert.That(result.ElementAt(1).CardState, Is.EqualTo("Active"));
            Assert.That(result.ElementAt(1).CommentCount, Is.EqualTo(2));
            Assert.That(result.ElementAt(1).CreatedBy, Is.EqualTo("CreatedBy2"));
            Assert.That(result.ElementAt(1).FinishTime, Is.EqualTo(finishTime.ToUniversalTime()));
            Assert.That(result.ElementAt(1).NumRevisions, Is.EqualTo(3));
            Assert.That(result.ElementAt(1).StartTime, Is.EqualTo(startTime.ToUniversalTime()));
            Assert.That(result.ElementAt(1).DevelopmentTeamName, Is.EqualTo("Enterprise Team"));
            Assert.That(result.ElementAt(1).CurrentBoardColumn, Is.EqualTo("Working"));
            Assert.That(result.ElementAt(1).LastChangedBy, Is.EqualTo("ChangedBy1"));

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task When_work_item_card_is_created_in_active_column()
        {
            var createdAt = DateTime.Now.AddHours(-5);
            var taskItem1 = new JsonTaskItem
            {
                taskid = 1,
                title = "TaskItem1",
                type = "Product",
                createdat = createdAt,
                reporter = "CreatedBy1",
                updatedat = DateTime.Now.AddHours(-1),
                columnid = "active",
                columnname = "Ready for Prod Deploy",
                priority = "High",
                comments = new[]
                {
                    "Comment1",
                    "Comment2",
                    "Comment3",
                    "Comment4",
                    "Comment5",
                    "Comment6",
                    "Comment7",
                    "Comment8",
                    "Comment9",
                    "Comment10"
                },
                links = new JsonTaskItemLinks
                {
                    child = 1
                }
            };
            var taskItem1JToken = JToken.Parse(JsonConvert.SerializeObject(taskItem1));
            var jsonTaskItemList = new JArray
            {
                taskItem1JToken
            };
            var mockKanbanizeApiWrapper = new Mock<IKanbanizeApiRepository>();
            mockKanbanizeApiWrapper.Setup(x => x.GetTaskItemHistory(It.IsAny<JToken>(), It.IsAny<int>()))
                .Returns("");
            var mockReleaseDataAccess = new Mock<IReleaseRepository>();
            mockReleaseDataAccess.Setup(x => x.GetFirstReleaseBeforeDateAsync(It.IsAny<DateTime>())).ReturnsAsync(
                    new Release
                    {
                        Id = 2,
                        Attempts = 5,
                        FinishTime = DateTime.Now.AddHours(-1),
                        Name = "Release2",
                        ReleaseEnvironment = new ReleaseEnvironment
                        {
                            Id = 2,
                            Name = "ReleaseEnvironment2"
                        },
                        StartTime = DateTime.Now.AddHours(-3),
                        Status = "failed"
                    });

            var deserializer = new KanbanizeDeserializer(mockKanbanizeApiWrapper.Object);
            var results = await deserializer.TaskItemListAsync(jsonTaskItemList, 5);
            var result = results.ToList();

            Assert.That(result.ElementAt(0).Id, Is.EqualTo(1));
            Assert.That(result.ElementAt(0).Impact, Is.EqualTo("High"));
            Assert.That(result.ElementAt(0).Title, Is.EqualTo("TaskItem1"));
            Assert.That(result.ElementAt(0).Type, Is.EqualTo(TaskItemType.Product));
            Assert.That(result.ElementAt(0).CardState, Is.EqualTo("Resolved"));
            Assert.That(result.ElementAt(0).CommentCount, Is.EqualTo(10));
            Assert.That(result.ElementAt(0).CreatedBy, Is.EqualTo("CreatedBy1"));
            Assert.That(result.ElementAt(0).FinishTime, Is.EqualTo(createdAt.ToUniversalTime()));
            Assert.That(result.ElementAt(0).NumRevisions, Is.EqualTo(0));
            Assert.That(result.ElementAt(0).StartTime, Is.EqualTo(createdAt.ToUniversalTime()));
            Assert.That(result.ElementAt(0).DevelopmentTeamName, Is.EqualTo("Assessment Team"));
            Assert.That(result.ElementAt(0).CurrentBoardColumn, Is.EqualTo("Ready for Prod Deploy"));
        }
    }
}

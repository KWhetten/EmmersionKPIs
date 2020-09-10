using System;
using System.Collections.Generic;
using System.Linq;
using DataWrapper.ApiWrapper;
using DataWrapper.DatabaseAccess;
using DataWrapper.Deserialize;
using DataObjects;
using KPIDataExtractor.UnitTests.Objects.DevOps;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Deserializer
{
    [TestFixture]
    public class DevOpsDeserializerTests
    {
        [Test]
        public void When_deserializing_releases()
        {
            var startTime = DateTime.Now.AddHours(-2);
            var finishTime = DateTime.Now.AddHours(-1);

            var release1 = new JsonRelease
            {
                id = 1,
                release = new JsonRelease2
                {
                    name = "JsonRelease1"
                },
                deploymentStatus = "succeeded",
                definitionEnvironmentId = 1,
                releaseEnvironment = new JsonReleaseEnvironment
                {
                    name = "JsonReleaseEnvironment1"
                },
                attempt = 3,
                startedOn = startTime,
                completedOn = DateTime.Now
            };
            var release2 = new JsonRelease
            {
                id = 2,
                release = new JsonRelease2
                {
                    name = "JsonRelease2"
                },
                deploymentStatus = "failed",
                definitionEnvironmentId = 2,
                releaseEnvironment = new JsonReleaseEnvironment
                {
                    name = "JsonReleaseEnvironment2"
                },
                attempt = 5,
                startedOn = DateTime.Now.AddHours(-3),
                completedOn = finishTime
            };

            var release1JToken = JToken.Parse(JsonConvert.SerializeObject(release1));
            var release2JToken = JToken.Parse(JsonConvert.SerializeObject(release2));

            var jsonReleases = new JArray
            {
                release1JToken,
                release2JToken
            };

            var deserializer = new DevOpsDeserializer(null, null);
            var result = deserializer.Releases(jsonReleases);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[1].Id, Is.EqualTo(2));
            Assert.That(result[0].Attempts, Is.EqualTo(3));
            Assert.That(result[1].FinishTime, Is.EqualTo(finishTime));
            Assert.That(result[0].Name, Is.EqualTo("JsonRelease1"));
            Assert.That(result[1].ReleaseEnvironment.Id, Is.EqualTo(2));
            Assert.That(result[0].ReleaseEnvironment.Name, Is.EqualTo("JsonReleaseEnvironment1"));
            Assert.That(result[0].StartTime, Is.EqualTo(startTime));
            Assert.That(result[1].Status, Is.EqualTo("failed"));
        }

        [Test]
        public void When_deserializing_work_item_card_list()
        {
            var createdDate1 = DateTime.Now.AddDays(-3);
            var changedDate1 = DateTime.Now.AddHours(-3);
            var createdDate2 = DateTime.Now.AddDays(-2);
            var changedDate2 = DateTime.Now.AddHours(-2);
            var releaseStartTime = DateTime.Now.AddHours(-2);
            var releaseFinishTime = DateTime.Now;

            var release = new Release
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
            };

            var mockDevOpsApiWrapper = new Mock<IDevOpsApiWrapper>();
            mockDevOpsApiWrapper.Setup(x => x.GetWorkItemUpdates(It.IsAny<WorkItemCard>())).Returns(JObject.Parse(
                JsonConvert.SerializeObject(new JsonWorkItemCardUpdate
                {
                    fields = new JsonWorkItemCardUpdateFields
                    {
                        SysBoardColumn = new JsonWorkItemCardBoardColumn
                        {
                            oldValue = "Backlog"
                        },
                        SysChangedDate = new JsonWorkItemCardChangedDate
                        {
                            newValue = DateTime.Now
                        }
                    }
                }).Replace("Sys", "System.")));

            var mockDataAccess = new Mock<IDataAccess>();
            mockDataAccess.Setup(x => x.GetReleasesBeforeDate(It.IsAny<DateTime>())).Returns(new List<Release>
            {
                release,
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
                }
            });

            var jsonWorkItem1 = JObject.Parse(JsonConvert.SerializeObject(new JsonWorkItemCard
                {
                    id = 1,
                    fields = new JsonWorkItemCardFields
                    {
                        SysTitle = "WorkItemCard1",
                        SysWorkItemType = "Strategic Product Work",
                        SysBoardLane = "Assessment Team",
                        SysCreatedDate = createdDate1,
                        SysCreatedBy = new JsonWorkItemCardCreatedBy
                        {
                            displayName = "CreatedBy1"
                        },
                        SysChangedDate = changedDate1,
                        SysChangedBy = new JsonWorkItemCardChangedBy
                        {
                            displayName = "ChangedBy1"
                        },
                        SysBoardColumn = "In Production",
                        SysState = "Resolved",
                        CusImpact = "High",
                        SysCommentCount = 23
                    },
                    rev = 3
                }).Replace("Sys", "System.")
                .Replace("Cus", "Custom."));
            var jsonWorkItem2 = JObject.Parse(JsonConvert.SerializeObject(new JsonWorkItemCard
                {
                    id = 2,
                    fields = new JsonWorkItemCardFields
                    {
                        SysTitle = "WorkItemCard2",
                        SysWorkItemType = "Strategic Engineering Work",
                        SysBoardLane = "Enterprise Team",
                        SysCreatedDate = createdDate2,
                        SysCreatedBy = new JsonWorkItemCardCreatedBy
                        {
                            displayName = "CreatedBy2"
                        },
                        SysChangedDate = changedDate2,
                        SysChangedBy = new JsonWorkItemCardChangedBy
                        {
                            displayName = "ChangedBy2"
                        },
                        SysBoardColumn = "Working On",
                        SysState = "Active",
                        CusImpact = "Low",
                        SysCommentCount = 2
                    },
                    rev = 2
                }).Replace("Sys", "System.")
                .Replace("Cus", "Custom."));
            var jsonWorkItem3 = JObject.Parse(JsonConvert.SerializeObject(new JsonWorkItemCard
                {
                    id = 3,
                    fields = new JsonWorkItemCardFields
                    {
                        SysTitle = "WorkItemCard3",
                        SysWorkItemType = "Tactical Product Work",
                        SysBoardLane = "The Bad Team",
                        SysCreatedDate = createdDate2,
                        SysCreatedBy = new JsonWorkItemCardCreatedBy
                        {
                            displayName = "CreatedBy1"
                        },
                        SysChangedDate = changedDate1,
                        SysChangedBy = new JsonWorkItemCardChangedBy
                        {
                            displayName = "ChangedBy2"
                        },
                        SysBoardColumn = "Done",
                        SysState = "Closed",
                        CusImpact = "None",
                        SysCommentCount = 5687412
                    },
                    rev = 256
                }).Replace("Sys", "System.")
                .Replace("Cus", "Custom."));
            var jsonWorkItemList = new JArray
            {
                jsonWorkItem1,
                jsonWorkItem2,
                jsonWorkItem3
            };

            var deserializer = new DevOpsDeserializer(mockDevOpsApiWrapper.Object, mockDataAccess.Object);
            var result = deserializer.WorkItemCardList(jsonWorkItemList).ToList();

            Assert.That(result.ElementAt(0).Id, Is.EqualTo(1));
            Assert.That(result.ElementAt(0).Impact, Is.EqualTo("High"));
            Assert.That(result.ElementAt(0).Release, Is.EqualTo(release));
            Assert.That(result.ElementAt(0).Title, Is.EqualTo("WorkItemCard1"));
            Assert.That(result.ElementAt(0).Type, Is.EqualTo(WorkItemCardType.StrategicProduct));
            Assert.That(result.ElementAt(0).CardState, Is.EqualTo("Resolved"));
            Assert.That(result.ElementAt(0).CommentCount, Is.EqualTo(23));
            Assert.That(result.ElementAt(0).CreatedBy, Is.EqualTo("CreatedBy1"));
            Assert.That(result.ElementAt(0).CreatedOn, Is.EqualTo(createdDate1));
            Assert.That(result.ElementAt(0).FinishTime, Is.EqualTo(DateTime.MaxValue));
            Assert.That(result.ElementAt(0).NumRevisions, Is.EqualTo(3));
            Assert.That(result.ElementAt(0).StartTime, Is.EqualTo(DateTime.MinValue));
            Assert.That(result.ElementAt(0).DevelopmentTeamName, Is.EqualTo("Assessment Team"));
            Assert.That(result.ElementAt(0).CurrentBoardColumn, Is.EqualTo("In Production"));
            Assert.That(result.ElementAt(0).LastChangedBy, Is.EqualTo("ChangedBy1"));
            Assert.That(result.ElementAt(0).LastChangedOn, Is.EqualTo(changedDate1));

            Assert.That(result.ElementAt(1).Id, Is.EqualTo(2));
            Assert.That(result.ElementAt(1).Impact, Is.EqualTo("Low"));
            Assert.That(result.ElementAt(1).Title, Is.EqualTo("WorkItemCard2"));
            Assert.That(result.ElementAt(1).Type, Is.EqualTo(WorkItemCardType.StrategicEngineering));
            Assert.That(result.ElementAt(1).CardState, Is.EqualTo("Active"));
            Assert.That(result.ElementAt(1).CommentCount, Is.EqualTo(2));
            Assert.That(result.ElementAt(1).CreatedBy, Is.EqualTo("CreatedBy2"));
            Assert.That(result.ElementAt(1).CreatedOn, Is.EqualTo(createdDate2));
            Assert.That(result.ElementAt(1).FinishTime, Is.EqualTo(DateTime.MaxValue));
            Assert.That(result.ElementAt(1).NumRevisions, Is.EqualTo(2));
            Assert.That(result.ElementAt(1).StartTime, Is.EqualTo(DateTime.MinValue));
            Assert.That(result.ElementAt(1).DevelopmentTeamName, Is.EqualTo("Enterprise Team"));
            Assert.That(result.ElementAt(1).CurrentBoardColumn, Is.EqualTo("Working On"));
            Assert.That(result.ElementAt(1).LastChangedBy, Is.EqualTo("ChangedBy2"));
            Assert.That(result.ElementAt(1).LastChangedOn, Is.EqualTo(changedDate2));

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void When_getting_a_start_time_for_work_item_card()
        {
            var changedDate = DateTime.Now.AddHours(-1);

            var workItemUpdateList = new List<JsonWorkItemCardUpdate>
            {
                new JsonWorkItemCardUpdate
                {
                    fields = new JsonWorkItemCardUpdateFields
                    {
                        SysBoardColumn = new JsonWorkItemCardBoardColumn
                        {
                            oldValue = "Parking Lot"
                        },
                        SysChangedDate = new JsonWorkItemCardChangedDate
                        {
                            newValue = changedDate
                        }
                    }
                }
            };

            var jsonWorkItemUpdates = JToken.Parse(JsonConvert.SerializeObject(workItemUpdateList)
                .Replace("Sys", "System."));

            var deserializer = new DevOpsDeserializer(null, null);
            var result =
                deserializer.JsonWorkItemStartTime(jsonWorkItemUpdates);

            Assert.That(result, Is.EqualTo(changedDate));
        }

        [Test]
        public void When_getting_a_start_time_for_work_item_card_that_has_not_started()
        {
            var changedDate = DateTime.Now.AddHours(-1);

            var workItemUpdateList = new List<JsonWorkItemCardUpdate>
            {
                new JsonWorkItemCardUpdate
                {
                    fields = new JsonWorkItemCardUpdateFields
                    {
                        SysBoardColumn = new JsonWorkItemCardBoardColumn
                        {
                            oldValue = ""
                        },
                        SysChangedDate = new JsonWorkItemCardChangedDate
                        {
                            newValue = changedDate
                        }
                    }
                }
            };

            var jsonWorkItemUpdates = JToken.Parse(JsonConvert.SerializeObject(workItemUpdateList)
                .Replace("Sys", "System."));

            var deserializer = new DevOpsDeserializer(null, null);
            var result =
                deserializer.JsonWorkItemStartTime(jsonWorkItemUpdates);

            Assert.That(result, Is.EqualTo(DateTime.MinValue));
        }

        [Test]
        public void When_getting_an_end_time_for_work_item_card()
        {
            var changedDate = DateTime.Now.AddHours(-1);

            var workItemUpdateList = new List<JsonWorkItemCardUpdate>
            {
                new JsonWorkItemCardUpdate
                {
                    fields = new JsonWorkItemCardUpdateFields
                    {
                        SysState = new JsonWorkItemCardState
                        {
                            newValue = "Resolved"
                        },
                        SysChangedDate = new JsonWorkItemCardChangedDate
                        {
                            newValue = changedDate
                        }
                    }
                }
            };

            var jsonWorkItemUpdates = JToken.Parse(JsonConvert.SerializeObject(workItemUpdateList)
                .Replace("Sys", "System."));

            var deserializer = new DevOpsDeserializer(null, null);
            var result =
                deserializer.JsonWorkItemFinishTime(jsonWorkItemUpdates);

            Assert.That(result, Is.EqualTo(changedDate));
        }

        [Test]
        public void When_getting_an_end_time_for_work_item_card_that_has_not_ended()
        {
            var changedDate = DateTime.Now.AddHours(-1);

            var workItemUpdateList = new List<JsonWorkItemCardUpdate>
            {
                new JsonWorkItemCardUpdate
                {
                    fields = new JsonWorkItemCardUpdateFields
                    {
                        SysState = new JsonWorkItemCardState
                        {
                            newValue = ""
                        },
                        SysChangedDate = new JsonWorkItemCardChangedDate
                        {
                            newValue = changedDate
                        }
                    }
                }
            };

            var jsonWorkItemUpdates = JToken.Parse(JsonConvert.SerializeObject(workItemUpdateList)
                .Replace("Sys", "System."));

            var deserializer = new DevOpsDeserializer(null, null);
            var result =
                deserializer.JsonWorkItemFinishTime(jsonWorkItemUpdates);

            Assert.That(result, Is.EqualTo(DateTime.MaxValue));
        }
    }
}

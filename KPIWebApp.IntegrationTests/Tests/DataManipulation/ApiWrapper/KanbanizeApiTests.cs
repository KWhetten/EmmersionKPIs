﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Api;
using DataAccess.DataRepositories;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using JsonTaskItem = KPIWebApp.IntegrationTests.TestObjects.Kanbanize.JsonTaskItem;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.ApiWrapper
{
    [TestFixture]
    public class KanbanizeApiTests
    {
        [Test]
        public async Task When_getting_task_item_list()
        {
            var kanbanizeApi = new KanbanizeApi();
            var result = await kanbanizeApi.GetTaskItemListAsync(4);

            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void When_getting_task_item_history()
        {
            var taskItemIds = new List<int>
            {
                120, 121, 122, 123
            };
            var kanbanizeApiWrapper = new KanbanizeApi();
            var result = kanbanizeApiWrapper.GetHistoryEvents(taskItemIds, 5);

            Assert.That(result.First["historydetails"]["item"].First["eventtype"].ToString(), Is.EqualTo("Transitions"));
            Assert.That(result.First["historydetails"]["item"].First["historyevent"].ToString(), Is.EqualTo("Task archived"));
            Assert.That(result.First["historydetails"]["item"].First["taskid"].ToString(), Is.EqualTo("120"));
            Assert.That(result.First["historydetails"]["item"].First["author"].ToString(), Is.EqualTo("superuser"));
            Assert.That(result.First["historydetails"]["item"].First["entrydate"].ToString(), Is.EqualTo("2020-09-09 22:06:02"));
            Assert.That(result.First["historydetails"]["item"].First["historyid"].ToString(), Is.EqualTo("1741"));
        }

        [Test]
        public void When_getting_task_item_history_with_invalid_history()
        {
            var workItemId = new JsonTaskItem
            {
                taskid = 120
            };
            var jsonTaskItem = JObject.Parse(JsonConvert.SerializeObject(workItemId));

            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(x => x.Execute(It.IsAny<RestRequest>())).Returns(new RestResponse
            {
                Headers = {new Parameter("x-ms-continuationtoken", "358", ParameterType.HttpHeader)},
                Content =
                    "<note>\n<to>Tove</to>\n<from>Jani</from>\n<heading>Reminder</heading>\n<body>Don't forget me this weekend!</body>\n</note>"
            });

            var kanbanizeApiWrapper = new KanbanizeApi(mockRestClient.Object, new TaskItemRepository(), new DatabaseConnection());
            var result = kanbanizeApiWrapper.GetHistoryEvents(new List<int>{workItemId.taskid}, 5);

            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public async Task When_getting_board_ids()
        {
            var kanbanizeApi = new KanbanizeApi();
            var result = await kanbanizeApi.GetBoardIdsAsync();

            Assert.That(result, Is.EqualTo(new List<int>{4, 5}));
        }
    }
}

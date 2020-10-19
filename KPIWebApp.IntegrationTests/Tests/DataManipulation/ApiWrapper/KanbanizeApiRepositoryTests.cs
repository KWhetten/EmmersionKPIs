using System;
using System.Linq;
using DataAccess.ApiWrapper;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using JsonTaskItem = KPIWebApp.IntegrationTests.TestObjects.Kanbanize.JsonTaskItem;

namespace KPIDataExtractor.UnitTests.Tests.DataManipulation.ApiWrapper
{
    [TestFixture]
    public class KanbanizeApiRepositoryTests
    {
        [Test]
        public void When_getting_work_item_card_history()
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
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<xml><taskid>120</taskid><boardid>4</boardid><title>Add convenient link to start Google login</title><description/><type>Engineering</type><assignee>None</assignee><subtasks>0</subtasks><subtaskscomplete>0</subtaskscomplete><color>#809fdf</color><priority>Average</priority><size/><deadline/><deadlineoriginalformat/><extlink/><tags/><leadtime>1</leadtime><blocked/><blockedreason/><columnname>Released to Prod this week</columnname><lanename>Standard</lanename><subtaskdetails/><columnid>done_102</columnid><laneid>19</laneid><position>0</position><workflow>0</workflow><workflow_id>19</workflow_id><attachments/><columnpath>Released to Prod this week</columnpath><loggedtime>0</loggedtime><reporter>emandres</reporter><customfields/><updatedat>2020-08-27 10:40:23</updatedat><historydetails><item><eventtype>Transitions</eventtype><historyevent>Task moved</historyevent><taskid>120</taskid><details>From 'In Process.Ready for Prod Deploy' to 'Released to Prod this week'</details><author>emandres</author><entrydate>2020-08-27 10:40:23</entrydate><historyid>1271</historyid></item><item><eventtype>Updates</eventtype><historyevent>Task updated</historyevent><taskid>120</taskid><details>New title: Add convenient link to start Google login</details><author>emandres</author><entrydate>2020-08-27 08:52:09</entrydate><historyid>1254</historyid></item><item><eventtype>Reached limit</eventtype><historyevent>The column limit was reached</historyevent><taskid>120</taskid><details/><author>MatthewYKnowles</author><entrydate>2020-08-27 08:51:56</entrydate><historyid>1253</historyid></item><item><eventtype>Transitions</eventtype><historyevent>Task moved</historyevent><taskid>120</taskid><details>From 'Top Priority' to 'In Process.Ready for Prod Deploy'</details><author>MatthewYKnowles</author><entrydate>2020-08-27 08:51:56</entrydate><historyid>1252</historyid></item><item><eventtype>Transitions</eventtype><historyevent>Task moved</historyevent><taskid>120</taskid><details>From 'Engineering Backlog' to 'Top Priority'</details><author>emandres</author><entrydate>2020-08-26 08:45:51</entrydate><historyid>1165</historyid></item><item><eventtype>Updates</eventtype><historyevent>Link Deleted</historyevent><taskid>120</taskid><details>Child of 42 Title: Q3-4: Admins cannot manage their account without contacting support</details><author>emandres</author><entrydate>2020-08-20 13:16:13</entrydate><historyid>1065</historyid></item><item><eventtype>Updates</eventtype><historyevent>Color changed</historyevent><taskid>120</taskid><details>New color: #809fdf</details><author>mike.clement</author><entrydate>2020-08-13 14:52:53</entrydate><historyid>765</historyid></item><item><eventtype>Updates</eventtype><historyevent>Link Created</historyevent><taskid>120</taskid><details>Child of 42 Title: Q3-4: Admins cannot manage their account without contacting support</details><author>emandres</author><entrydate>2020-08-11 07:47:30</entrydate><historyid>485</historyid></item><item><eventtype>Updates</eventtype><historyevent>Task created</historyevent><taskid>120</taskid><details>Task : Add UI to start Google login</details><author>emandres</author><entrydate>2020-08-11 07:47:30</entrydate><historyid>483</historyid></item></historydetails></xml>"
            });

            var kanbanizeApiWrapper = new KanbanizeApiRepository(mockRestClient.Object);
            var result = kanbanizeApiWrapper.GetTaskItemHistory(jsonTaskItem, 5);

            Assert.That(result.First["eventtype"].ToString(), Is.EqualTo("Transitions"));
            Assert.That(result.First["historyevent"].ToString(), Is.EqualTo("Task moved"));
            Assert.That(result.First["taskid"].ToString(), Is.EqualTo("120"));
            Assert.IsTrue(result.First["details"].ToString().Contains("to 'Released to Prod this week"));
            Assert.That(result.First["author"].ToString(), Is.EqualTo("emandres"));
            Assert.That(result.First["entrydate"].ToString(), Is.EqualTo("2020-08-27 10:40:23"));
            Assert.That(result.First["historyid"].ToString(), Is.EqualTo("1271"));
        }

        [Test]
        public void When_getting_work_item_card_history_with_invalid_history()
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

            var kanbanizeApiWrapper = new KanbanizeApiRepository(mockRestClient.Object);
            var result = kanbanizeApiWrapper.GetTaskItemHistory(jsonTaskItem, 5);

            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void When_getting_work_item_card_list()
        {
            var kanbanizeApiWrapper = new KanbanizeApiRepository(new RestClient());
            var result = kanbanizeApiWrapper.GetTaskItemList(4);

            Assert.That(result.Last["taskid"].ToString(), Is.EqualTo("221"));
            Assert.That(result.Last["type"].ToString(), Is.EqualTo("Engineering"));
            Assert.That(result.Last["title"].ToString(), Is.EqualTo("Migrate holonet users to use emmersion.ai emails"));
            Assert.That(result.Last["priority"].ToString(), Is.EqualTo("Average"));
            Assert.That(result.Last["columnid"].ToString(), Is.EqualTo("archive_103"));
            Assert.That(result.Last["columnname"].ToString(), Is.EqualTo("Archive"));
            Assert.That(result.Last["reporter"].ToString(), Is.EqualTo("emandres"));
            Assert.That(result.Last["createdat"].ToString(), Is.EqualTo("2020-08-27 08:53:40"));
            Assert.That(result.Last["updatedat"].ToString(), Is.EqualTo("2020-10-06 22:06:39"));
            Assert.That(result.Last["comments"].ToString(), Is.EqualTo(""));
        }

        [Test]
        public void When_adding_archived_task_item_list_with_no_items()
        {
            var mockRestClient = new Mock<RestClient>();
            mockRestClient.Setup(x => x.Execute(It.IsAny<RestRequest>())).Returns(new RestResponse
            {
                Headers = {new Parameter("x-ms-continuationtoken", "358", ParameterType.HttpHeader)},
                Content = "<?xml version=\'1.0\' encoding=\'utf-8\'?>\n<xml></xml>"
            });
            var kanbanizeApiWrapper = new KanbanizeApiRepository(mockRestClient.Object);
            var result = kanbanizeApiWrapper.AddArchivedTaskItemList(new JArray(), 4);

            Assert.That(result, Is.EqualTo(new JArray()));
        }
    }
}

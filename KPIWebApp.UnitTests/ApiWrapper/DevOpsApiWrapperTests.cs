using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DataWrapper.ApiWrapper;
using DataObjects;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;

namespace KPIDataExtractor.UnitTests.ApiWrapper
{
    [TestFixture]
    public class DevOpsApiWrapperTests
    {
        private const string Organization = "emmersionlearning";

        [Test]
        public void When_getting_release_list()
        {
            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(x => x.Execute(It.IsAny<RestRequest>())).Returns(new RestResponse
            {
                Headers = {new Parameter("x-ms-continuationtoken", "358", ParameterType.HttpHeader)},
                Content =
                    "{\n\"value\": [\n{\"item1\": \"This is a json value\"}, {\n\"item2\": \"This is another json value\"}\n]\n}"
            });

            var devOpsApiWrapper = new DevOpsApiWrapper(mockRestClient.Object);

            var result = devOpsApiWrapper.GetReleaseList();

            Assert.That(result.Count(), Is.EqualTo(4));
            Assert.That(result.First, Is.EqualTo(JObject.Parse("{\n\"item1\": \"This is a json value\"\n}")));
            Assert.That(result.Last, Is.EqualTo(JObject.Parse("{\n\"item2\": \"This is another json value\"\n}")));
        }

        [Test]
        public void When_getting_release_list_with_no_continuation_token()
        {
            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(x => x.Execute(It.IsAny<RestRequest>())).Returns(new RestResponse
            {
                Headers = {new Parameter("This doesn't matter", ":)", ParameterType.HttpHeader)},
                Content = "{\n\"value\": []\n}"
            });

            var devOpsApiWrapper = new DevOpsApiWrapper(mockRestClient.Object);

            var result = devOpsApiWrapper.GetReleaseList();

            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void When_getting_work_item_card_history()
        {
            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.SetupSequence(x => x.Execute(It.IsAny<RestRequest>())).Returns(new RestResponse
            {
                Headers = {new Parameter("This doesn't matter", ":)", ParameterType.HttpHeader)},
                Content = "<invalid-xml-message>This is a message</invalid-xml-message>"
            }).Returns(new RestResponse
            {
                Headers = {new Parameter("This doesn't matter", ":)", ParameterType.HttpHeader)},
                Content = "{\n\"value\": \"This is a value.\"\n}"
            });


            var devOpsApiWrapper = new DevOpsApiWrapper(mockRestClient.Object);
            var result = devOpsApiWrapper.GetWorkItemUpdates(new WorkItemCard());

            Assert.That(result.ToString(), Is.EqualTo("This is a value."));
        }

        [Test]
        public void When_getting_work_item_card_list()
        {
            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(x => x.Execute(It.IsAny<RestRequest>())).Returns(new RestResponse
            {
                Headers = {new Parameter("This doesn't matter", ":)", ParameterType.HttpHeader)},
                Content = "{\n\"value\": [1, 2, 3, 4, 5]\n}"
            });

            var uri = new Uri($"https://dev.azure.com/{Organization}");
            var credentials = new VssCredentials(true);

            var mockWorkItemTrackingHttpClient = new Mock<WorkItemTrackingHttpClient>(uri, credentials);
            mockWorkItemTrackingHttpClient.Setup(x => x.QueryByWiqlAsync(It.IsAny<Wiql>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int?>(),
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WorkItemQueryResult
                {
                    WorkItems = new[]
                    {
                        new WorkItemReference
                        {
                            Id = 1
                        },
                        new WorkItemReference
                        {
                            Id = 2
                        },
                        new WorkItemReference
                        {
                            Id = 3
                        },
                        new WorkItemReference
                        {
                            Id = 4
                        },
                        new WorkItemReference
                        {
                            Id = 5
                        }
                    }
                });

            mockWorkItemTrackingHttpClient.Setup(x
                    => x.GetWorkItemsAsync(It.IsAny<IEnumerable<int>>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<WorkItemExpand?>(),
                        It.IsAny<WorkItemErrorPolicy?>(),
                        It.IsAny<object>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WorkItem>
                {
                    new WorkItem
                    {
                        Id = 1
                    },
                    new WorkItem
                    {
                        Id = 2
                    },
                    new WorkItem
                    {
                        Id = 3
                    },
                    new WorkItem
                    {
                        Id = 4
                    },
                    new WorkItem
                    {
                        Id = 5
                    }
                });

            var devOpsApiWrapper = new DevOpsApiWrapper(mockRestClient.Object, mockWorkItemTrackingHttpClient.Object);
            var result = devOpsApiWrapper.GetWorkItemCardList();

            Assert.That(result.Result.Count(), Is.EqualTo(5));
            Assert.That(result.Result[0].ToString(), Is.EqualTo("1"));
            Assert.That(result.Result[1].ToString(), Is.EqualTo("2"));
            Assert.That(result.Result[2].ToString(), Is.EqualTo("3"));
            Assert.That(result.Result[3].ToString(), Is.EqualTo("4"));
            Assert.That(result.Result[4].ToString(), Is.EqualTo("5"));
        }

        [Test]
        public void When_getting_work_item_card_list_with_no_items()
        {
            var mockRestClient = new Mock<IRestClient>();
            mockRestClient.Setup(x => x.Execute(It.IsAny<RestRequest>())).Returns(new RestResponse
            {
                Headers = {new Parameter("This doesn't matter", ":)", ParameterType.HttpHeader)},
                Content = "{\n\"value\": [1]\n}"
            });

            var uri = new Uri($"https://dev.azure.com/{Organization}");
            var credentials = new VssCredentials(true);

            var mockWorkItemTrackingHttpClient = new Mock<WorkItemTrackingHttpClient>(uri, credentials);
            mockWorkItemTrackingHttpClient.Setup(x => x.QueryByWiqlAsync(It.IsAny<Wiql>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int?>(),
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WorkItemQueryResult
                {
                    WorkItems = new WorkItemReference[] { }
                });

            mockWorkItemTrackingHttpClient.Setup(x
                    => x.GetWorkItemsAsync(It.IsAny<IEnumerable<int>>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<DateTime?>(),
                        It.IsAny<WorkItemExpand?>(),
                        It.IsAny<WorkItemErrorPolicy?>(),
                        It.IsAny<object>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WorkItem>());

            var devOpsApiWrapper = new DevOpsApiWrapper(mockRestClient.Object, mockWorkItemTrackingHttpClient.Object);
            var result = devOpsApiWrapper.GetWorkItemCardList();

            Assert.That(result.Result.Count(), Is.EqualTo(0));
        }
    }
}

using System.Linq;
using DataAccess.Api;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.ApiWrapper
{
    [TestFixture]
    public class DevOpsApiTests
    {
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

            var devOpsApiWrapper = new DevOpsApi(mockRestClient.Object);

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

            var devOpsApiWrapper = new DevOpsApi(mockRestClient.Object);

            var result = devOpsApiWrapper.GetReleaseList();

            Assert.That(result.Count(), Is.EqualTo(0));
        }
    }
}

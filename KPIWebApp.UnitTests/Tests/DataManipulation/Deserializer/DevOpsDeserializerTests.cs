using System;
using DataAccess.DataRepositories;
using DataAccess.Deserialize;
using KPIDataExtractor.UnitTests.TestObjects.DevOps;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.DataManipulation.Deserializer
{
    [TestFixture]
    public class DevOpsDeserializerTests
    {
        [Test]
        public void When_deserializing_releases()
        {
            var startTime = DateTimeOffset.Now.AddHours(-2);
            var finishTime = DateTimeOffset.Now.AddHours(-1);

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
                completedOn = finishTime,
                releaseDefinition = new ReleaseDefinition
                {
                    name = "TrueNorthTest Release"
                }
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
                startedOn = DateTimeOffset.Now.AddHours(-3),
                completedOn = finishTime,
                releaseDefinition = new ReleaseDefinition
                {
                    name = "Assessments PC"
                }
            };

            var release1JToken = JToken.Parse(JsonConvert.SerializeObject(release1));
            var release2JToken = JToken.Parse(JsonConvert.SerializeObject(release2));

            var jsonReleases = new JArray
            {
                release1JToken,
                release2JToken
            };

            var mockReleaseRepository = new Mock<ReleaseRepository>();
            mockReleaseRepository.Setup(x => x.ReleaseIsFinishedInDatabase(It.IsAny<int>()))
                .Returns(false);

            var deserializer = new DevOpsDeserializer(mockReleaseRepository.Object);
            var result = deserializer.DeserializeReleases(jsonReleases);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(release1.id));
            Assert.That(result[0].Attempts, Is.EqualTo(release1.attempt));
            Assert.That(result[0].FinishTime, Is.EqualTo(release1.completedOn));
            Assert.That(result[0].Name, Is.EqualTo(release1.release.name));
            Assert.That(result[0].ReleaseEnvironment.Id, Is.EqualTo(release1.definitionEnvironmentId));
            Assert.That(result[0].ReleaseEnvironment.Name, Is.EqualTo(release1.releaseEnvironment.name));
            Assert.That(result[0].StartTime, Is.EqualTo(release1.startedOn));
            Assert.That(result[0].State, Is.EqualTo(release1.deploymentStatus));

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[1].Id, Is.EqualTo(release2.id));
            Assert.That(result[1].Attempts, Is.EqualTo(release2.attempt));
            Assert.That(result[1].FinishTime, Is.EqualTo(release2.completedOn));
            Assert.That(result[1].Name, Is.EqualTo(release2.release.name));
            Assert.That(result[1].ReleaseEnvironment.Id, Is.EqualTo(release2.definitionEnvironmentId));
            Assert.That(result[1].ReleaseEnvironment.Name, Is.EqualTo(release2.releaseEnvironment.name));
            Assert.That(result[1].StartTime, Is.EqualTo(release2.startedOn));
            Assert.That(result[1].State, Is.EqualTo(release2.deploymentStatus));
        }
    }

    public class ReleaseDefinition
    {
        public string name { get; set; }
    }
}

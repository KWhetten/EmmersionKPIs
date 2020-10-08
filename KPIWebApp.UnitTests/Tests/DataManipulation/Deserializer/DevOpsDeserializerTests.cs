using System;
using DataManipulation.Deserialize;
using KPIDataExtractor.UnitTests.Objects.DevOps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.DataWrapper.Deserializer
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
                completedOn = DateTime.Now,
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
                startedOn = DateTime.Now.AddHours(-3),
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

            var deserializer = new DevOpsDeserializer();
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
    }

    public class ReleaseDefinition
    {
        public string name { get; set; }
    }
}

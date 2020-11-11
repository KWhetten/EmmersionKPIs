using System;
using KPIDataExtractor.UnitTests.Tests.DataManipulation.Deserializer;

namespace KPIDataExtractor.UnitTests.TestObjects.DevOps
{
    public class JsonRelease
    {
        public int id { get; set; }
        public JsonRelease2 release { get; set; }
        public string deploymentStatus { get; set; }
        public int definitionEnvironmentId { get; set; }
        public JsonReleaseEnvironment releaseEnvironment { get; set; }
        public int attempt { get; set; }
        public DateTimeOffset startedOn { get; set; }
        public DateTimeOffset completedOn { get; set; }
        public ReleaseDefinition releaseDefinition;
    }

    public class JsonRelease2
    {
        public string name { get; set; }
    }

    public class JsonReleaseEnvironment
    {
        public string name { get; set; }
    }
}

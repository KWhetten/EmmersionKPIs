using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using Newtonsoft.Json.Linq;

namespace DataAccess.Deserialize
{
    public interface IDevOpsDeserializer
    {
        List<Release> DeserializeReleases(JToken jsonObjects);
    }

    public class DevOpsDeserializer : IDevOpsDeserializer
    {
        public List<Release> DeserializeReleases(JToken jsonObjects)
        {
            var releaseRepository = new ReleaseRepository();
            return (from item in jsonObjects
                where ((item["releaseDefinition"]["name"].ToString() == "TrueNorthTest Release"
                           || item["releaseDefinition"]["name"].ToString() == "Assessments PC")
                        && !releaseRepository.ReleaseIsFinishedInDatabase((int) item["id"]))
                select new Release
                {
                    Id = (int) item["id"],
                    Name = (string) item["release"]["name"],
                    State = (string) item["deploymentStatus"],
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = (int) item["definitionEnvironmentId"],
                        Name = (string) item["releaseEnvironment"]["name"]
                    },
                    Attempts = (int) item["attempt"],
                    StartTime = (DateTimeOffset) item["startedOn"],
                    FinishTime = (DateTimeOffset) item["completedOn"]
                }).ToList();
        }
    }
}

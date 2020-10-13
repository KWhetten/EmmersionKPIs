using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Objects;
using Newtonsoft.Json.Linq;

namespace DataAccess.Deserialize
{
    public interface IDevOpsDeserializer
    {
        List<Release> Releases(JToken jsonObjects);
    }

    public class DevOpsDeserializer : IDevOpsDeserializer
    {
        public List<Release> Releases(JToken jsonObjects)
        {
            return (from item in jsonObjects
                where (DateTime) item["completedOn"] >= DateTime.Now.AddDays(-360)
                      && (item["releaseDefinition"]["name"].ToString() == "TrueNorthTest Release"
                          || item["releaseDefinition"]["name"].ToString() == "Assessments PC")
                select new Release
                {
                    Id = (int) item["id"],
                    Name = (string) item["release"]["name"],
                    Status = (string) item["deploymentStatus"],
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = (int) item["definitionEnvironmentId"],
                        Name = (string) item["releaseEnvironment"]["name"]
                    },
                    Attempts = (int) item["attempt"],
                    StartTime = (DateTime) item["startedOn"],
                    FinishTime = (DateTime) item["completedOn"]
                }).ToList();
        }
    }
}

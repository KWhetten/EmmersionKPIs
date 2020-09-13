using System;
using System.Collections.Generic;
using System.Linq;
using DataObjects;
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
            return jsonObjects.Select(jsonObject => new Release
                {
                    Id = (int) jsonObject["id"],
                    Name = (string) jsonObject["release"]["name"],
                    Status = (string) jsonObject["deploymentStatus"],
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = (int) jsonObject["definitionEnvironmentId"],
                        Name = (string) jsonObject["releaseEnvironment"]["name"]
                    },
                    Attempts = (int) jsonObject["attempt"],
                    StartTime = (DateTime) jsonObject["startedOn"],
                    FinishTime = (DateTime) jsonObject["completedOn"]
                })
                .ToList();
        }


    }
}

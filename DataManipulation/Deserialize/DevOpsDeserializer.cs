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
        private readonly IReleaseRepository releaseRepository;
        private readonly IReleaseEnvironmentRepository releaseEnvironmentRepository;

        public DevOpsDeserializer()
        {
            releaseRepository = new ReleaseRepository();
            releaseEnvironmentRepository = new ReleaseEnvironmentRepository();
        }
        public DevOpsDeserializer(IReleaseRepository releaseRepository)
        {
            this.releaseRepository = releaseRepository;
        }
        public List<Release> DeserializeReleases(JToken jsonObjects)
        {
            List<Release> list = new List<Release>();
            foreach (var item in jsonObjects)
            {
                if ((item["releaseDefinition"]["name"].ToString().Contains("TrueNorthTest")
                     || item["releaseDefinition"]["name"].ToString().Contains("Assessments")
                     || item["releaseDefinition"]["name"].ToString().Contains("Production"))
                    && !releaseRepository.ReleaseIsFinishedInDatabase((int) item["id"]))

                    releaseEnvironmentRepository.SaveReleaseEnvironmentAsync((int) item["releaseDefinition"]["id"],
                        item["releaseDefinition"]["name"].ToString());

                    list.Add(new Release
                    {
                        Id = (int) item["id"],
                        Name = (string) item["release"]["name"],
                        State = (string) item["deploymentStatus"],
                        ReleaseEnvironment = new ReleaseEnvironment {Id = (int) item["definitionEnvironmentId"], Name = (string) item["releaseEnvironment"]["name"]},
                        Attempts = (int) item["attempt"],
                        StartTime = (DateTimeOffset) item["startedOn"],
                        FinishTime = (DateTimeOffset) item["completedOn"]
                    });
            }

            return list;
        }
    }
}

﻿﻿using System;
using System.Text;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DataManipulation.ApiWrapper
{
    public interface IDevOpsApiWrapper
    {
        JToken GetReleaseList();
    }

    public class DevOpsApiWrapper : IDevOpsApiWrapper
    {
        private readonly IRestClient client;
        private const string PersonalAccessToken = "y74r3pyqwyre2iq77ikznq7ewzhpgtvon6wcjurqvqcnrrltae7a";
        private const string Organization = "emmersionlearning";
        private const string Project = "EmmersionLearning";

        public DevOpsApiWrapper(IRestClient client)
        {
            this.client = client;
        }

        public int? ContinuationToken { get; set; }

        public string GetInformation(string uri)
        {
            var request = new RestRequest(uri, Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization",
                $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($":{PersonalAccessToken}"))}");
            var response = client.Execute(request);

            foreach (var header in response.Headers)
            {
                if (header.Name == "x-ms-continuationtoken")
                {
                    var newValue = int.Parse((string) header.Value);
                    if (ContinuationToken != newValue)
                    {
                        ContinuationToken = newValue;
                        return response.Content;
                    }
                }
            }

            ContinuationToken = null;

            return response.Content;
        }

        public JToken GetReleaseList()
        {
            const string deploymentStatus = "succeeded,failed";

            var uri =
                $"https://vsrm.dev.azure.com/{Organization}/{Project}/_apis/release/deployments?deploymentStatus={deploymentStatus}&api-version=5.1";

            var resultList = JObject.Parse(GetInformation(uri))["value"] as JArray;
            while (ContinuationToken != null)
            {
                uri =
                    $"https://vsrm.dev.azure.com/{Organization}/{Project}/_apis/release/deployments?deploymentStatus={deploymentStatus}&continuationToken={ContinuationToken}&api-version=5.1";

                try
                {
                    var temp = GetInformation(uri);
                    var newResult = JObject.Parse(temp)["value"] as JArray;

                foreach (var item in newResult)
                {
                    resultList?.Add(item);
                }
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            return resultList;
        }
    }
}

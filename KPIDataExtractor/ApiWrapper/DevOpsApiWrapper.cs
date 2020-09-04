using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KPIDataExtractor.Objects;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace KPIDataExtractor.ApiWrapper
{
    public interface IDevOpsApiWrapper
    {
        int? ContinuationToken { get; set; }
        string GetInformation(string uri);
        Task<JToken> GetWorkItemCardList();
        JToken GetWorkItemUpdates(WorkItemCard workItemCard);
        Task<IList<WorkItem>> QueryWorkItemIds();
        JToken GetReleaseList();
    }

    public class DevOpsApiWrapper : IDevOpsApiWrapper
    {
        private readonly IRestClient client;
        private readonly WorkItemTrackingHttpClient httpClient;
        private const string PersonalAccessToken = "y74r3pyqwyre2iq77ikznq7ewzhpgtvon6wcjurqvqcnrrltae7a";
        private const string Organization = "emmersionlearning";
        private const string Project = "EmmersionLearning";

        public DevOpsApiWrapper(IRestClient client, WorkItemTrackingHttpClient httpClient)
        {
            this.client = client;
            this.httpClient = httpClient;
        }

        public DevOpsApiWrapper(IRestClient client)
        {
            var uri = new Uri($"https://dev.azure.com/{Organization}");
            var credentials = new VssBasicCredential(string.Empty, PersonalAccessToken);
            httpClient = new WorkItemTrackingHttpClient(uri, credentials);
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

        public async Task<JToken> GetWorkItemCardList()
        {
            var workItemCardList = await QueryWorkItemIds();

            var result = new JArray();

            while (workItemCardList.Count > 0)
            {
                var ids = GetWorkItemIds(workItemCardList.Take(200));
                workItemCardList = workItemCardList.TakeLast(workItemCardList.Count - 200).ToList();

                var uri =
                    $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workitems?ids={ids}&api-version=5.1";

                var newResult = JObject.Parse(GetInformation(uri))["value"];

                foreach (var item in newResult)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public JToken GetWorkItemUpdates(WorkItemCard workItemCard)
        {
            string json;
            do
            {
                json = GetInformation(
                    $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workItems/{workItemCard.Id}/updates?api-version=5.1");
            } while (json.StartsWith("<"));

            return !string.IsNullOrEmpty(json)
                ? JObject.Parse(json)["value"]
                : new JArray();
        }

        public async Task<IList<WorkItem>> QueryWorkItemIds()
        {
            var query = "Select [Id] " +
                        "From WorkItems " +
                        "WHERE ([System.WorkItemType] == 'Strategic Product Work' OR " +
                        "[System.WorkItemType] == 'Tactical Product Work' OR " +
                        "[System.WorkItemType] == 'Unanticipated Product Work' OR " +
                        "[System.WorkItemType] == 'Strategic Engineering Work' OR " +
                        "[System.WorkItemType] == 'Tactical Engineering Work' OR " +
                        "[System.WorkItemType] == 'Unanticipated Engineering Work') " +
                        "AND [System.BoardColumn] != 'Parking Lot' " +
                        "Order By [Changed Date] Desc";

            var wiql = new Wiql
            {
                Query = query
            };

            var result = await httpClient.QueryByWiqlAsync(wiql).ConfigureAwait(false);

            var ids = result.WorkItems.Select(item => item.Id).ToList();
            if (ids.Count == 0) return Array.Empty<WorkItem>();

            var itemList = new List<WorkItem>();
            for (var i = 0;
                i < ids.Count;
                i = 0)
            {
                var ids200 = ids.Take(200);
                ids = ids.TakeLast(ids.Count - 200).ToList();
                itemList = itemList.Concat(await httpClient.GetWorkItemsAsync(ids200).ConfigureAwait(false)).ToList();
            }

            return itemList;
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

                var newResult = JObject.Parse(GetInformation(uri))["value"] as JArray;

                foreach (var item in newResult)
                {
                    resultList?.Add(item);
                }
            }

            return resultList;
        }

        private static string GetWorkItemIds(IEnumerable<WorkItem> workItemsList)
        {
            var ids = workItemsList.Aggregate("", (current, workItem) => current + $"{workItem.Id},");
            return ids.Length > 0
                ? ids.Remove(ids.Length - 1)
                : "";
        }
    }
}

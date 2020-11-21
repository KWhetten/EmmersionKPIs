using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using DataAccess.DataRepositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DataAccess.Api
{
    public interface IKanbanizeApiRepository
    {
        string GetInformation(string uri, string body);
        JToken GetTaskItemList(int boardId);
        Task<JToken> GetHistoryEventsAsync(List<int> taskItemIds, int boardId);
    }

    public class KanbanizeApi : IKanbanizeApiRepository
    {
        private readonly IRestClient client;
        private const string ApiKey = "TUilAxpp68ooVyExDLxkwNfQpVt8TTO7ZMWk1Mif";
        private const string Subdomain = "emmersion";

        public KanbanizeApi()
        {
            client = new RestClient();
        }

        public KanbanizeApi(IRestClient client)
        {
            this.client = client;
        }

        public string GetInformation(string uri, string body)
        {
            var request = new RestRequest(uri, Method.POST);
            request.AddHeader("ApiKey", ApiKey);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var response = client.Execute(request);

            return response.Content;
        }

        public JToken GetTaskItemList(int boardId)
        {
            var uri =
                $"http://{Subdomain}.kanbanize.com/index.php/api/kanbanize/get_all_tasks/";
            var body = "{\"boardid\":\"" + boardId + "\", \"comments\": \"yes\"}";

            var xmlTaskItemList = GetInformation(uri, body);

            var doc = new XmlDocument();
            doc.LoadXml(xmlTaskItemList);

            var json = JObject.Parse(JsonConvert.SerializeXmlNode(doc));
            var jsonList = json["xml"]["item"];

            var result = new JArray();

            foreach (var item in jsonList)
            {
                var taskItemRepository = new TaskItemRepository();
                if (item["workflow_name"].ToString().Contains("Delivery")
                    && !taskItemRepository.TaskItemHasAlreadyBeenReleasedAsync((int) item["taskid"]))
                {
                    result.Add(item);
                }
                else
                {
                    if (item["workflow_name"].ToString().Contains("Delivery"))
                    {
                        Console.WriteLine(
                            $"Task {item["taskid"]} has already been released. No more updates are needed.");
                    }
                }
            }

            return AddArchivedTaskItemList(result, boardId);
        }

        public JArray AddArchivedTaskItemList(JArray result, int boardId)
        {
            try
            {
                var uri =
                    $"http://{Subdomain}.kanbanize.com/index.php/api/kanbanize/get_all_tasks/";
                var body = "{\"boardid\":\"" + boardId + "\", \"comments\": \"yes\", \"container\": \"archive\"}";

                var xmlTaskItemList = GetInformation(uri, body);

                var doc = new XmlDocument();
                doc.LoadXml(xmlTaskItemList);

                var json = JObject.Parse(JsonConvert.SerializeXmlNode(doc));
                var jsonList = json["xml"]["task"]["item"];

                foreach (var item in jsonList)
                {
                    if ((boardId == 4 && (int) item["workflow_id"] == 19)
                        || (boardId == 5 && (int) item["workflow_id"] == 8))
                    {
                        result.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("");
            }

            return result;
        }

        public async Task<JToken> GetHistoryEventsAsync(List<int> taskItemIds, int boardId)
        {
            var taskItemIdsString = taskItemIds.Aggregate("", (current, taskItemId) => current + $"{taskItemId},");
            taskItemIdsString = taskItemIdsString.Substring(0, taskItemIdsString.Length - 1);

            var uri =
                $"http://{Subdomain}.kanbanize.com/index.php/api/kanbanize/get_task_details/";
            var body = "{\"boardid\":\"" + boardId + "\", \"taskid\":[" + taskItemIdsString +
                       "], \"history\": \"yes\"}";
            var xmlTaskItemDetails = GetInformation(uri, body);

            var doc = new XmlDocument();
            doc.LoadXml(xmlTaskItemDetails);

            var json = JObject.Parse(JsonConvert.SerializeXmlNode(doc));

            try
            {
                return json["xml"]["item"];
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Unable to get history for Task {taskItemIds}.\nException: {ex.Message}\nJson:{json}");
                return new JArray();
            }
        }
    }
}

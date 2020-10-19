using System;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DataAccess.ApiWrapper
{
    public interface IKanbanizeApiRepository
    {
        string GetInformation(string uri, string body);
        JToken GetTaskItemList(int boardId);
        JToken GetTaskItemHistory(JToken taskItem, int boardId);
    }

    public class KanbanizeApiRepository : IKanbanizeApiRepository
    {
        private readonly IRestClient client;
        private const string ApiKey = "TUilAxpp68ooVyExDLxkwNfQpVt8TTO7ZMWk1Mif";
        private const string Subdomain = "emmersion";

        public KanbanizeApiRepository(IRestClient client)
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
                if (item["workflow_name"].ToString().Contains("Delivery"))
                {
                    result.Add(item);
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
                    if ((boardId == 4 && (int) item["workflow_id"] == 19) ||
                        (boardId == 5 && (int) item["workflow_id"] == 8))
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

        public JToken GetTaskItemHistory(JToken jsonTaskItem, int boardId)
        {
            var uri =
                $"http://{Subdomain}.kanbanize.com/index.php/api/kanbanize/get_task_details/";
            var body = "{\"boardid\":\"" + boardId + "\", \"taskid\":\"" + jsonTaskItem["taskid"] + "\", \"history\": \"yes\"}";

            var xmlTaskItemDetails = GetInformation(uri, body);

            var doc = new XmlDocument();
            doc.LoadXml(xmlTaskItemDetails);

            var json = JObject.Parse(JsonConvert.SerializeXmlNode(doc));
            try
            {
                return json["xml"]["historydetails"]["item"];
            }
            catch
            {
                return "";
            }
        }
    }
}

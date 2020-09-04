using System.Xml;
using KPIDataExtractor.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace KPIDataExtractor.ApiWrapper
{
    public interface IKanbanizeApiWrapper
    {
        string GetInformation(string uri, string body);
        JToken GetWorkItemCardList(int boardId);
        JToken GetWorkItemCardHistory(JToken workItemCard, int boardId);
    }

    public class KanbanizeApiWrapper : IKanbanizeApiWrapper
    {
        private readonly IRestClient client;
        private const string ApiKey = "TUilAxpp68ooVyExDLxkwNfQpVt8TTO7ZMWk1Mif";
        private const string Subdomain = "emmersion";

        public KanbanizeApiWrapper(IRestClient client)
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

        public JToken GetWorkItemCardList(int boardId)
        {
            var uri =
                $"http://{Subdomain}.kanbanize.com/index.php/api/kanbanize/get_all_tasks/";
            var body = "{\"boardid\":\"" + boardId + "\", \"comments\": \"yes\"}";

            var xmlWorkItemCardList = GetInformation(uri, body);

            var doc = new XmlDocument();
            doc.LoadXml(xmlWorkItemCardList);

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

            return result;
        }

        public JToken GetWorkItemCardHistory(JToken jsonWorkItemCard, int boardId)
        {
            var uri =
                $"http://{Subdomain}.kanbanize.com/index.php/api/kanbanize/get_task_details/";
            var body = "{\"boardid\":\"" + boardId + "\", \"taskid\":\"" + jsonWorkItemCard["taskid"] + "\", \"history\": \"yes\"}";

            var xmlWorkItemCardDetails = GetInformation(uri, body);

            var doc = new XmlDocument();
            doc.LoadXml(xmlWorkItemCardDetails);

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using DataAccess.DataRepositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DataAccess.Api
{
    public interface IKanbanizeApi
    {
        string GetInformation(string uri, string body);
        Task<JToken> GetTaskItemListAsync(int boardId);
        JToken GetHistoryEvents(List<int> taskItemIds, int boardId);
        public Task<List<int>> GetBoardIdsAsync();
        public Task UpdateBoardUsersAsync(int boardId);
    }

    public class KanbanizeApi : IKanbanizeApi
    {
        private readonly IRestClient client;
        private readonly ITaskItemRepository taskItemRepository;
        private IDatabaseConnection databaseConnection;
        private readonly string apiKey = File.ReadLines($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/EmmersionKPI/kanbanizeApiKey.txt").First();
        private const string Subdomain = "emmersion";
        private readonly DevelopmentTeamsRepository developmentTeamsRepository = new DevelopmentTeamsRepository();

        public KanbanizeApi()
        {
            client = new RestClient();
            taskItemRepository = new TaskItemRepository();
        }

        public KanbanizeApi(IRestClient client, ITaskItemRepository taskItemRepository, IDatabaseConnection databaseConnection)
        {
            this.client = client;
            this.taskItemRepository = taskItemRepository;
            this.databaseConnection = databaseConnection;
        }

        public string GetInformation(string uri, string body)
        {
            var request = new RestRequest(uri, Method.POST);
            request.AddHeader("ApiKey", apiKey);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var response = client.Execute(request);

            return response.Content;
        }

        public async Task<JToken> GetTaskItemListAsync(int boardId)
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
                if (item["workflow_name"].ToString().Contains("Delivery")
                    && !await taskItemRepository.TaskItemHasBeenReleasedAsync((int) item["taskid"]))
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

            return await AddArchivedTaskItemListAsync(result, boardId);
        }

        public async Task<JArray> AddArchivedTaskItemListAsync(JArray result, int boardId)
        {
            JObject json;
            JToken jsonList;
            var uri =
                $"http://{Subdomain}.kanbanize.com/index.php/api/kanbanize/get_all_tasks/";
            var body = "{\"boardid\":\"" + boardId + "\", \"comments\": \"yes\", \"container\": \"archive\"}";

            var xmlTaskItemList = GetInformation(uri, body);

            var doc = new XmlDocument();
            doc.LoadXml(xmlTaskItemList);

            try
            {
                json = JObject.Parse(JsonConvert.SerializeXmlNode(doc));
                jsonList = json["xml"]["task"]["item"];
            }
            catch (InvalidOperationException ex)
            {
                return new JArray();
            }

            foreach (var item in jsonList)
            {
                try
                {
                    var taskItemRepository = new TaskItemRepository();
                    if (((int) item["workflow_id"] == 19 && boardId == 4)
                        || ((int) item["workflow_id"] == 8 && boardId == 5)
                        && !await taskItemRepository.TaskItemHasBeenReleasedAsync((int) item["taskid"]))
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
                catch (Exception ex)
                {
                    Console.Write("");
                }
            }

            return result;
        }

        public JToken GetHistoryEvents(List<int> taskItemIds, int boardId)
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

        public async Task<List<int>> GetBoardIdsAsync()
        {
            var uri = $"https://{Subdomain}.kanbanize.com/index.php/api/kanbanize/get_projects_and_boards/";
            var body = "{}";
            var xmlTaskItemList = GetInformation(uri, body);

            var doc = new XmlDocument();
            doc.LoadXml(xmlTaskItemList);

            var json = JObject.Parse(JsonConvert.SerializeXmlNode(doc));

            var jsonInformation = json["xml"]["projects"]["item"];

            foreach (var boardGroup in jsonInformation)
            {
                if (boardGroup["name"].ToString() != "Product Development") continue;
                jsonInformation = boardGroup;
                break;
            }

            jsonInformation = jsonInformation["boards"]["item"];
            var boardIds = new List<int>();

            foreach (var apple in jsonInformation)
            {
                await developmentTeamsRepository.InsertDevTeamAsync((int) apple["id"], apple["name"].ToString());
                boardIds.Add((int) apple["id"]);
            }

            return boardIds;
        }

        public async Task UpdateBoardUsersAsync(int boardId)
        {
            var uri = $"https://{Subdomain}.kanbanize.com/index.php/api/kanbanize/get_board_settings/";
            var body = $"{{\"boardid\":\"{boardId}\"}}";
            var xmlTaskItemList = GetInformation(uri, body);

            var doc = new XmlDocument();
            doc.LoadXml(xmlTaskItemList);

            var json = JObject.Parse(JsonConvert.SerializeXmlNode(doc));
            var usernames = json["xml"]["usernames"]["item"];

            var developerRepository = new DeveloperRepository();
            foreach (var username in usernames)
            {
                await developerRepository.InsertDeveloperAsync(username.ToString());
            }
        }
    }
}

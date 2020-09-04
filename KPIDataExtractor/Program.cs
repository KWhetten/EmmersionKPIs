using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using KPIDataExtractor.ApiWrapper;
using KPIDataExtractor.DatabaseAccess;
using KPIDataExtractor.Deserialize;
using RestSharp;

namespace KPIDataExtractor
{
    public static class Program
    {
        private static readonly IDevOpsApiWrapper DevOpsApiWrapper = new DevOpsApiWrapper(new RestClient());
        private static readonly IKanbanizeApiWrapper KanbanizeApiWrapper = new KanbanizeApiWrapper(new RestClient());
        private static readonly IDataAccess DataAccess = new DataAccess();
        private static readonly IDevOpsDeserializer DevOpsDeserializer = new DevOpsDeserializer(DevOpsApiWrapper, DataAccess);
        private static readonly IKanbanizeDeserializer KanbanizeDeserializer = new KanbanizeDeserializer(KanbanizeApiWrapper, DataAccess);

        public static async Task Main()
        {
            InsertReleasesIntoDatabaseFromApi();

            await InsertWorkItemsIntoDatabaseFromApi();
        }

        private static void InsertReleasesIntoDatabaseFromApi()
        {
            var releases = DevOpsApiWrapper.GetReleaseList();

            DataAccess.InsertReleaseList(DevOpsDeserializer.Releases(releases));
        }

        private static async Task InsertWorkItemsIntoDatabaseFromApi()
        {
            const int enterpriseTeamBoardId = 4;
            const int assessmentsTeamBoardId = 5;

            await InsertDevOpsCards();
            InsertKanbanizeCards(enterpriseTeamBoardId);
            InsertKanbanizeCards(assessmentsTeamBoardId);
        }

        private static async Task InsertDevOpsCards()
        {
            var workItemCardList = await DevOpsApiWrapper.GetWorkItemCardList();
            if (workItemCardList.Any())
            {
                DataAccess.InsertWorkItemCardList(DevOpsDeserializer.WorkItemCardList(workItemCardList));
            }
            else
                Console.WriteLine("No new cards.");
        }

        private static void InsertKanbanizeCards(int boardId)
        {
            var workItemCardList = KanbanizeApiWrapper.GetWorkItemCardList(boardId);
            if (workItemCardList.Any())
            {
                DataAccess.InsertWorkItemCardList(KanbanizeDeserializer.WorkItemCardList(workItemCardList, boardId));
            }
            else
                Console.WriteLine("No new cards.");
        }
    }
}

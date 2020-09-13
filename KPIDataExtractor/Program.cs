using System;
using System.Linq;
using DataAccess.ApiWrapper;
using DataAccess.DatabaseAccess;
using DataAccess.Deserialize;
using RestSharp;

namespace KPIDataExtractor
{
    public static class Program
    {
        private static readonly IDevOpsApiWrapper DevOpsApiWrapper = new DevOpsApiWrapper(new RestClient());
        private static readonly IKanbanizeApiWrapper KanbanizeApiWrapper = new KanbanizeApiWrapper(new RestClient());
        private static readonly IDataAccess DataAccess = new DatabaseWrapper();
        private static readonly IDevOpsDeserializer DevOpsDeserializer = new DevOpsDeserializer();
        private static readonly IKanbanizeDeserializer KanbanizeDeserializer = new KanbanizeDeserializer(KanbanizeApiWrapper, DataAccess);

        public static void Main()
        {
            InsertReleasesIntoDatabaseFromApi();

            InsertWorkItemsIntoDatabaseFromApi();
        }

        private static void InsertReleasesIntoDatabaseFromApi()
        {
            var releases = DevOpsApiWrapper.GetReleaseList();

            DataAccess.InsertReleaseList(DevOpsDeserializer.Releases(releases));
        }

        private static void InsertWorkItemsIntoDatabaseFromApi()
        {
            const int enterpriseTeamBoardId = 4;
            const int assessmentsTeamBoardId = 5;

            InsertKanbanizeCards(enterpriseTeamBoardId);
            InsertKanbanizeCards(assessmentsTeamBoardId);
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

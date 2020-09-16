using System;
using System.Linq;
using DataManipulation.ApiWrapper;
using DataManipulation.DatabaseAccess;
using DataManipulation.Deserialize;
using RestSharp;

namespace KPIDataExtractor
{
    public static class Program
    {
        private static readonly IDevOpsApiWrapper DevOpsApiWrapper = new DevOpsApiWrapper(new RestClient());
        private static readonly IKanbanizeApiWrapper KanbanizeApiWrapper = new KanbanizeApiWrapper(new RestClient());
        private static readonly IDatabaseWrapper DatabaseWrapper = new DatabaseWrapper();
        private static readonly IDevOpsDeserializer DevOpsDeserializer = new DevOpsDeserializer();
        private static readonly IKanbanizeDeserializer KanbanizeDeserializer = new KanbanizeDeserializer(KanbanizeApiWrapper, DatabaseWrapper);

        public static void Main()
        {
            InsertReleasesIntoDatabaseFromApi();

            InsertWorkItemsIntoDatabaseFromApi();
        }

        private static void InsertReleasesIntoDatabaseFromApi()
        {
            var releases = DevOpsApiWrapper.GetReleaseList();

            DatabaseWrapper.InsertReleaseList(DevOpsDeserializer.Releases(releases));
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
                DatabaseWrapper.InsertWorkItemCardList(KanbanizeDeserializer.WorkItemCardList(workItemCardList, boardId));
            }
            else
                Console.WriteLine("No new cards.");
        }
    }
}

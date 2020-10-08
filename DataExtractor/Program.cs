using System;
using System.Linq;
using DataAccess.DatabaseAccess;
using DataManipulation.ApiWrapper;
using DataManipulation.Deserialize;
using RestSharp;

namespace KPIDataExtractor
{
    public static class Program
    {
        private static readonly IDevOpsApiRepository DevOpsApiRepository = new DevOpsApiRepository(new RestClient());
        private static readonly IKanbanizeApiRepository KanbanizeApiRepository = new KanbanizeApiRepository(new RestClient());
        private static readonly ReleaseRepository ReleaseRepository = new ReleaseRepository();
        private static readonly IDevOpsDeserializer DevOpsDeserializer = new DevOpsDeserializer();
        private static readonly IKanbanizeDeserializer KanbanizeDeserializer
            = new KanbanizeDeserializer(KanbanizeApiRepository, new ReleaseRepository(),
                new TaskItemRepository(), new UserRepository());
        private static readonly TaskItemRepository TaskItemRepository = new TaskItemRepository();

        public static void Main()
        {
            InsertReleasesIntoDatabaseFromApi();

            InsertWorkItemsIntoDatabaseFromApi();
        }

        private static void InsertReleasesIntoDatabaseFromApi()
        {
            var releases = DevOpsApiRepository.GetReleaseList();

            ReleaseRepository.InsertReleaseList(DevOpsDeserializer.Releases(releases));
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
            var TaskItemList = KanbanizeApiRepository.GetTaskItemList(boardId);
            if (TaskItemList.Any())
            {
                TaskItemRepository.InsertTaskItemList(KanbanizeDeserializer.TaskItemList(TaskItemList, boardId));
            }
            else
                Console.WriteLine("No new cards.");
        }
    }
}

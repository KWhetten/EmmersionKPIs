using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.ApiWrapper;
using DataAccess.DatabaseAccess;
using DataAccess.DataRepositories;
using DataAccess.Deserialize;
using DataManipulation.ApiWrapper;
using DataManipulation.DatabaseAccess;
using RestSharp;

namespace KPIDataExtractor
{
    public static class Program
    {
        private static readonly IDevOpsApiRepository DevOpsApiRepository = new DevOpsApiRepository(new RestClient());
        private static readonly IKanbanizeApiRepository KanbanizeApiRepository = new KanbanizeApiRepository(new RestClient());
        private static readonly ReleaseRepository ReleaseRepository = new ReleaseRepository(new DatabaseConnection());
        private static readonly IDevOpsDeserializer DevOpsDeserializer = new DevOpsDeserializer();
        private static readonly IKanbanizeDeserializer KanbanizeDeserializer
            = new KanbanizeDeserializer(KanbanizeApiRepository);
        private static readonly TaskItemRepository TaskItemRepository = new TaskItemRepository(new DatabaseConnection());

        public static async Task Main()
        {
            await InsertReleasesIntoDatabaseFromApiAsync();

            await InsertWorkItemsIntoDatabaseFromApiAsync();
        }

        private static async Task InsertReleasesIntoDatabaseFromApiAsync()
        {
            var releases = DevOpsApiRepository.GetReleaseList();

            await ReleaseRepository.InsertReleaseListAsync(DevOpsDeserializer.Releases(releases));
        }

        private static async Task InsertWorkItemsIntoDatabaseFromApiAsync()
        {
            const int enterpriseTeamBoardId = 4;
            const int assessmentsTeamBoardId = 5;

            await InsertKanbanizeCardsAsync(enterpriseTeamBoardId);
            await InsertKanbanizeCardsAsync(assessmentsTeamBoardId);
        }

        private static async Task InsertKanbanizeCardsAsync(int boardId)
        {
            var taskItemList = KanbanizeApiRepository.GetTaskItemList(boardId);
            if (taskItemList.Any())
            {
                await TaskItemRepository.InsertTaskItemListAsync(await KanbanizeDeserializer.TaskItemListAsync(taskItemList, boardId));
            }
            else
                Console.WriteLine("No new cards.");
        }
    }
}

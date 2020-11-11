using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Api;
using DataAccess.DataRepositories;
using DataAccess.Deserialize;
using DataAccess.Deserialize.Kanbanize;
using RestSharp;

namespace KPIDataExtractor
{
    public static class Program
    {
        private static readonly IDevOpsApiRepository DevOpsApiRepository = new DevOpsApiRepository(new RestClient());
        private static readonly IKanbanizeApiRepository KanbanizeApiRepository = new KanbanizeApi(new RestClient());
        private static readonly ReleaseRepository ReleaseRepository = new ReleaseRepository();
        private static readonly IDevOpsDeserializer DevOpsDeserializer = new DevOpsDeserializer();
        private static readonly TaskItemRepository TaskItemRepository = new TaskItemRepository();

        private static readonly IKanbanizeTaskItemDeserializer KanbanizeTaskItemDeserializer =
            new KanbanizeTaskItemDeserializer();

        public static async Task Main()
        {
            await InsertReleasesIntoDatabaseFromApiAsync();

            await InsertTaskItemsIntoDatabaseFromApiAsync();
        }

        private static async Task InsertReleasesIntoDatabaseFromApiAsync()
        {
            var releases = DevOpsApiRepository.GetReleaseList();

            await ReleaseRepository.InsertReleaseListAsync(DevOpsDeserializer.DeserializeReleases(releases));
        }

        private static async Task InsertTaskItemsIntoDatabaseFromApiAsync()
        {
            const int enterpriseTeamBoardId = 4;
            const int assessmentsTeamBoardId = 5;

            await InsertKanbanizeTaskItemsAsync(enterpriseTeamBoardId);
            await InsertKanbanizeTaskItemsAsync(assessmentsTeamBoardId);
        }

        private static async Task InsertKanbanizeTaskItemsAsync(int boardId)
        {
            var taskItemList = KanbanizeApiRepository.GetTaskItemList(boardId);
            if (taskItemList.Any())
            {
                var deserialized =
                    await KanbanizeTaskItemDeserializer.DeserializeTaskItemListAsync(taskItemList, boardId);
                await TaskItemRepository.InsertTaskItemListAsync(deserialized);
            }
            else
                Console.WriteLine("No new cards.");
        }
    }
}

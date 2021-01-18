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
        private static readonly IDevOpsApi DevOpsApi = new DevOpsApi(new RestClient());
        private static readonly IKanbanizeApi KanbanizeApi = new KanbanizeApi();
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
            var releases = DevOpsApi.GetReleaseList();

            var deserializedReleases = DevOpsDeserializer.DeserializeReleases(releases);

            await ReleaseRepository.InsertReleaseListAsync(deserializedReleases);
        }

        private static async Task InsertTaskItemsIntoDatabaseFromApiAsync()
        {
            var ids = await KanbanizeApi.GetBoardIdsAsync();

            foreach (var id in ids)
            {
                await KanbanizeApi.UpdateBoardUsersAsync(id);
                await InsertKanbanizeTaskItemsAsync(id);
            }
        }

        private static async Task InsertKanbanizeTaskItemsAsync(int boardId)
        {
            var taskItemList = await KanbanizeApi.GetTaskItemListAsync(boardId);
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

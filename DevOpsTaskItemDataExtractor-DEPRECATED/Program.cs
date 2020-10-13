using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using KPIDevOpsDataExtractor_DEPRECATED.Deserializer;
using RestSharp;
using DevOpsApiWrapper = KPIDevOpsDataExtractor_DEPRECATED.ApiWrapper.DevOpsApiWrapper;
using IDevOpsApiWrapper = KPIDevOpsDataExtractor_DEPRECATED.ApiWrapper.IDevOpsApiWrapper;

namespace KPIDevOpsDataExtractor_DEPRECATED
{
    public static class Program
    {
        private static readonly IDevOpsApiWrapper DevOpsApiWrapper = new DevOpsApiWrapper(new RestClient());
        private static readonly IDevOpsDeserializer DevOpsDeserializer = new DevOpsDeserializer(DevOpsApiWrapper, new ReleaseRepository(new DatabaseConnection()));

        public static async Task Main()
        {
            await InsertWorkItemsIntoDatabaseFromApiAsync();
        }

        private static async Task InsertWorkItemsIntoDatabaseFromApiAsync()
        {
            await InsertDevOpsCardsAsync();
        }

        private static async Task InsertDevOpsCardsAsync()
        {
            var accessTaskItemData = new TaskItemRepository(new DatabaseConnection());

            var taskItemList = await DevOpsApiWrapper.GetTaskItemList();
            if (taskItemList.Any())
            {
                await accessTaskItemData.InsertTaskItemListAsync(await DevOpsDeserializer.TaskItemListAsync(taskItemList));
            }
            else
                Console.WriteLine("No new cards.");
        }
    }
}

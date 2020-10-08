using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DatabaseAccess;
using DataManipulation.DatabaseAccess;
using KPIDevOpsDataExtractor_DEPRECATED.Deserializer;
using RestSharp;
using DevOpsApiWrapper = KPIDevOpsDataExtractor_DEPRECATED.ApiWrapper.DevOpsApiWrapper;
using IDevOpsApiWrapper = KPIDevOpsDataExtractor_DEPRECATED.ApiWrapper.IDevOpsApiWrapper;

namespace KPIDevOpsDataExtractor_DEPRECATED
{
    public static class Program
    {
        private static readonly IDevOpsApiWrapper DevOpsApiWrapper = new DevOpsApiWrapper(new RestClient());
        private static readonly IDevOpsDeserializer DevOpsDeserializer = new DevOpsDeserializer(DevOpsApiWrapper, new ReleaseRepository(), new TaskItemRepository(), new UserRepository());

        public static async Task Main()
        {
            await InsertWorkItemsIntoDatabaseFromApi();
        }

        private static async Task InsertWorkItemsIntoDatabaseFromApi()
        {
            await InsertDevOpsCards();
        }

        private static async Task InsertDevOpsCards()
        {
            var accessTaskItemData = new TaskItemRepository();

            var TaskItemList = await DevOpsApiWrapper.GetTaskItemList();
            if (TaskItemList.Any())
            {
                accessTaskItemData.InsertTaskItemList(DevOpsDeserializer.TaskItemList(TaskItemList));
            }
            else
                Console.WriteLine("No new cards.");
        }
    }
}

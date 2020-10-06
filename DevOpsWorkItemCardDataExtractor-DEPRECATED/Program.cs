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
        private static readonly IDevOpsDeserializer DevOpsDeserializer = new DevOpsDeserializer(DevOpsApiWrapper, new ReleaseDataAccess(), new WorkItemCardDataAccess(), new UserDataAccess());

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
            var accessWorkItemCardData = new WorkItemCardDataAccess();

            var workItemCardList = await DevOpsApiWrapper.GetWorkItemCardList();
            if (workItemCardList.Any())
            {
                accessWorkItemCardData.InsertWorkItemCardList(DevOpsDeserializer.WorkItemCardList(workItemCardList));
            }
            else
                Console.WriteLine("No new cards.");
        }
    }
}

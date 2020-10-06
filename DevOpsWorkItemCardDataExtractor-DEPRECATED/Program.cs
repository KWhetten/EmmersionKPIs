using System;
using System.Linq;
using System.Threading.Tasks;
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
        private static readonly IDatabaseWrapper DataAccess = new DatabaseWrapper();
        private static readonly IDevOpsDeserializer DevOpsDeserializer = new DevOpsDeserializer(DevOpsApiWrapper, DataAccess);

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
            var workItemCardList = await DevOpsApiWrapper.GetWorkItemCardList();
            if (workItemCardList.Any())
            {
                DataAccess.InsertWorkItemCardList(DevOpsDeserializer.WorkItemCardList(workItemCardList));
            }
            else
                Console.WriteLine("No new cards.");
        }
    }
}

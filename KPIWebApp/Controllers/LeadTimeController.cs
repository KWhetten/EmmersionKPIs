using System;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("lead-time")]
    public class LeadTimeController : ControllerBase
    {
        [HttpGet]
        public async Task<ScatterPlotData[]> Get(string startDateString, string finishDateString)
        {
            DateTime startDate;
            DateTime finishDate;
            try
            {
                startDate = DateTime.Parse(startDateString);
                finishDate = DateTime.Parse(finishDateString);
            }
            catch (Exception ex)
            {
                startDate = new DateTime(2015, 1, 1);
                finishDate = DateTime.Now;
            }

            var taskItemRepository = new TaskItemRepository(new DatabaseConnection());
            var rawData = (await taskItemRepository.GetTaskItemListAsync(startDate, finishDate)).ToArray();

            var cardTypes = await taskItemRepository.GetTaskItemTypesAsync();

            var cardsByType = GetCardsByType(cardTypes, rawData);

            return cardsByType;
        }

        private static ScatterPlotData[] GetCardsByType(TaskItemType[] cardTypes, TaskItem[] rawData)
        {
            var leadTimeDataByType = EstablishScatterPlotDataStructure(cardTypes);

            return PopulateScatterPlotLeadTimeInfo(rawData, leadTimeDataByType);
        }

        private static ScatterPlotData[] PopulateScatterPlotLeadTimeInfo(TaskItem[] rawData, ScatterPlotData[] scatterPlotData)
        {
            foreach (var datum in rawData)
            {
                var typeIndex = (int) datum.Type;
                var currentData = scatterPlotData[typeIndex].data;
                var newData = new LeadTimeData[currentData.Length + 1];
                for (var i = 0; i < currentData.Length; ++i)
                {
                    newData[i] = currentData[i];
                }

                newData[currentData.Length] = new LeadTimeData
                {
                    finishTime = datum.FinishTime,
                    leadTime = (datum.FinishTime - datum.StartTime).TotalHours / 24
                };
                scatterPlotData[typeIndex].data = newData;
            }

            return scatterPlotData;
        }

        private static ScatterPlotData[] EstablishScatterPlotDataStructure(TaskItemType[] cardTypes)
        {
            var scatterPlotDataStructure = new ScatterPlotData[cardTypes.Length];
            for (var i = 0; i < cardTypes.Length; i++)
            {
                scatterPlotDataStructure[i] = new ScatterPlotData
                {
                    name = cardTypes[i].ToString(),
                    turboThreshold = 500000,
                    data = new LeadTimeData[0]
                };
            }

            return scatterPlotDataStructure;
        }
    }

    public class LeadTimeData
    {
        public DateTime finishTime;
        public double leadTime;
    }
}

public class ScatterPlotData
{
    public string name { get; set; }
    public int turboThreshold { get; set; }

    public LeadTimeData[] data { get; set; }
}

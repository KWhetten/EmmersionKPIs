using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;

namespace KPIWebApp.Helpers
{
    public class ScatterPlotHelper
    {
        private readonly IReleaseRepository releaseRepository;
        private readonly ITaskItemRepository taskItemRepository;

        public ScatterPlotHelper()
        {
            taskItemRepository = new TaskItemRepository(new DatabaseConnection());
        }
        public ScatterPlotHelper(ITaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
        }

        public ScatterPlotHelper(IReleaseRepository releaseRepository)
        {
            this.releaseRepository = releaseRepository;
        }

        public async Task<ScatterPlotData[]> GetLeadTimeScatterPlotData(DateTime startDate, DateTime finishDate)
        {
            var rawData = (await taskItemRepository.GetTaskItemListAsync(startDate, finishDate)).ToArray();

            var cardTypes = await taskItemRepository.GetTaskItemTypesAsync();

            return GetCardsByType(cardTypes, rawData);
        }

        public async Task<ScatterPlotData[]> GetReleaseScatterPlotData(DateTime startDate, DateTime finishDate)
        {
            var rawData = (await releaseRepository.GetReleaseListAsync(startDate, finishDate)).ToArray();

            return new ScatterPlotData[1];
        }

        private ScatterPlotData[] GetCardsByType(TaskItemType[] cardTypes, TaskItem[] rawData)
        {
            var leadTimeDataByType = EstablishScatterPlotDataStructure(cardTypes);

            return PopulateScatterPlotLeadTimeInfo(rawData, leadTimeDataByType);
        }

        private ScatterPlotData[] PopulateScatterPlotLeadTimeInfo(TaskItem[] rawData, ScatterPlotData[] scatterPlotData)
        {
            foreach (var datum in rawData)
            {
                var typeIndex = (int) datum.Type - 1;

                var newData = new Datum
                {
                    x = datum.FinishTime,
                    y = datum.CalculateLeadTimeHours()
                };
                scatterPlotData[typeIndex].data.Add(newData);
            }

            return scatterPlotData;
        }

        private ScatterPlotData[] EstablishScatterPlotDataStructure(TaskItemType[] cardTypes)
        {
            var scatterPlotDataStructure = new ScatterPlotData[cardTypes.Length];
            for (var i = 0; i < cardTypes.Length; i++)
            {
                scatterPlotDataStructure[i] = new ScatterPlotData
                {
                    name = cardTypes[i].ToString(),
                    turboThreshold = 500000,
                    data = new List<Datum>()
                };
            }

            return scatterPlotDataStructure;
        }
    }

    public class Datum
    {
        public DateTime x { get; set; }
        public decimal y { get; set; }
    }
}

public class ScatterPlotData
{
    public string name { get; set; }
    public int turboThreshold { get; set; }
    public List<Datum> data { get; set; }
}

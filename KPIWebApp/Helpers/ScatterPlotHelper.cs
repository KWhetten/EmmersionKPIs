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
            taskItemRepository = new TaskItemRepository();
            releaseRepository = new ReleaseRepository();
        }
        public ScatterPlotHelper(ITaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
        }

        public ScatterPlotHelper(IReleaseRepository releaseRepository)
        {
            this.releaseRepository = releaseRepository;
        }

        public ScatterPlotData[] GetLeadTimeScatterPlotData(DateTimeOffset startDate, DateTimeOffset finishDate)
        {
            var rawData = taskItemRepository.GetTaskItemList(startDate, finishDate);

            var cardTypes = taskItemRepository.GetTaskItemTypes();

            return GetCardsByType(cardTypes, rawData.ToArray());
        }

        public async Task<ScatterPlotData[]> GetReleaseScatterPlotData(DateTimeOffset startDate, DateTimeOffset finishDate)
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
                if (newData.x < new DateTimeOffset(DateTime.Now.AddYears(-1)))
                {
                    continue;
                }
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
        public DateTimeOffset? x { get; set; }
        public decimal y { get; set; }
    }
}

public class ScatterPlotData
{
    public string name { get; set; }
    public int turboThreshold { get; set; }
    public List<Datum> data { get; set; }
}

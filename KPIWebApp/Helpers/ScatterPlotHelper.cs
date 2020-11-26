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
        public bool Product { get; set; }
        public bool Engineering { get; set; }
        public bool Unanticipated { get; set; }

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

        public async Task<Dictionary<TaskItemType, ScatterPlotData>> GetLeadTimeScatterPlotData(DateTimeOffset startDate, DateTimeOffset finishDate, bool product, bool engineering, bool unanticipated)
        {
            Product = product;
            Engineering = engineering;
            Unanticipated = unanticipated;
            var rawData = await taskItemRepository.GetTaskItemListAsync(startDate, finishDate);

            var taskItemTypes = GetTaskItemTypes();

            return GetCardsByType(taskItemTypes, rawData.ToArray());
        }

        private List<TaskItemType> GetTaskItemTypes()
        {
            var cardTypes = new List<TaskItemType>();
            if (Product)
            {
                cardTypes.Add(TaskItemType.Product);
            }

            if (Engineering)
            {
                cardTypes.Add(TaskItemType.Engineering);
            }

            if (Unanticipated)
            {
                cardTypes.Add(TaskItemType.Unanticipated);
            }

            return cardTypes;
        }

        private Dictionary<TaskItemType, ScatterPlotData> GetCardsByType(List<TaskItemType> cardTypes, TaskItem[] rawData)
        {
            var leadTimeDataByType = EstablishScatterPlotDataStructure(cardTypes);

            return PopulateScatterPlotLeadTimeInfo(rawData, leadTimeDataByType);
        }

        private Dictionary<TaskItemType, ScatterPlotData> PopulateScatterPlotLeadTimeInfo(TaskItem[] rawData, Dictionary<TaskItemType, ScatterPlotData> scatterPlotData)
        {
            foreach (var datum in rawData)
            {
                try
                {
                    if ((datum.Type == TaskItemType.Product && Product)
                        || (datum.Type == TaskItemType.Engineering && Engineering)
                        || (datum.Type == TaskItemType.Unanticipated && Unanticipated))
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

                        scatterPlotData[datum.Type].data.Add(newData);
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            return scatterPlotData;
        }

        private Dictionary<TaskItemType, ScatterPlotData> EstablishScatterPlotDataStructure(List<TaskItemType> cardTypes)
        {
            var scatterPlotDataStructure = new Dictionary<TaskItemType, ScatterPlotData>();
            foreach (var cardType in cardTypes)
            {
                scatterPlotDataStructure[cardType] = new ScatterPlotData
                {
                    name = cardType.ToString(),
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

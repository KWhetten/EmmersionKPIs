using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DatabaseAccess;
using DataAccess.DataRepositories;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;
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

            var taskItemDataAccess = new TaskItemRepository(new DatabaseConnection());
            var rawData = await taskItemDataAccess.GetTaskItemListAsync(startDate, finishDate);
            var cardTypes = new List<string>();

            foreach (var datum in rawData.Where(datum => !cardTypes.Contains(datum.Type.ToString())))
            {
                cardTypes.Add(datum.Type.ToString());
            }

            var cardsByType = GetCardsByType(cardTypes, rawData).ToArray();

            var scatterPlotData = new ScatterPlotData[cardTypes.Count];

            for (var i = 0; i < scatterPlotData.Length; ++i)
            {
                scatterPlotData[i] = new ScatterPlotData
                {
                    name = cardsByType[i].ElementAt(i).Type.ToString(),
                    turboThreshold = 500000
                };
                var data = new LeadTimeData[scatterPlotData[i].data.Length];

                foreach (var card in cardsByType[i])
                {
                    var leadTime = (decimal) (card.FinishTime - card.StartTime).TotalHours / 24m;
                    var datum = new LeadTimeData
                    {
                        finishTime = card.FinishTime,
                        leadTime = leadTime
                    };
                    data[i] = datum;
                }

                scatterPlotData[i].data = data;
            }

            return scatterPlotData;
        }

        private static List<List<TaskItem>> GetCardsByType(List<string> cardTypes, List<TaskItem> rawData)
        {
            var cardsByType = new List<List<TaskItem>>();
            var i = 0;
            foreach (var type in cardTypes)
            {
                cardsByType.Add(new List<TaskItem>());

                foreach (var datum in rawData.Where(datum => datum.Type.ToString() == type))
                {
                    cardsByType.ElementAt(i).Add(datum);
                }

                ++i;
            }

            return cardsByType;
        }
    }

    public class LeadTimeData
    {
        public DateTime finishTime;
        public decimal leadTime;
    }
}

public class ScatterPlotData
{
    public string name { get; set; }
    public int turboThreshold { get; set; }

    public LeadTimeData[] data { get; set; }
}

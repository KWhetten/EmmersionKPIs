using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;
using Accord.MachineLearning;
using Accord.Math.Distances;
using DataAccess.Objects;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("cluster-analysis")]
    public class TaskItemClusterAnalysis
    {
        private readonly List<string> users = new List<string>();

        [HttpGet]
        public async Task Get()
        {
            var startDate = new DateTimeOffset(new DateTime(2015, 1, 1), DateTimeOffset.Now.Offset);
            var finishDate = new DateTimeOffset(DateTime.Now, DateTimeOffset.Now.Offset);

            var taskItemClusterAnalysisHelper = new TaskItemClusterAnalysisHelper();
            var taskItems = (await taskItemClusterAnalysisHelper.GetTaskItems(startDate, finishDate));

            var taskItemIntArrays = new long[taskItems.Count][];

            var i = 0;
            foreach (var taskItem in taskItems)
            {
                taskItemIntArrays[i] = GetIntArrayFromTaskItem(taskItem);
                i++;
            }

            var kmodes = new KModes<long>(4, new Hamming<long>())
            {
                MaxIterations = 100
            };

            // Compute and retrieve the data centroids
            var clusters = kmodes.Learn(taskItemIntArrays);

            // Use the centroids to partition all the data
            var labels = clusters.Decide(taskItemIntArrays);

            Console.WriteLine("Id,Title,StartTime,FinishTime,Type,DevelopmentTeamName,CreatedOn,CreatedBy,LastChangedOn,LastChangedBy,CurrentBoardColumn,State,NumRevisions,ReleaseId");
            i = -1;
            Console.WriteLine("CLUSTER 1");
            foreach (var label in labels)
            {
                ++i;
                if (label != 0) continue;
                var task = taskItems.ElementAt(i);
                Console.WriteLine($"{task.Id},{task.Title},{task.StartTime},{task.FinishTime},{task.Type.ToString()},{task.DevelopmentTeam},{task.CreatedOn},{task.CreatedBy},{task.LastChangedOn},{task.LastChangedBy},{task.CurrentBoardColumn},{task.State},{task.NumRevisions},{task.Release.Id}");
            }
            i = -1;
            Console.WriteLine("CLUSTER 2");
            foreach (var label in labels)
            {
                ++i;
                if (label != 1) continue;
                var task = taskItems.ElementAt(i);
                Console.WriteLine($"{task.Id},{task.Title},{task.StartTime},{task.FinishTime},{task.Type.ToString()},{task.DevelopmentTeam},{task.CreatedOn},{task.CreatedBy},{task.LastChangedOn},{task.LastChangedBy},{task.CurrentBoardColumn},{task.State},{task.NumRevisions},{task.Release.Id}");
            }
            i = -1;
            Console.WriteLine("CLUSTER 3");
            foreach (var label in labels)
            {
                ++i;
                if (label != 2) continue;
                var task = taskItems.ElementAt(i);
                Console.WriteLine($"{task.Id},{task.Title},{task.StartTime},{task.FinishTime},{task.Type.ToString()},{task.DevelopmentTeam},{task.CreatedOn},{task.CreatedBy},{task.LastChangedOn},{task.LastChangedBy},{task.CurrentBoardColumn},{task.State},{task.NumRevisions},{task.Release.Id}");
            }
            i = -1;
            Console.WriteLine("CLUSTER 4");
            foreach (var label in labels)
            {
                ++i;
                if (label != 3) continue;
                var task = taskItems.ElementAt(i);
                Console.WriteLine($"{task.Id},{task.Title},{task.StartTime},{task.FinishTime},{task.Type.ToString()},{task.DevelopmentTeam},{task.CreatedOn},{task.CreatedBy},{task.LastChangedOn},{task.LastChangedBy},{task.CurrentBoardColumn},{task.State},{task.NumRevisions},{task.Release.Id}");
            }
            Console.Write("");
        }

        private long[] GetIntArrayFromTaskItem(TaskItem taskItem)
        {
            var startTime = GetDateTimeInt(taskItem.StartTime);
            var finishTime = GetDateTimeInt(taskItem.FinishTime);
            var type = (int) taskItem.Type;
            var developmentTeamName = GetDevelopmentTeamNameInt(taskItem.DevelopmentTeam);
            var createdOn = GetDateTimeInt(taskItem.CreatedOn);
            var createdBy = GetUserInt(taskItem.CreatedBy);
            var lastChangedOn = GetDateTimeInt(taskItem.LastChangedOn);
            var lastChangedBy = GetUserInt(taskItem.LastChangedBy);
            var currentBoardColumn = GetCurrentBoardColumnInt(taskItem.CurrentBoardColumn);
            var state = (int) taskItem.State;
            var numRevisions = taskItem.NumRevisions;
            var releaseId = taskItem.Release.Id;

            return new[]
            {
                startTime, finishTime, type, developmentTeamName, createdOn, createdBy, lastChangedOn,
                lastChangedBy, currentBoardColumn, state, numRevisions, releaseId
            };
        }

        private long GetStateInt(string taskItemState)
        {
            return taskItemState switch
            {
                "Backlog" => 0,
                "Top Priority" => 1,
                "In Process" => 2,
                "Released" => 3,
                _ => -1
            };
        }

        private long GetCurrentBoardColumnInt(string currentBoardColumn)
        {
            return currentBoardColumn switch
            {
                "Backlog" => 0,
                "Engineering Backlog" => 0,
                "Engineering" => 0,
                "Product Backlog" => 0,
                "Product" => 0,
                "Top Priority" => 1,
                "In Process.Working" => 2,
                "In Process" => 2,
                "Working" => 2,
                "Ready for Prod Deploy" => 3,
                "In Process.Ready for Prod Deploy" => 3,
                "Released to Prod this week" => 4,
                "Ready to Archive" => 5,
                "Archive" => 6,
                _ => -1
            };
        }

        private long GetUserInt(string userName)
        {
            if (!users.Contains(userName))
            {
                users.Add(userName);
            }

            return users.IndexOf(userName);
        }

        private long GetDevelopmentTeamNameInt(string taskItemDevelopmentTeamName)
        {
            return taskItemDevelopmentTeamName switch
            {
                "Assessments Team" => 0,
                "Enterprise Team" => 1,
                _ => -1
            };
        }

        private long GetDateTimeInt(DateTimeOffset? givenTime)
        {
            if (givenTime == null) return 0;

            var time = givenTime.Value;
            var intString = $"{time.Year}" +
                            $"{(time.Month < 10 ? "0" + time.Month : time.Month.ToString())}" +
                            $"{(time.Day < 10 ? "0" + time.Day : time.Day.ToString())}" +
                            $"{(time.Hour < 10 ? "0" + time.Hour : time.Hour.ToString())}" +
                            $"{(time.Minute < 10 ? "0" + time.Minute : time.Minute.ToString())}" +
                            $"{(time.Second < 10 ? "0" + time.Second : time.Second.ToString())}";
            return long.Parse(intString);
        }
    }
}

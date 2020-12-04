using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Fitting;
using DataAccess.DataRepositories;
using DataAccess.Objects;

namespace KPIWebApp.Helpers
{
    public class MultinomialLogisticRegressionAnalysisHelper
    {
        public async Task<MultinomialLogisticRegressionAnalysisItemList> GetLogisticRegressionAnalysisData(
            DateTimeOffset? startDate, DateTimeOffset? finishDate)
        {
            var logisticRegressionData = new MultinomialLogisticRegressionAnalysisItemList();

            var taskItemRepository = new TaskItemRepository();
            var taskItemList = await taskItemRepository.GetTaskItemListAsync(startDate, finishDate);

            foreach (var taskItem in taskItemList)
            {
                if (!logisticRegressionData.Users.Contains(taskItem.CreatedBy))
                {
                    logisticRegressionData.Users.Add(taskItem.CreatedBy);
                }

                if (!logisticRegressionData.Users.Contains(taskItem.LastChangedBy))
                {
                    logisticRegressionData.Users.Add(taskItem.LastChangedBy);
                }
            }

            var inputs = new List<List<double>>();
            var outputList = new List<int>();
            var ids = new List<int>();
            var titles = new List<string>();

            foreach (var logisticRegressionTaskItem
                in from taskItem in taskItemList
                where taskItem.StartTime != null
                      && taskItem.FinishTime != null
                select GetLogisticRegressionTaskItem(taskItem))
            {
                ids.Add(logisticRegressionTaskItem.Id);
                titles.Add(logisticRegressionTaskItem.Title);
                inputs.Add(new List<double>
                {
                    logisticRegressionTaskItem.Lifetime.TotalDays,
                    logisticRegressionTaskItem.LeadTime.TotalDays,
                    logisticRegressionTaskItem.TimeSpentInBacklog.TotalDays,
                    (logisticRegressionTaskItem.DevTeamIsAssessments ? 1.0 : 0.0),
                    (logisticRegressionTaskItem.DevTeamIsEnterprise ? 1.0 : 0.0),
                    logisticRegressionTaskItem.NumRevisions
                });

                foreach (var user in logisticRegressionData.Users)
                {
                    inputs.Last().Add(logisticRegressionTaskItem.CreatedBy == user ? 1.0 : 0.0);
                }

                foreach (var user in logisticRegressionData.Users)
                {
                    inputs.Last().Add(logisticRegressionTaskItem.LastChangedBy == user ? 1.0 : 0.0);
                }

                outputList.Add((int) logisticRegressionTaskItem.TaskItemType);
            }

            var inputArray = inputs.Select(inputList => inputList.ToArray()).ToArray();
            var outputArray = outputList.ToArray();

            var lbnr = new LowerBoundNewtonRaphson()
            {
                MaxIterations = 100,
                Tolerance = 1e-6
            };

            var mlr = lbnr.Learn(inputArray, outputArray);

            var answers = mlr.Decide(inputArray);

            var probabilities = mlr.Probabilities(inputArray);

            logisticRegressionData.Error = new ZeroOneLoss(outputArray).Loss(answers);

            for (var i = 0; i < ids.Count; i++)
            {
                var probability = probabilities[i].Max();

                var logisticRegressionItem = new MultinomialLogisticRegressionAnalysisItem
                {
                    Id = ids[i],
                    Inputs = inputs[i],
                    Title = titles[i],
                    Output = outputArray[i],
                    Prediction = answers[i],
                    Probability = probability
                };

                if (logisticRegressionItem.Output != logisticRegressionItem.Prediction)
                {
                    logisticRegressionData.Items.Add(logisticRegressionItem);
                }
            }

            return logisticRegressionData;
        }

        private LogisticRegressionTaskItem GetLogisticRegressionTaskItem(TaskItem taskItem)
        {
            return new LogisticRegressionTaskItem
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Lifetime = (taskItem.LastChangedOn - taskItem.CreatedOn).GetValueOrDefault(),
                LeadTime = (taskItem.FinishTime - taskItem.StartTime).GetValueOrDefault(),
                TimeSpentInBacklog = (taskItem.StartTime - taskItem.CreatedOn).GetValueOrDefault(),
                TaskItemType = taskItem.Type,
                DevTeamIsAssessments = taskItem.DevelopmentTeam == "Assessments Team",
                DevTeamIsEnterprise = taskItem.DevelopmentTeam == "Enterprise Team",
                NumRevisions = taskItem.NumRevisions,
                CreatedBy = taskItem.CreatedBy,
                LastChangedBy = taskItem.LastChangedBy
            };
        }
    }
}

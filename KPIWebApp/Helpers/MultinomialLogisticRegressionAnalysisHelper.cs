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
            DateTimeOffset? startDate, DateTimeOffset? finishDate,
            bool product, bool engineering, bool unanticipated, bool assessmentsTeam, bool enterpriseTeam)
        {
            var logisticRegressionData = new MultinomialLogisticRegressionAnalysisItemList();

            var taskItemRepository = new TaskItemRepository();
            var taskItemList = await taskItemRepository.GetTaskItemListAsync(startDate, finishDate);

            logisticRegressionData.UserIds = GetUserIds(taskItemList);

            var inputs = new List<List<double>>();
            var outputList = new List<int>();
            var ids = new List<int>();
            var titles = new List<string>();
            var taskItemHelper = new TaskItemHelper();

            foreach (var logisticRegressionTaskItem
                in from taskItem in taskItemList
                where taskItem.StartTime != null
                      && taskItem.FinishTime != null
                      && taskItemHelper.TaskItemDevTeamIsSelected(assessmentsTeam, enterpriseTeam, taskItem)
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

                foreach (var user in logisticRegressionData.UserIds)
                {
                    inputs.Last().Add(logisticRegressionTaskItem.CreatedById == user ? 1.0 : 0.0);
                }

                foreach (var user in logisticRegressionData.UserIds)
                {
                    inputs.Last().Add(logisticRegressionTaskItem.LastChangedBy.Id == user ? 1.0 : 0.0);
                }

                outputList.Add((int) logisticRegressionTaskItem.TaskItemType);
            }

            var inputArray = inputs.Select(inputList => inputList.ToArray()).ToArray();
            var actualResults = outputList.ToArray();

            var lbnr = new LowerBoundNewtonRaphson()
            {
                MaxIterations = 100,
                Tolerance = 1e-6
            };

            var mlr = lbnr.Learn(inputArray, actualResults);

            var predictions = mlr.Decide(inputArray);

            var probabilities = mlr.Probabilities(inputArray);

            logisticRegressionData.Error = new ZeroOneLoss(actualResults).Loss(predictions);

            for (var i = 0; i < ids.Count; i++)
            {
                if (taskItemHelper.TaskItemTypeIsSelected(product, engineering, unanticipated, actualResults[i]))
                {
                    var probability = probabilities[i].Max();

                    var logisticRegressionItem = new MultinomialLogisticRegressionAnalysisItem
                    {
                        Id = ids[i],
                        Inputs = inputs[i],
                        Title = titles[i],
                        Actual = actualResults[i],
                        Prediction = predictions[i],
                        Probability = probability
                    };

                    if (logisticRegressionItem.Actual != logisticRegressionItem.Prediction)
                    {
                        logisticRegressionData.Items.Add(logisticRegressionItem);
                    }
                }
            }

            return logisticRegressionData;
        }

        private static List<int> GetUserIds(List<TaskItem> taskItemList)
        {
            var ids = new List<int>();
            foreach (var taskItem in taskItemList)
            {
                if (!ids.Contains(taskItem.CreatedBy.Id))
                {
                    ids.Add(taskItem.CreatedBy.Id);
                }

                if (!ids.Contains(taskItem.LastChangedBy.Id))
                {
                    ids.Add(taskItem.LastChangedBy.Id);
                }
            }

            return ids;
        }

        private RegressionAnalysisTaskItem GetLogisticRegressionTaskItem(TaskItem taskItem)
        {
            return new RegressionAnalysisTaskItem
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Lifetime = (taskItem.LastChangedOn - taskItem.CreatedOn).GetValueOrDefault(),
                LeadTime = (taskItem.FinishTime - taskItem.StartTime).GetValueOrDefault(),
                TimeSpentInBacklog = (taskItem.StartTime - taskItem.CreatedOn).GetValueOrDefault(),
                TaskItemType = taskItem.Type,
                DevTeamIsAssessments = taskItem.DevelopmentTeam.Name == "Assessments",
                DevTeamIsEnterprise = taskItem.DevelopmentTeam.Name == "Enterprise",
                NumRevisions = taskItem.NumRevisions,
                CreatedById = taskItem.CreatedBy.Id,
                LastChangedBy = taskItem.LastChangedBy
            };
        }
    }
}

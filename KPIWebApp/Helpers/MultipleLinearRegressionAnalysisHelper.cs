using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Linear;
using DataAccess.DataRepositories;
using DataAccess.Objects;

namespace KPIWebApp.Helpers
{
    public class MultipleLinearRegressionAnalysisHelper
    {
        public async Task<string> GetEstimation(MultipleLinearRegressionTaskItem item)
        {
            var multipleLinearRegressionAnalysisData = await GetMultipleLinearRegressionAnalysisData(item);

            var ols = new OrdinaryLeastSquares()
            {
                UseIntercept = true
            };

            var regression = ols.Learn(multipleLinearRegressionAnalysisData.Inputs,
                multipleLinearRegressionAnalysisData.Outputs);

            var backlogTimeCoefficient = regression.Weights[0];
            var productCoefficient = regression.Weights[1];
            var engineeringCoefficient = regression.Weights[2];
            var unanticipatedCoefficient = regression.Weights[3];
            var assessmentsTeamCoefficient = regression.Weights[4];
            var enterpriseTeamCoefficient = regression.Weights[5];

            multipleLinearRegressionAnalysisData.Predicted = regression.Transform(multipleLinearRegressionAnalysisData.Inputs);

            multipleLinearRegressionAnalysisData.Error =
                new SquareLoss(multipleLinearRegressionAnalysisData.Outputs).Loss(
                    multipleLinearRegressionAnalysisData.Predicted);

            multipleLinearRegressionAnalysisData.R2 = regression.CoefficientOfDetermination(
                multipleLinearRegressionAnalysisData.Inputs, multipleLinearRegressionAnalysisData.Outputs, adjust: false);

            return (multipleLinearRegressionAnalysisData.Predicted.Last() * 8).ToString("F2");
        }

        private async Task<MultipleLinearRegressionAnalysisData> GetMultipleLinearRegressionAnalysisData(MultipleLinearRegressionTaskItem item)
        {
            var multipleLinearRegressionAnalysisData = new MultipleLinearRegressionAnalysisData();

            var taskItemRepository = new TaskItemRepository();
            var taskItemList =
                await taskItemRepository.GetTaskItemListAsync(new DateTimeOffset(new DateTime(2020, 1, 1)), DateTimeOffset.Now);

            foreach (var taskItem in taskItemList.Where(taskItem =>
                !multipleLinearRegressionAnalysisData.Users.Contains(taskItem.CreatedBy)))
            {
                multipleLinearRegressionAnalysisData.Users.Add(taskItem.CreatedBy);
            }

            var inputs = new List<List<double>>();
            var outputList = new List<double>();

            foreach (var logisticRegressionTaskItem
                in from taskItem in taskItemList
                where taskItem.StartTime != null
                      && taskItem.FinishTime != null
                select GetLogisticRegressionTaskItem(taskItem))
            {
                multipleLinearRegressionAnalysisData.Ids.Add(logisticRegressionTaskItem.Id);
                inputs.Add(new List<double>
                {
                    logisticRegressionTaskItem.TimeSpentInBacklog.TotalDays,
                    (logisticRegressionTaskItem.DevTeamIsAssessments ? 1.0 : 0.0),
                    (logisticRegressionTaskItem.DevTeamIsEnterprise ? 1.0 : 0.0)
                });

                foreach (var user in multipleLinearRegressionAnalysisData.Users)
                {
                    inputs.Last().Add(logisticRegressionTaskItem.CreatedBy == user ? 1.0 : 0.0);
                }

                outputList.Add(logisticRegressionTaskItem.LeadTime.TotalDays);
            }

            var itemInput = new List<double>
            {
                item.TimeSpentInBacklog,
                item.TypeIsProduct ? 1.0 : 0.0,
                item.TypeIsEngineering ? 1.0 : 0.0,
                item.TypeIsUnanticipated ? 1.0 : 0.0,
                item.DevTeamIsAssessments ? 1.0 : 0.0,
                item.DevTeamIsEnterprise ? 1.0 : 0.0
            };
            itemInput.AddRange(multipleLinearRegressionAnalysisData.Users.Select(user => user == item.CreatedBy ? 1.0 : 0.0));

            inputs.Add(itemInput);
            outputList.Add(-1.0);
            multipleLinearRegressionAnalysisData.Ids.Add(0);

            multipleLinearRegressionAnalysisData.Inputs = inputs.Select(input => input.ToArray()).ToArray();
            multipleLinearRegressionAnalysisData.Outputs = outputList.ToArray();
            return multipleLinearRegressionAnalysisData;
        }

        private LogisticRegressionTaskItem GetLogisticRegressionTaskItem(TaskItem taskItem)
        {
            return new LogisticRegressionTaskItem
            {
                Id = taskItem.Id,
                Lifetime = (taskItem.LastChangedOn - taskItem.CreatedOn).GetValueOrDefault(),
                LeadTime = (taskItem.FinishTime - taskItem.StartTime).GetValueOrDefault(),
                TimeSpentInBacklog = (taskItem.StartTime - taskItem.CreatedOn).GetValueOrDefault(),
                TaskItemType = taskItem.Type,
                DevTeamIsAssessments = taskItem.DevelopmentTeam == "Assessments Team",
                DevTeamIsEnterprise = taskItem.DevelopmentTeam == "Enterprise Team",
                NumRevisions = taskItem.NumRevisions,
                CreatedBy = taskItem.CreatedBy
            };
        }
    }
}

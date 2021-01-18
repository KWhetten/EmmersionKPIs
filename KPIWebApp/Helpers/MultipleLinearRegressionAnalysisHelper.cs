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
        private TaskItemRepository taskItemRepository;

        public MultipleLinearRegressionAnalysisHelper()
        {
            taskItemRepository = new TaskItemRepository();
        }

        public MultipleLinearRegressionAnalysisHelper(TaskItemRepository taskItemRepository)
        {
            this.taskItemRepository = taskItemRepository;
        }

        public async Task<string> GetEstimation(MultipleLinearRegressionTaskItem item)
        {
            var multipleLinearRegressionAnalysisData = await GetMultipleLinearRegressionAnalysisData(item);

            var ols = new OrdinaryLeastSquares()
            {
                UseIntercept = true,
                IsRobust = true
            };

            var regression = ols.Learn(multipleLinearRegressionAnalysisData.Inputs,
                multipleLinearRegressionAnalysisData.Outputs);

            multipleLinearRegressionAnalysisData.Predicted = regression.Transform(multipleLinearRegressionAnalysisData.Inputs);

            multipleLinearRegressionAnalysisData.Error =
                new SquareLoss(multipleLinearRegressionAnalysisData.Outputs).Loss(
                    multipleLinearRegressionAnalysisData.Predicted);

            multipleLinearRegressionAnalysisData.R2 = regression.CoefficientOfDetermination(
                multipleLinearRegressionAnalysisData.Inputs, multipleLinearRegressionAnalysisData.Outputs, adjust: false);

            return (multipleLinearRegressionAnalysisData.Predicted.Last() * 8).ToString("F2");
        }

        public async Task<MultipleLinearRegressionAnalysisData> GetMultipleLinearRegressionAnalysisData(MultipleLinearRegressionTaskItem item)
        {
            var multipleLinearRegressionAnalysisData = new MultipleLinearRegressionAnalysisData();

            var taskItemList =
                await taskItemRepository.GetTaskItemListAsync(new DateTimeOffset(new DateTime(2020, 1, 1)), DateTimeOffset.Now);

            foreach (var taskItem in taskItemList.Where(taskItem =>
                !multipleLinearRegressionAnalysisData.UserIds.Contains(taskItem.CreatedBy.Id)))
            {
                multipleLinearRegressionAnalysisData.UserIds.Add(taskItem.CreatedBy.Id);
            }

            var inputs = new List<List<double>>();
            var outputList = new List<double>();

            foreach (var logisticRegressionTaskItem
                in from taskItem in taskItemList
                where taskItem.StartTime != null
                      && taskItem.FinishTime != null
                select GetRegressionAnalysisTaskItem(taskItem))
            {
                multipleLinearRegressionAnalysisData.Ids.Add(logisticRegressionTaskItem.Id);
                inputs.Add(new List<double>
                {
                    logisticRegressionTaskItem.TimeSpentInBacklog.TotalDays,
                    logisticRegressionTaskItem.TypeIsProduct ? 1.0 : 0.0,
                    logisticRegressionTaskItem.TypeIsEngineering ? 1.0 : 0.0,
                    logisticRegressionTaskItem.TypeIsUnanticipated ? 1.0 : 0.0,
                    (logisticRegressionTaskItem.DevTeamIsAssessments ? 1.0 : 0.0),
                    (logisticRegressionTaskItem.DevTeamIsEnterprise ? 1.0 : 0.0)
                });

                foreach (var userId in multipleLinearRegressionAnalysisData.UserIds)
                {
                    inputs.Last().Add(logisticRegressionTaskItem.CreatedById == userId ? 1.0 : 0.0);
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

            foreach (var userId in multipleLinearRegressionAnalysisData.UserIds)
            {
                itemInput.Add(item.CreatedBy.Id == userId ? 1.0 : 0.0);
            }

            inputs.Add(itemInput);
            outputList.Add(-1.0);
            multipleLinearRegressionAnalysisData.Ids.Add(0);

            multipleLinearRegressionAnalysisData.Inputs = inputs.Select(input => input.ToArray()).ToArray();
            multipleLinearRegressionAnalysisData.Outputs = outputList.ToArray();
            return multipleLinearRegressionAnalysisData;
        }

        private RegressionAnalysisTaskItem GetRegressionAnalysisTaskItem(TaskItem taskItem)
        {
            return new RegressionAnalysisTaskItem
            {
                Id = taskItem.Id,
                Lifetime = (taskItem.LastChangedOn - taskItem.CreatedOn).GetValueOrDefault(),
                LeadTime = (taskItem.FinishTime - taskItem.StartTime).GetValueOrDefault(),
                TimeSpentInBacklog = (taskItem.StartTime - taskItem.CreatedOn).GetValueOrDefault(),
                TypeIsProduct = taskItem.Type == TaskItemType.Product,
                TypeIsEngineering = taskItem.Type == TaskItemType.Engineering,
                TypeIsUnanticipated = taskItem.Type == TaskItemType.Unanticipated,
                DevTeamIsAssessments = taskItem.DevelopmentTeam.Name == "Assessments",
                DevTeamIsEnterprise = taskItem.DevelopmentTeam.Name == "Enterprise",
                NumRevisions = taskItem.NumRevisions,
                CreatedById = taskItem.CreatedBy.Id
            };
        }
    }
}

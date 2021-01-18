using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Deserialize.Kanbanize;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("multiple-linear-regression-analysis")]
    public class MultipleLinearRegressionAnalysisController
    {
        [HttpGet]
        public async Task<string> GetAsync(double timeSpentInBacklog, string type, string devTeam, string createdBy)
        {
            var helper = new MultipleLinearRegressionAnalysisHelper();
            var developerRepository = new DeveloperRepository();
            var taskItemType = GetTaskItemType(type);

            var taskItem = new MultipleLinearRegressionTaskItem
            {
                TimeSpentInBacklog = timeSpentInBacklog,

                TypeIsProduct = taskItemType == TaskItemType.Product,
                TypeIsEngineering = taskItemType == TaskItemType.Engineering,
                TypeIsUnanticipated = taskItemType == TaskItemType.Unanticipated,

                DevTeamIsAssessments = devTeam == "Assessments",
                DevTeamIsEnterprise = devTeam == "Enterprise",

                CreatedBy = await developerRepository.GetDeveloperByNameAsync(createdBy)
            };

            return await helper.GetEstimation(taskItem);
        }

        private TaskItemType GetTaskItemType(string type)
        {
            var taskItemDeserialization = new KanbanizeTaskItemDeserializer();

            return taskItemDeserialization.GetCardType(type);
        }
    }
}

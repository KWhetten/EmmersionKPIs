using System;
using System.Threading.Tasks;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("multinomial-logistic-regression-analysis")]
    public class MultinomialLogisticRegressionAnalysisController
    {
        [HttpGet]
        public async Task<MultinomialLogisticRegressionAnalysisItemList> GetAsync(string startTime, string finishTime, bool product, bool engineering, bool unanticipated, bool assessmentsTeam, bool enterpriseTeam)
        {
            var startDate = DateHelper.GetStartDate(startTime);
            var finishDate = DateHelper.GetFinishDate(finishTime);

            var helper = new MultinomialLogisticRegressionAnalysisHelper();

            return await helper.GetLogisticRegressionAnalysisData(startDate, finishDate, product, engineering, unanticipated, assessmentsTeam, enterpriseTeam);
        }
    }
}

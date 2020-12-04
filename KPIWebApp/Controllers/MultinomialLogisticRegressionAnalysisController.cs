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
        public async Task<MultinomialLogisticRegressionAnalysisItemList> GetAsync(string startTime, string finishTime)
        {
            var startDate = DateHelper.GetStartDate(startTime).Date;
            var finishDate = DateHelper.GetFinishDate(finishTime).AddDays(1).Date;

            var helper = new MultinomialLogisticRegressionAnalysisHelper();

            return await helper.GetLogisticRegressionAnalysisData(startDate, finishDate);
        }
    }
}

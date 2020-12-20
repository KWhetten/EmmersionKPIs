using System;
using System.Collections.Generic;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using KPIWebApp.Models;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class LeadTimeHelperTests
    {

        [Test]
        public void When_getting_lead_time_and_the_task_list_is_empty()
        {
            var overviewHelper = new OverviewHelper();
            var overviewData = new TaskItemOverviewData();
            var result = overviewHelper.PopulateOverviewData(overviewData, new List<TaskItem>());

            Assert.That(result, Is.EqualTo(overviewData));
        }

        [Test]
        public void When_getting_lead_time_and_task_is_not_finished()
        {
            var leadTimeHelper = new LeadTimeHelper();

            var task = new TaskItem
            {
                StartTime = DateTimeOffset.Now.Date,
                FinishTime = null
            };

            var result = leadTimeHelper.CalculateLeadTimeHours(task);

            Assert.That(result, Is.EqualTo(0));
        }
    }
}

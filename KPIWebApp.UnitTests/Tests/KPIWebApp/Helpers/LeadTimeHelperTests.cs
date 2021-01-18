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

        [Test]
        public void When_getting_lead_time()
        {
            var leadTimeHelper = new LeadTimeHelper();

            var task = new TaskItem
            {
                StartTime = new DateTimeOffset(new DateTime(2021, 1, 11)),
                FinishTime = new DateTimeOffset(new DateTime(2021, 1, 15, 23, 59, 59))
            };

            var result = leadTimeHelper.CalculateLeadTimeHours(task);

            Assert.That(result, Is.InRange(55.9, 56.1));
        }
    }
}

using System;
using KPIWebApp.Helpers;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp
{
    public class DateHelperTests
    {
        [Test]
        public void When_getting_finish_date()
        {
            var result = DateHelper.GetFinishDate("09/01/2020");

            Assert.That(result, Is.EqualTo(new DateTime(2020, 09 ,1)));
        }

        [Test]
        public void When_getting_finish_date_from_non_date_string()
        {
            var result = DateHelper.GetFinishDate("This is not a date!");

            Assert.That(result, Is.EqualTo(new DateTime(2015, 1 ,1)));
        }

        [Test]
        public void When_getting_finish_date_from_null_string()
        {
            var result = DateHelper.GetFinishDate(null);

            Assert.That(result, Is.EqualTo(DateTime.Today.AddDays(1)));
        }


        [Test]
        public void When_getting_start_date()
        {
            var result = DateHelper.GetStartDate("09/30/2020");

            Assert.That(result, Is.EqualTo(new DateTime(2020, 09 ,30)));
        }

        [Test]
        public void When_getting_start_date_from_non_date_string()
        {
            var result = DateHelper.GetStartDate("This is not a date!");

            Assert.That(result, Is.EqualTo(new DateTime(2015, 1 ,1)));
        }

        [Test]
        public void When_getting_start_date_from_null_string()
        {
            var result = DateHelper.GetStartDate(null);

            Assert.That(result, Is.EqualTo(new DateTime(2015, 1 ,1)));
        }
    }
}

using System;
using KPIWebApp.Helpers;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class DateHelperTests
    {
        [Test]
        public void When_getting_finish_date_from_string()
        {
            var result = DateHelper.GetFinishDate("09/01/2020");

            Assert.That(result, Is.EqualTo(new DateTimeOffset(new DateTime(2020, 09 ,1, 23, 59, 59))));
        }

        [Test]
        public void When_getting_finish_date_from_non_date_string()
        {
            var result = DateHelper.GetFinishDate("This is not a date!");

            Assert.That(result, Is.EqualTo(new DateTimeOffset(DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59))));
        }

        [Test]
        public void When_getting_finish_date_from_empty_string()
        {
            var result = DateHelper.GetFinishDate("");

            Assert.That(result, Is.EqualTo(new DateTimeOffset(DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59))));
        }


        [Test]
        public void When_getting_start_date_from_string()
        {
            var result = DateHelper.GetStartDate("09/30/2020");

            Assert.That(result, Is.EqualTo(new DateTimeOffset(new DateTime(2020, 09 ,30))));
        }

        [Test]
        public void When_getting_start_date_from_non_date_string()
        {
            var result = DateHelper.GetStartDate("This is not a date!");

            Assert.That(result, Is.EqualTo(new DateTimeOffset(new DateTime(2020, 10, 19))));
        }

        [Test]
        public void When_getting_start_date_from_empty_string()
        {
            var result = DateHelper.GetStartDate("");

            Assert.That(result, Is.EqualTo(new DateTimeOffset(new DateTime(2020, 10, 19))));
        }

        [Test]
        public void When_getting_start_date_from_null_string()
        {
            var result = DateHelper.GetStartDate((string) null);

            Assert.That(result, Is.EqualTo(new DateTimeOffset(new DateTime(2020, 10, 19))));
        }

        [Test]
        public void When_getting_finish_date_from_null_string()
        {
            var result = DateHelper.GetFinishDate((string) null);

            Assert.That(result, Is.EqualTo(new DateTimeOffset(DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59))));
        }

        [Test]
        public void When_getting_start_date_from_DateTimeOffset()
        {
            var result = DateHelper.GetStartDate(new DateTimeOffset(new DateTime(2021, 1, 1)));

            Assert.That(result, Is.EqualTo(new DateTimeOffset(new DateTime(2021, 1, 1))));
        }

        [Test]
        public void When_getting_finish_date_from_DateTimeOffset()
        {
            var result = DateHelper.GetFinishDate(new DateTimeOffset(new DateTime(2021, 1, 1)));

            Assert.That(result, Is.EqualTo(new DateTimeOffset(new DateTime(2021, 1, 1, 23, 59, 59))));
        }
    }
}

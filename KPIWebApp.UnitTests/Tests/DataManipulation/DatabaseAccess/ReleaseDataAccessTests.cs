﻿using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.DatabaseAccess;
using DataObjects.Objects;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.DataWrapper.DatabaseAccess
{
    [TestFixture]
    public class ReleaseDataAccessTests
    {
        [Test]
        public void When_getting_releases_before_date()
        {
            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            var accessReleaseData = new ReleaseDataAccess();
            var result = accessReleaseData.GetReleasesBeforeDate(date);

            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void When_getting_releases_before_date_max()
        {
            var date = DateTime.MaxValue;

            var accessReleaseData = new ReleaseDataAccess();
            var result = accessReleaseData.GetReleasesBeforeDate(date);

            Assert.That(result.Count, Is.EqualTo(1));

            Assert.That(result[0].Attempts, Is.EqualTo(0));
            Assert.That(result[0].FinishTime, Is.EqualTo(DateTime.MinValue));
            Assert.That(result[0].Id, Is.EqualTo(0));
            Assert.That(result[0].Name, Is.Null);
            Assert.That(result[0].ReleaseEnvironment, Is.Null);
            Assert.That(result[0].Status, Is.EqualTo(null));
        }

        [Test]
        public void When_inserting_release_list()
        {
            var startTime = DateTime.Today.AddDays(-2);
            var finishTime = DateTime.Today;
            var startTime2 = DateTime.Today.AddDays(-3);
            var release1 = new Release
            {
                Id = 1,
                Status = "Status1",
                ReleaseEnvironment = new ReleaseEnvironment
                {
                    Id = 1,
                    Name = "ProductionReleaseEnvironment1"
                },
                StartTime = startTime,
                FinishTime = finishTime,
                Name = "Release1",
                Attempts = 2
            };
            var release2 = new Release
            {
                Id = 2,
                Status = "Status2",
                ReleaseEnvironment = new ReleaseEnvironment
                {
                    Id = 1,
                    Name = "ProductionReleaseEnvironment1"
                },
                StartTime = startTime2,
                FinishTime = DateTime.MaxValue,
                Name = "Release2",
                Attempts = 1
            };
            var release3 = new Release
            {
                Id = 3,
                Status = "Status3",
                ReleaseEnvironment = new ReleaseEnvironment
                {
                    Id = 2,
                    Name = "ReleaseEnvironment1"
                },
                StartTime = DateTime.Today,
                FinishTime = DateTime.Today,
                Name = "Release3",
                Attempts = 57
            };
            var releaseList = new List<Release>
            {
                release1,
                release2,
                release3
            };

            var accessReleaseData = new ReleaseDataAccess();
            accessReleaseData.InsertReleaseList(releaseList);

            var result1 = accessReleaseData.GetReleaseById(release1.Id);

            var result2 = accessReleaseData.GetReleaseById(release2.Id);

            var ex = Assert.Throws<InvalidOperationException>(() => accessReleaseData.GetReleaseById(release3.Id));
            Assert.That(ex.Message, Is.EqualTo("Sequence contains no elements"));

            Assert.That(release1.Id, Is.EqualTo(result1.Id));
            Assert.That(release1.Status, Is.EqualTo(result1.Status));
            Assert.That(release1.ReleaseEnvironment.Id, Is.EqualTo(result1.ReleaseEnvironment.Id));
            Assert.That(release1.ReleaseEnvironment.Name, Is.EqualTo(result1.ReleaseEnvironment.Name));
            Assert.That(release1.StartTime, Is.EqualTo(result1.StartTime));
            Assert.That(release1.FinishTime, Is.EqualTo(result1.FinishTime));
            Assert.That(release1.Name, Is.EqualTo(result1.Name));
            Assert.That(release1.Attempts, Is.EqualTo(result1.Attempts));

            Assert.That(release2.Id, Is.EqualTo(result2.Id));
            Assert.That(release2.Status, Is.EqualTo(result2.Status));
            Assert.That(release2.ReleaseEnvironment.Id, Is.EqualTo(result2.ReleaseEnvironment.Id));
            Assert.That(release2.ReleaseEnvironment.Name, Is.EqualTo(result2.ReleaseEnvironment.Name));
            Assert.That(release2.StartTime, Is.EqualTo(result2.StartTime));
            Assert.That(release2.FinishTime, Is.EqualTo(DateTime.MaxValue));
            Assert.That(release2.Name, Is.EqualTo(result2.Name));
            Assert.That(release2.Attempts, Is.EqualTo(result2.Attempts));

            accessReleaseData.RemoveReleaseById(release1.Id);
            accessReleaseData.RemoveReleaseById(release2.Id);
            accessReleaseData.RemoveReleaseEnvironmentById(release1.ReleaseEnvironment.Id);
        }

        [Test]
        public void When_inserting_release_list_null()
        {
            var accessReleaseData = new ReleaseDataAccess();
            accessReleaseData.InsertReleaseList(null);
        }
    }
}

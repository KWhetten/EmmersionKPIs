﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using NUnit.Framework;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.DataRepositories
{
    [TestFixture]
    public class ReleaseRepositoryTests
    {
        private readonly ReleaseRepository releaseRepository = new ReleaseRepository(new DatabaseConnection());
        private readonly ReleaseEnvironmentRepository releaseEnvironmentRepository = new ReleaseEnvironmentRepository(new DatabaseConnection());


        [Test]
        public async Task When_getting_releases_in_date_range()
        {
            var result = (await releaseRepository.GetReleaseListAsync(DateTime.Now.AddDays(-30), DateTime.Now)).ToList();

            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task When_getting_releases_before_date()
        {
            var date = DateTimeOffset.Now;

            var result = await releaseRepository.GetFirstReleaseBeforeDateAsync(date);

            Assert.That(result != null);
        }

        [Test]
        public async Task When_getting_release_before_date_max()
        {
            DateTimeOffset? date = null;

            var result = await releaseRepository.GetFirstReleaseBeforeDateAsync(date);

            Assert.That(result.Attempts, Is.EqualTo(0));
            Assert.That(result.FinishTime, Is.EqualTo(null));
            Assert.That(result.Id, Is.EqualTo(0));
            Assert.That(result.Name, Is.Null);
            Assert.That(result.ReleaseEnvironment, Is.Null);
            Assert.That(result.State, Is.EqualTo(null));
        }

        [Test]
        public async Task When_inserting_release_list()
        {
            var startTime = DateTimeOffset.Now.Date.AddDays(-2);
            var finishTime = DateTimeOffset.Now.Date;
            var startTime2 = DateTimeOffset.Now.Date.AddDays(-3);
            var release1 = new Release
            {
                Id = 1,
                State = "Status1",
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
                State = "Status2",
                ReleaseEnvironment = new ReleaseEnvironment
                {
                    Id = 1,
                    Name = "ProductionReleaseEnvironment1"
                },
                StartTime = startTime2,
                FinishTime = null,
                Name = "Release2",
                Attempts = 1
            };
            var release3 = new Release
            {
                Id = 3,
                State = "Status3",
                ReleaseEnvironment = new ReleaseEnvironment
                {
                    Id = 2,
                    Name = "ReleaseEnvironment1"
                },
                StartTime = DateTimeOffset.Now.Date,
                FinishTime = DateTimeOffset.Now.Date,
                Name = "Release3",
                Attempts = 57
            };
            var releaseList = new List<Release>
            {
                release1,
                release2,
                release3
            };
            await releaseRepository.InsertReleaseListAsync(releaseList);

            var result1 = await releaseRepository.GetReleaseByIdAsync(release1.Id);

            var result2 = await releaseRepository.GetReleaseByIdAsync(release2.Id);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await releaseRepository.GetReleaseByIdAsync(release3.Id));
            Assert.That(ex.Message, Is.EqualTo("Sequence contains no elements"));

            Assert.That(release1.Id, Is.EqualTo(result1.Id));
            Assert.That(release1.State, Is.EqualTo(result1.State));
            Assert.That(release1.ReleaseEnvironment.Id, Is.EqualTo(result1.ReleaseEnvironment.Id));
            Assert.That(release1.ReleaseEnvironment.Name, Is.EqualTo(result1.ReleaseEnvironment.Name));
            Assert.That(release1.StartTime, Is.EqualTo(result1.StartTime));
            Assert.That(release1.FinishTime, Is.EqualTo(result1.FinishTime));
            Assert.That(release1.Name, Is.EqualTo(result1.Name));
            Assert.That(release1.Attempts, Is.EqualTo(result1.Attempts));

            Assert.That(release2.Id, Is.EqualTo(result2.Id));
            Assert.That(release2.State, Is.EqualTo(result2.State));
            Assert.That(release2.ReleaseEnvironment.Id, Is.EqualTo(result2.ReleaseEnvironment.Id));
            Assert.That(release2.ReleaseEnvironment.Name, Is.EqualTo(result2.ReleaseEnvironment.Name));
            Assert.That(release2.StartTime, Is.EqualTo(result2.StartTime));
            Assert.That(release2.FinishTime, Is.EqualTo(null));
            Assert.That(release2.Name, Is.EqualTo(result2.Name));
            Assert.That(release2.Attempts, Is.EqualTo(result2.Attempts));

            await releaseRepository.RemoveReleaseByIdAsync(release1.Id);
            await releaseRepository.RemoveReleaseByIdAsync(release2.Id);
            await releaseEnvironmentRepository.RemoveReleaseEnvironmentById(release1.ReleaseEnvironment.Id);
        }

        [Test]
        public async Task When_inserting_release_list_null()
        {
            await releaseRepository.InsertReleaseListAsync(null);
        }

        [Test]
        public void When_trying_to_insert_a_release_that_is_already_finished()
        {
            var result = releaseRepository.ReleaseIsFinishedInDatabase(1);
            Assert.IsFalse(result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using DataObjects;
using DataWrapper.DatabaseAccess;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.DatabaseAccess
{
    [TestFixture]
    public class DataAccessTests
    {
        [Test]
        public void When_getting_releases_before_date()
        {
            var date = new DateTime(2020, 9, 1, 5, 30, 0);

            var dataAccess = new DataWrapper.DatabaseAccess.DataAccess();
            var result = dataAccess.GetReleasesBeforeDate(date);

            Assert.That(result.Count(), Is.EqualTo(9));

            Assert.That(result[0].Attempts, Is.EqualTo(1));
            Assert.That(result[0].FinishTime, Is.EqualTo(new DateTime(2020, 8, 31, 19, 26, 42)));
            Assert.That(result[0].Id, Is.EqualTo(12373));
            Assert.That(result[0].Name, Is.EqualTo("TrueNorthTest-6756"));
            Assert.That(result[0].ReleaseEnvironment.Id, Is.EqualTo(15));
            Assert.That(result[0].ReleaseEnvironment.Name, Is.EqualTo("Production                                        "));
            Assert.That(result[0].Status, Is.EqualTo("succeeded"));
        }

        [Test]
        public void When_inserting_release_list()
        {
            var startTime = DateTime.Today.AddDays(-2);
            var finishTime = DateTime.Today;
            var startTime2 = DateTime.Today.AddDays(-3);
            var finishTime2 = DateTime.Today.AddDays(-1);
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
                FinishTime = finishTime2,
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

            var dataAccess = new DataWrapper.DatabaseAccess.DataAccess();
            dataAccess.InsertReleaseList(releaseList);

            var result1 = dataAccess.GetReleaseById(release1.Id);

            var result2 = dataAccess.GetReleaseById(release2.Id);

            var ex = Assert.Throws<InvalidOperationException>(() => dataAccess.GetReleaseById(release3.Id));
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
            Assert.That(release2.FinishTime, Is.EqualTo(result2.FinishTime));
            Assert.That(release2.Name, Is.EqualTo(result2.Name));
            Assert.That(release2.Attempts, Is.EqualTo(result2.Attempts));

            dataAccess.RemoveReleaseById(release1.Id);
            dataAccess.RemoveReleaseById(release2.Id);
            dataAccess.RemoveReleaseEnvironmentById(release1.ReleaseEnvironment.Id);
        }

        [Test]
        public void When_inserting_release_list_null()
        {
            var dataAccess = new DataWrapper.DatabaseAccess.DataAccess();
            dataAccess.InsertReleaseList(null);
        }

        [Test]
        public void When_inserting_work_item_card_list()
        {
            var card1 = new WorkItemCard
            {
                Id = 1,
                Title = "Card1",
                StartTime = DateTime.Today.AddDays(-2).Date,
                FinishTime = DateTime.Today.Date,
                Type = WorkItemCardType.Unanticipated,
                DevelopmentTeamName = "Team1",
                CreatedOn = DateTime.Today,
                CreatedBy = "CreatedBy1",
                LastChangedOn = DateTime.Today,
                LastChangedBy = "ChangedBy1",
                CurrentBoardColumn = "CurrentColumn1",
                CardState = "State1",
                Impact = "Impact1",
                CommentCount = 5,
                NumRevisions = 4,
                Release = new Release
                {
                    Id = 1,
                    Status = "Status1",
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = 1,
                        Name = "ProductionReleaseEnvironment1"
                    },
                    StartTime = DateTime.Today,
                    FinishTime = DateTime.Today,
                    Name = "Release1",
                    Attempts = 2
                }
            };
            var card2 = new WorkItemCard
            {

            };

            var workItemCardList = new List<WorkItemCard>
            {
                card1,
                card2
            };

            var dataAccess = new DataWrapper.DatabaseAccess.DataAccess();
            dataAccess.InsertWorkItemCardList(workItemCardList);

            var result1 = dataAccess.GetCardById(card1.Id);

            var ex = Assert.Throws<InvalidOperationException>(() => dataAccess.GetCardById(card2.Id));
            Assert.That(ex.Message, Is.EqualTo("Sequence contains no elements"));

            Assert.That(card1.Id, Is.EqualTo(result1.Id));
            Assert.That(card1.Title, Is.EqualTo(result1.Title));
            Assert.That(card1.StartTime, Is.EqualTo(result1.StartTime));
            Assert.That(card1.FinishTime, Is.EqualTo(result1.FinishTime));
            Assert.That(card1.Type, Is.EqualTo(result1.Type));
            Assert.That(card1.DevelopmentTeamName, Is.EqualTo(result1.DevelopmentTeamName));
            Assert.That(card1.CreatedOn, Is.EqualTo(result1.CreatedOn));
            Assert.That(card1.CreatedBy, Is.EqualTo(result1.CreatedBy));
            Assert.That(card1.LastChangedOn, Is.EqualTo(result1.LastChangedOn));
            Assert.That(card1.LastChangedBy, Is.EqualTo(result1.LastChangedBy));
            Assert.That(card1.CurrentBoardColumn, Is.EqualTo(result1.CurrentBoardColumn));
            Assert.That(card1.CardState, Is.EqualTo(result1.CardState));
            Assert.That(card1.Impact, Is.EqualTo(result1.Impact));
            Assert.That(card1.CommentCount, Is.EqualTo(result1.CommentCount));
            Assert.That(card1.NumRevisions, Is.EqualTo(result1.NumRevisions));
            Assert.That(card1.Release.Id, Is.EqualTo(result1.Release.Id));
            Assert.That(card1.Release.Status, Is.EqualTo(result1.Release.Status));
            Assert.That(card1.Release.ReleaseEnvironment.Id, Is.EqualTo(result1.Release.ReleaseEnvironment.Id));
            Assert.That(card1.Release.ReleaseEnvironment.Name, Is.EqualTo(result1.Release.ReleaseEnvironment.Name));
            Assert.That(card1.Release.StartTime, Is.EqualTo(result1.Release.StartTime));
            Assert.That(card1.Release.FinishTime, Is.EqualTo(result1.Release.FinishTime));
            Assert.That(card1.Release.Name, Is.EqualTo(result1.Release.Name));
            Assert.That(card1.Release.Attempts, Is.EqualTo(result1.Release.Attempts));

            dataAccess.RemoveWorkItemCardById(card1.Id);
            dataAccess.RemoveReleaseById(card1.Release.Id);
            dataAccess.RemoveReleaseEnvironmentById(card1.Release.ReleaseEnvironment.Id);
        }

        [Test]
        public void When_getting_all_work_item_cards()
        {
            var dataAccess = new DataAccess();

            var result = dataAccess.GetWorkItemCardList();

            Assert.That(result.Count(), Is.GreaterThan(400));
        }

        [Test]
        public void When_getting_all_releases()
        {
            var dataAccess = new DataAccess();

            var result = dataAccess.GetReleaseList();

            Assert.That(result.Count(), Is.GreaterThan(20));
        }
    }
}

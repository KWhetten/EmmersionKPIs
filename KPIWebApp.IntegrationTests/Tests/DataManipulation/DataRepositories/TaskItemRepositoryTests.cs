using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using NUnit.Framework;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.DataRepositories
{
    [TestFixture]
    public class TaskItemRepositoryTests
    {
        private readonly TaskItemRepository taskItemRepository = new TaskItemRepository(new DatabaseConnection());
        private readonly ReleaseRepository releaseRepository = new ReleaseRepository(new DatabaseConnection());

        [Test]
        public async Task When_inserting_work_item_card_list()
        {
            var flag = false;
            var message = "";

            var card1 = new TaskItem
            {
                Id = 1,
                Title = "Card1",
                StartTime = DateTime.Today.AddDays(-2).Date,
                FinishTime = DateTime.Today.Date,
                Type = TaskItemType.Unanticipated,
                DevelopmentTeam = "Team1",
                CreatedOn = DateTime.Today,
                CreatedBy = "CreatedBy1",
                LastChangedOn = DateTime.Today,
                LastChangedBy = "ChangedBy1",
                CurrentBoardColumn = BoardColumn.Backlog,
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent
                    {
                        Id = 0,
                        TaskItemColumn = BoardColumn.InProcessWorking,
                        EventDate = new DateTimeOffset(new DateTime(2020, 10, 27)),
                        EventType = "Task moved"
                    }
                },
                State = TaskItemState.Backlog,
                NumRevisions = 4,
                Release = new Release
                {
                    Id = 1,
                    State = "State1",
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
            var card2 = new TaskItem();

            var taskItemList = new List<TaskItem>
            {
                card1,
                card2
            };

            try
            {
                await taskItemRepository.InsertTaskItemListAsync(taskItemList);

                var result1 = await taskItemRepository.GetTaskItemByIdAsync(card1.Id);

                var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await taskItemRepository.GetTaskItemByIdAsync(card2.Id));
                Assert.That(ex.Message, Is.EqualTo("Sequence contains no elements"));

                Assert.That(card1.Id, Is.EqualTo(result1.Id));
                Assert.That(card1.Title, Is.EqualTo(result1.Title));
                Assert.That(card1.StartTime, Is.EqualTo(result1.StartTime));
                Assert.That(card1.FinishTime, Is.EqualTo(result1.FinishTime));
                Assert.That(card1.Type, Is.EqualTo(result1.Type));
                Assert.That(card1.DevelopmentTeam, Is.EqualTo(result1.DevelopmentTeam));
                Assert.That(card1.CreatedOn, Is.EqualTo(result1.CreatedOn));
                Assert.That(card1.CreatedBy, Is.EqualTo(result1.CreatedBy));
                Assert.That(card1.LastChangedOn, Is.EqualTo(result1.LastChangedOn));
                Assert.That(card1.LastChangedBy, Is.EqualTo(result1.LastChangedBy));
                Assert.That(card1.CurrentBoardColumn, Is.EqualTo(result1.CurrentBoardColumn));
                Assert.That(card1.State, Is.EqualTo(result1.State));
                Assert.That(card1.NumRevisions, Is.EqualTo(result1.NumRevisions));
                Assert.That(card1.Release.Id, Is.EqualTo(result1.Release.Id));
                Assert.That(card1.Release.State, Is.EqualTo(result1.Release.State));
                Assert.That(card1.Release.ReleaseEnvironment.Id, Is.EqualTo(result1.Release.ReleaseEnvironment.Id));
                Assert.That(card1.Release.ReleaseEnvironment.Name, Is.EqualTo(result1.Release.ReleaseEnvironment.Name));
                Assert.That(card1.Release.StartTime, Is.EqualTo(result1.Release.StartTime));
                Assert.That(card1.Release.FinishTime, Is.EqualTo(result1.Release.FinishTime));
                Assert.That(card1.Release.Name, Is.EqualTo(result1.Release.Name));
                Assert.That(card1.Release.Attempts, Is.EqualTo(result1.Release.Attempts));
            }
            catch (Exception ex)
            {
                flag = true;
                message = ex.Message;
            }
            finally
            {
                await taskItemRepository.RemoveHistoryItemByIdAsync(card1.HistoryEvents[0].Id);
                await taskItemRepository.RemoveTaskItemByIdAsync(card1.Id);
                await releaseRepository.RemoveReleaseByIdAsync(card1.Release.Id);
                await releaseRepository.RemoveReleaseEnvironmentById(card1.Release.ReleaseEnvironment.Id);
                if (flag)
                {
                    Assert.False(true, message);
                }
            }
        }

        [Test]
        public async Task When_getting_work_item_cards_in_date_range()
        {
            var result = await taskItemRepository.GetTaskItemListAsync(DateTimeOffset.Now.AddDays(-30), DateTimeOffset.Now);

            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task When_getting_releases_in_date_range()
        {
            var result = await releaseRepository.GetReleaseListAsync(DateTime.Now.AddDays(-30), DateTime.Now);

            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void When_getting_task_item_types()
        {
            var result = taskItemRepository.GetTaskItemTypes();

            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(TaskItemType.Product));
            Assert.That(result[1], Is.EqualTo(TaskItemType.Engineering));
            Assert.That(result[2], Is.EqualTo(TaskItemType.Unanticipated));
        }

        [Test]
        public void When_task_item_has_already_been_released()
        {
            var result = taskItemRepository.TaskItemHasBeenReleasedAsync(1);

            Assert.False(result);

            result = taskItemRepository.TaskItemHasBeenReleasedAsync(462);

            Assert.True(result);
        }

        [Test]
        public async Task When_getting_history_events_by_task_item_id()
        {
            var result = await taskItemRepository.GetHistoryEventsByTaskIdAsync(361);

            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}

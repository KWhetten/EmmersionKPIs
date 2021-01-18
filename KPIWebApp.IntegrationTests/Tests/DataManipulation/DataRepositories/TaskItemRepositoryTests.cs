using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using NUnit.Framework;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.DataRepositories
{
    [TestFixture]
    public class TaskItemRepositoryTests
    {
        private readonly TaskItemRepository taskItemRepository = new TaskItemRepository();
        private readonly ReleaseRepository releaseRepository = new ReleaseRepository();
        private readonly HistoryEventRepository historyEventRepository = new HistoryEventRepository();

        private readonly ReleaseEnvironmentRepository releaseEnvironmentRepository =
            new ReleaseEnvironmentRepository(new DatabaseConnection());

        private readonly TaskItem card1 = new TaskItem
        {
            Id = 1,
            Title = "Card1",
            StartTime = DateTime.Today.AddDays(-2).Date,
            FinishTime = DateTime.Today.Date,
            Type = TaskItemType.Unanticipated,
            DevelopmentTeam = new DevelopmentTeam
                {
                    Id = 5,
                    Name = "Assessments"
                },
            CreatedOn = DateTime.Today,
            CreatedBy = new Developer{
                Id = 1014,
                Name = "Charles"
            },
            LastChangedOn = DateTimeOffset.Now.Date,
            LastChangedBy = new Developer{
                Id = 1014,
                Name = "Charles"
            },
            CurrentBoardColumn = BoardColumn.Backlog,
            State = TaskItemState.Released,
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

        private List<TaskItem> taskItemList;
        private readonly TaskItem card2 = new TaskItem();

        [SetUp]
        public void SetUp()
        {
            taskItemList = new List<TaskItem>
            {
                card1,
                card2
            };
        }

        [TearDown]
        public async Task TearDown()
        {
            await historyEventRepository.RemoveHistoryItemByIdAsync(card1.HistoryEvents[0].Id);
            await taskItemRepository.RemoveTaskItemByIdAsync(card1.Id);
            await taskItemRepository.RemoveTaskItemByIdAsync(card2.Id);
            await releaseRepository.RemoveReleaseByIdAsync(card1.Release.Id);
            await releaseEnvironmentRepository.RemoveReleaseEnvironmentById(card1.Release.ReleaseEnvironment.Id);
        }

        [Test]
        public async Task When_inserting_task_item_card_list()
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
            Assert.That(card1.DevelopmentTeam.Id, Is.EqualTo(result1.DevelopmentTeam.Id));
            Assert.That(card1.DevelopmentTeam.Name, Is.EqualTo(result1.DevelopmentTeam.Name));
            Assert.That(card1.CreatedOn, Is.EqualTo(result1.CreatedOn));
            Assert.That(card1.CreatedBy.Id, Is.EqualTo(result1.CreatedBy.Id));
            Assert.That(card1.CreatedBy.Name, Is.EqualTo(result1.CreatedBy.Name));
            Assert.That(card1.LastChangedOn, Is.EqualTo(result1.LastChangedOn));
            Assert.That(card1.LastChangedBy.Id, Is.EqualTo(result1.LastChangedBy.Id));
            Assert.That(card1.LastChangedBy.Name, Is.EqualTo(result1.LastChangedBy.Name));
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

        [Test]
        public async Task When_getting_task_item_cards_in_date_range()
        {
            var result =
                await taskItemRepository.GetTaskItemListAsync(DateTimeOffset.Now.AddDays(-30), DateTimeOffset.Now);

            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task When_inserting_task_item()
        {
            await taskItemRepository.InsertTaskItemAsync(card1);
            var result = taskItemRepository.GetTaskItemByIdAsync(1);

            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task When_a_task_item_has_already_been_released()
        {
            var result = await taskItemRepository.TaskItemHasBeenReleasedAsync(1);
            Assert.False(result);
            
            await taskItemRepository.InsertTaskItemAsync(card1);

            result = await taskItemRepository.TaskItemHasBeenReleasedAsync(1);
            Assert.True(result);
        }

        [Test]
        public async Task When_task_item_has_not_been_released()
        {
            var result = await taskItemRepository.TaskItemHasBeenReleasedAsync(null);

            Assert.False(result);
        }

        [Test]
        public async Task When_getting_task_item_list_from_invalid_task_item_info()
        {
            var result = await TaskItemRepository.GetTaskItemListFromTaskItemInfoAsync(null);

            Assert.That(result.Count, Is.Zero);
        }
    }
}

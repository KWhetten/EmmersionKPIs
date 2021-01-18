using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using NUnit.Framework;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.DataRepositories
{
    public class HistoryEventRepositoryTests
    {
        HistoryEventRepository historyEventRepository;

        public HistoryEventRepositoryTests()
        {
            historyEventRepository = new HistoryEventRepository();
        }

        [Test]
        public async Task When_getting_history_events_by_task_item_id()
        {
            var result = await historyEventRepository.GetHistoryEventsByTaskIdAsync(361);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task When_inserting_invalid_history_event()
        {
            var task = new TaskItem
            {
                HistoryEvents = new List<HistoryEvent>
                {
                    new HistoryEvent()
                }
            };

            var result = await historyEventRepository.InsertHistoryEventsAsync(task);

            Assert.False(result);
        }

        [Test]
        public async Task When_inserting_history_events_for_task_with_no_history_events()
        {
            var task = new TaskItem
            {
                HistoryEvents = new List<HistoryEvent>()
            };

            var result = await historyEventRepository.InsertHistoryEventsAsync(task);

            Assert.True(result);
        }
    }
}

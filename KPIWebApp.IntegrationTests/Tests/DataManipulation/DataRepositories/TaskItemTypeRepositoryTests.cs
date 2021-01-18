using DataAccess.DataRepositories;
using DataAccess.Objects;
using NUnit.Framework;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.DataRepositories
{
    public class TaskItemTypeRepositoryTests
    {
        readonly TaskItemTypeRepository taskItemTypeRepository;

        public TaskItemTypeRepositoryTests()
        {
            taskItemTypeRepository = new TaskItemTypeRepository();
        }
        [Test]
        public void When_getting_task_item_types()
        {
            var result = taskItemTypeRepository.GetTaskItemTypes();

            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(TaskItemType.Product));
            Assert.That(result[1], Is.EqualTo(TaskItemType.Engineering));
            Assert.That(result[2], Is.EqualTo(TaskItemType.Unanticipated));
        }

    }
}

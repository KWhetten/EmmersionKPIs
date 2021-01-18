using System.Threading.Tasks;
using DataAccess.DataRepositories;
using NUnit.Framework;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.DataRepositories
{
    public class DeveloperRepositoryTests
    {
        private readonly DeveloperRepository developerRepository = new DeveloperRepository();
        private const string Name = "Name1";

        [Test]
        public async Task When_inserting_new_developer()
        {
            await developerRepository.InsertDeveloperAsync(Name);
            var result = await developerRepository.GetDeveloperByNameAsync(Name);

            Assert.That(result.Name, Is.EqualTo(Name));
        }

        [TearDown]
        public async Task TearDown()
        {
            await developerRepository.RemoveDeveloperByNameAsync(Name);
        }
    }
}

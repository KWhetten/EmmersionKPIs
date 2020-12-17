using System.Threading.Tasks;
using DataAccess.DataRepositories;
using NUnit.Framework;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.DataRepositories
{
    public class DevelopmentTeamsRepositoryTests
    {
        private DevelopmentTeamsRepository developmentTeamsRepository = new DevelopmentTeamsRepository();
        private const int Id = 1;
        private const string Name = "Team1";

        [Test]
        public async Task When_saving_a_new_development_team()
        {
            await developmentTeamsRepository.SaveTeamAsync(Id, Name);
            var result = await developmentTeamsRepository.GetTeam(Id);

            Assert.That(result.Id, Is.EqualTo(Id));
            Assert.That(result.Name, Is.EqualTo(Name));
        }

        [TearDown]
        public void TearDown()
        {
            developmentTeamsRepository.RemoveTeam(Id);
        }
    }
}

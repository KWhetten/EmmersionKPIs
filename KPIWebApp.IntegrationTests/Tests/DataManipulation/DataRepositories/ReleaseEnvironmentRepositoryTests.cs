using System.Threading.Tasks;
using DataAccess.DataRepositories;
using NUnit.Framework;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.DataRepositories
{
    public class ReleaseEnvironmentRepositoryTests
    {
        private readonly ReleaseEnvironmentRepository releaseEnvironmentRepository;

        public ReleaseEnvironmentRepositoryTests()
        {
            releaseEnvironmentRepository = new ReleaseEnvironmentRepository();
        }

        [Test]
        public async Task When_getting_release_environment_by_id()
        {
            var result = await releaseEnvironmentRepository.GetReleaseEnvironmentByIdAsync(33);

            Assert.That(result.Id, Is.EqualTo(33));
            Assert.That(result.Name, Is.EqualTo("Production"));
        }

        [Test]
        public async Task When_saving_release_environment()
        {
            const int releaseEnvironmentId = 1;
            try
            {
                const string releaseEnvironmentName = "Envrionment1";

                await releaseEnvironmentRepository.SaveReleaseEnvironmentAsync(releaseEnvironmentId,
                    releaseEnvironmentName);

                var result = await releaseEnvironmentRepository.GetReleaseEnvironmentByIdAsync(releaseEnvironmentId);

                Assert.That(result.Id, Is.EqualTo(releaseEnvironmentId));
                Assert.That(result.Name, Is.EqualTo(releaseEnvironmentName));
            }
            finally
            {
                await releaseEnvironmentRepository.RemoveReleaseEnvironmentById(releaseEnvironmentId);
            }
        }
    }
}

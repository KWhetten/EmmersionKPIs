using System.Threading.Tasks;
using DataAccess.DataRepositories;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class RegisterHelperTests
    {
        [Test]
        public async Task When_registering_new_user()
        {
            var mockUserDataAccess = new Mock<IUserRepository>();
            mockUserDataAccess.Setup(x => x.InsertUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(1);

            var registerController = new RegisterHelper(mockUserDataAccess.Object);

            var registerData = new RegisterHelper.RegisterData
            {
                BaseUrl = "baseurl",
                Email = "email@email.email",
                FirstName = "First Name",
                LastName = "Last Name"
            };

            var result = await registerController.RegisterUser(registerData);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task When_registering_new_user_with_duplicate_email()
        {
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(x => x.InsertUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(-1);

            var registerHelper = new RegisterHelper(mockUserRepository.Object);

            var registerData = new RegisterHelper.RegisterData
            {
                BaseUrl = "baseurl",
                Email = "email@email.email",
                FirstName = "First Name",
                LastName = "Last Name"
            };

            var result = await registerHelper.RegisterUser(registerData);

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public async Task When_registering_new_user_and_error_occurs()
        {
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(x => x.InsertUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(0);

            var registerHelper = new RegisterHelper(mockUserRepository.Object);

            var registerData = new RegisterHelper.RegisterData
            {
                BaseUrl = "baseurl",
                Email = "email@email.email",
                FirstName = "First Name",
                LastName = "Last Name"
            };

            var result = await registerHelper.RegisterUser(registerData);

            Assert.That(result, Is.EqualTo(0));
        }
    }
}

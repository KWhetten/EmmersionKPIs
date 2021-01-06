using System.Threading.Tasks;
using DataAccess.DataRepositories;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class PasswordChangeHelperTests
    {
        [Test]
        public async Task When_changing_password()
        {
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(x => x.InsertPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(1);

            var passwordChangeHelper = new PasswordChangeHelper(mockUserRepository.Object);

            var changePasswordData = new PasswordChangeHelper.ChangePasswordData
            {
                Email = "email@email.email",
                Password = "Password1"
            };

            var result = await passwordChangeHelper.UpdatePassword(changePasswordData.Email, changePasswordData.Password);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task When_changing_password_with_duplicate_email()
        {
            var mockUserDataAccess = new Mock<IUserRepository>();
            mockUserDataAccess.Setup(x => x.InsertPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(-1);

            var passwordChangeHelper = new PasswordChangeHelper(mockUserDataAccess.Object);

            var changePasswordData = new PasswordChangeHelper.ChangePasswordData
            {
                Email = "email@email.email",
                Password = "Password1"
            };

            var result = await passwordChangeHelper.UpdatePassword(changePasswordData.Email, changePasswordData.Password);

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public async Task When_changing_password_and_error_occurs()
        {
            var mockUserDataAccess = new Mock<IUserRepository>();
            mockUserDataAccess.Setup(x => x.InsertPasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(0);

            var passwordChangeHelper = new PasswordChangeHelper(mockUserDataAccess.Object);

            var changePasswordData = new PasswordChangeHelper.ChangePasswordData
            {
                Email = "email@email.email",
                Password = "Password1"
            };

            var result = await passwordChangeHelper.UpdatePassword(changePasswordData.Email, changePasswordData.Password);

            Assert.That(result, Is.EqualTo(0));
        }
    }
}

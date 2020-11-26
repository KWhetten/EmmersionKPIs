using System;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class ForgotPasswordHelperTests
    {
        [Test]
        public async Task When_forgot_password()
        {
            var mockDatabaseAccess = new Mock<IUserRepository>();
            mockDatabaseAccess.Setup(x => x.GetUserInfoByEmailAsync(It.IsAny<string>())).ReturnsAsync(new UserInfo
            {
                Email = "email@email.com",
                Password = "Password1"
            });
            var mockEmailHelper = new Mock<EmailHelper>();
            mockEmailHelper.Setup(x => x.SendForgotPasswordEmail(It.IsAny<UserInfo>(), It.IsAny<string>()))
                .Returns(true);

            var forgotPasswordHelper =
                new ForgotPasswordHelper(mockDatabaseAccess.Object, mockEmailHelper.Object);

            var result = await forgotPasswordHelper.SendEmail("email@email.com");

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task When_forgot_password_has_error()
        {
            var mockEmailManager = new Mock<EmailHelper>();
            mockEmailManager.Setup(x => x.SendForgotPasswordEmail(It.IsAny<UserInfo>(), It.IsAny<string>()))
                .Returns(false);

            var mockDatabaseAccess = new Mock<IUserRepository>();
            mockDatabaseAccess.Setup(x => x.GetUserInfoByEmailAsync(It.IsAny<string>())).ReturnsAsync(new UserInfo
            {
                Email = "email@email.com",
                Password = "Password1"
            });

            var forgotPasswordHelper =
                new ForgotPasswordHelper(mockDatabaseAccess.Object, mockEmailManager.Object);

            var result = await forgotPasswordHelper.SendEmail("email@email.com");

            Assert.That(result, Is.False);
        }

        [Test]
        public void When_forgot_password_with_incorrect_email()
        {
            var mockEmailManager = new Mock<EmailHelper>();
            mockEmailManager.Setup(x => x.SendForgotPasswordEmail(It.IsAny<UserInfo>(), It.IsAny<string>()))
                .Returns(true);


            var mockDatabaseAccess = new Mock<IUserRepository>();
            mockDatabaseAccess.Setup(x => x.GetUserInfoByEmailAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            var forgotPasswordHelper = new ForgotPasswordHelper(mockDatabaseAccess.Object, mockEmailManager.Object);

            Assert.ThrowsAsync<Exception>(async () => await forgotPasswordHelper.SendEmail("email@email.com"));
        }
    }
}

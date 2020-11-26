using System;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp.Helpers
{
    public class LoginUserHelperTests
    {
         [Test]
         public async Task When_logging_in_user_with_correct_password()
         {
             var mockAuthorizationData = new Mock<IUserRepository>();
             var guid = Guid.NewGuid();
             var loginData = new LoginUserHelper.LoginData
             {
                 Email = "email@email.email",
                 Guid = guid
             };
             var userInfo = new UserInfo
             {
                 FirstName = "First Name",
                 LastName = "Last Name",
                 Email = loginData.Email,
                 Guid = loginData.Guid,
                 Password = "Password1"
             };
             mockAuthorizationData.Setup(x => x.GetUserInfoByEmailAsync(It.IsAny<string>())).ReturnsAsync(userInfo);
             mockAuthorizationData.Setup(x => x.VerifyPasswordAsync(It.IsAny<UserInfo>())).ReturnsAsync(true);
             mockAuthorizationData.Setup(x => x.AuthorizeUserAsync(It.IsAny<UserInfo>()));

             var loginUserController = new LoginUserHelper(mockAuthorizationData.Object);

             var result = await loginUserController.LoginUser(loginData);

             Assert.That(result.FirstName, Is.EqualTo(userInfo.FirstName));
             Assert.That(result.LastName, Is.EqualTo(userInfo.LastName));
             Assert.That(result.Email, Is.EqualTo(userInfo.Email));
             Assert.That(result.Guid, Is.EqualTo(userInfo.Guid));
             Assert.That(result.Password, Is.EqualTo(userInfo.Password));
        }

        [Test]
        public async Task When_logging_in_user_with_incorrect_password()
        {
            var mockUserRepository = new Mock<IUserRepository>();
            var guid = Guid.NewGuid();
            mockUserRepository.Setup(x => x.GetUserInfoByEmailAsync(It.IsAny<string>())).ReturnsAsync(new UserInfo
            {
                FirstName = "First Name",
                LastName = "Last Name",
                Email = "email@email.email",
                Guid = guid,
                Password = "Password1"
            });
            mockUserRepository.Setup(x => x.VerifyPasswordAsync(It.IsAny<UserInfo>())).ReturnsAsync(false);
            mockUserRepository.Setup(x => x.AuthorizeUserAsync(It.IsAny<UserInfo>()));

            var loginUserHelper = new LoginUserHelper(mockUserRepository.Object);

            var result = await loginUserHelper.LoginUser(new LoginUserHelper.LoginData());

            Assert.That(result.FirstName, Is.EqualTo(null));
            Assert.That(result.LastName, Is.EqualTo(null));
            Assert.That(result.Email, Is.EqualTo(null));
            Assert.That(result.Guid, Is.EqualTo(new Guid()));
            Assert.That(result.Password, Is.EqualTo(null));
        }
    }
}

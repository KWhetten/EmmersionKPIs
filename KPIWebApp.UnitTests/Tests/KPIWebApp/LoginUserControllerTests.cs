using System;
using DataAccess.DatabaseAccess;
using DataObjects.Objects;
using KPIWebApp.Controllers;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp
{
    public class LoginUserControllerTests
    {
        // [Test]
        // public void When_logging_in_user_with_correct_password()
        // {
        //     var mockAuthorizationData = new Mock<UserDataAccess>();
        //     var guid = Guid.NewGuid();
        //     var loginData = new LoginData
        //     {
        //         Email = "email@email.email",
        //         Guid = guid
        //     };
        //     var userInfo = new UserInfo
        //     {
        //         FirstName = "First Name",
        //         LastName = "Last Name",
        //         Email = loginData.Email,
        //         Guid = loginData.Guid,
        //         Password = "Password1"
        //     };
        //     mockAuthorizationData.Setup(x => x.GetUserInfoByEmail(It.IsAny<string>())).Returns(userInfo);
        //     mockAuthorizationData.Setup(x => x.VerifyPassword(It.IsAny<UserInfo>())).Returns(true);
        //     mockAuthorizationData.Setup(x => x.AuthorizeUser(It.IsAny<UserInfo>()));
        //
        //     var loginUserController = new LoginUserController(mockAuthorizationData.Object);
        //
        //     var result = loginUserController.Post(new LoginData());
        //
        //     Assert.That(result.FirstName, Is.EqualTo(userInfo.FirstName));
        //     Assert.That(result.LastName, Is.EqualTo(userInfo.LastName));
        //     Assert.That(result.Email, Is.EqualTo(userInfo.Email));
        //     Assert.That(result.Guid, Is.EqualTo(userInfo.Guid));
        //     Assert.That(result.Password, Is.EqualTo(userInfo.Password));
        // }
        //
        // [Test]
        // public void When_logging_in_user_with_incorrect_password()
        // {
        //     var mockAuthorizationData = new Mock<UserDataAccess>();
        //     var guid = Guid.NewGuid();
        //     mockAuthorizationData.Setup(x => x.GetUserInfoByEmail(It.IsAny<string>())).Returns(new UserInfo
        //     {
        //         FirstName = "First Name",
        //         LastName = "Last Name",
        //         Email = "email@email.email",
        //         Guid = guid,
        //         Password = "Password1"
        //     });
        //     mockAuthorizationData.Setup(x => x.VerifyPassword(It.IsAny<UserInfo>())).Returns(false);
        //     mockAuthorizationData.Setup(x => x.AuthorizeUser(It.IsAny<UserInfo>()));
        //
        //     var loginUserController = new LoginUserController(mockAuthorizationData.Object);
        //
        //     var result = loginUserController.Post(new LoginData());
        //
        //     Assert.That(result.FirstName, Is.EqualTo(null));
        //     Assert.That(result.LastName, Is.EqualTo(null));
        //     Assert.That(result.Email, Is.EqualTo(null));
        //     Assert.That(result.Guid, Is.EqualTo(new Guid()));
        //     Assert.That(result.Password, Is.EqualTo(null));
        // }
    }
}

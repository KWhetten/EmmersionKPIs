using System;
using DataAccess.DatabaseAccess;
using DataObjects.Objects;
using KPIWebApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp
{
    public class ForgotPasswordControllerTests
    {
        //     [Test]
        //     public void When_forgot_password()
        //     {
        //         var mockDatabaseAccess = new Mock<UserDataAccess>();
        //         mockDatabaseAccess.Setup(x => x.GetUserInfoByEmail(It.IsAny<string>())).Returns(new UserInfo
        //         {
        //             Email = "email@email.com",
        //             Password = "Password1"
        //         });
        //         var mockEmailManager = new Mock<EmailManager>();
        //         mockEmailManager.Setup(x => x.SendForgotPasswordEmail(It.IsAny<UserInfo>(), It.IsAny<string>()))
        //             .Returns(true);
        //
        //         var forgotPasswordController =
        //             new ForgotPasswordController(mockDatabaseAccess.Object, mockEmailManager.Object);
        //
        //         var result = forgotPasswordController.Post("email@email.com");
        //
        //         Assert.That(result is OkResult, Is.True);
        //     }
        //
        //     [Test]
        //     public void When_forgot_password_with_incorrect_email()
        //     {
        //         var mockEmailManager = new Mock<EmailManager>();
        //         mockEmailManager.Setup(x => x.SendForgotPasswordEmail(It.IsAny<UserInfo>(), It.IsAny<string>()))
        //             .Returns(true);
        //
        //
        //         var mockDatabaseAccess = new Mock<UserDataAccess>();
        //         mockDatabaseAccess.Setup(x => x.GetUserInfoByEmail(It.IsAny<string>())).Throws(new Exception());
        //
        //         var forgotPasswordController = new ForgotPasswordController(mockDatabaseAccess.Object, mockEmailManager.Object);
        //
        //         var result = forgotPasswordController.Post("email@email.com");
        //
        //         Assert.That(result is BadRequestObjectResult, Is.True);
        //     }
        //
        //     [Test]
        //     public void When_forgot_password_has_error()
        //     {
        //         var mockEmailManager = new Mock<EmailManager>();
        //         mockEmailManager.Setup(x => x.SendForgotPasswordEmail(It.IsAny<UserInfo>(), It.IsAny<string>()))
        //             .Returns(false);
        //
        //         var mockDatabaseAccess = new Mock<UserDataAccess>();
        //         mockDatabaseAccess.Setup(x => x.GetUserInfoByEmail(It.IsAny<string>())).Returns(new UserInfo
        //         {
        //             Email = "email@email.com",
        //             Password = "Password1"
        //         });
        //
        //         var forgotPasswordController =
        //             new ForgotPasswordController(mockDatabaseAccess.Object, mockEmailManager.Object);
        //
        //         var result = forgotPasswordController.Post("email@email.com");
        //
        //         Assert.That(result is ObjectResult, Is.True);
        //     }
    }
}

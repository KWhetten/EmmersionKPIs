// using KPIWebApp.Controllers;
// using Microsoft.AspNetCore.Mvc;
// using Moq;
// using NUnit.Framework;
//
// namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp
// {
//     public class RegisterControllerTests
//     {
//         [Test]
//         public void When_registering_new_user()
//         {
//             var mockUserDataAccess = new Mock<UserDataAccess>();
//             mockUserDataAccess.Setup(x => x.InsertUserInfo(
//                     It.IsAny<string>(),
//                     It.IsAny<string>(),
//                     It.IsAny<string>()))
//                 .Returns(1);
//
//             var mockEmailManager = new Mock<EmailManager>();
//             mockEmailManager.Setup(x => x.SendRegistrationEmail(It.IsAny<UserInfo>(), It.IsAny<string>()));
//
//             var registerController = new RegisterController(mockUserDataAccess.Object, mockEmailManager.Object);
//
//             var registerData = new RegisterData
//             {
//                 BaseUrl = "baseurl",
//                 Email = "email@email.email",
//                 FirstName = "First Name",
//                 LastName = "Last Name"
//             };
//
//             var result = registerController.Post(registerData);
//
//             Assert.That(result is OkResult);
//         }
//
//         [Test]
//         public void When_registering_new_user_with_duplicate_email()
//         {
//             var mockUserDataAccess = new Mock<UserDataAccess>();
//             mockUserDataAccess.Setup(x => x.InsertUserInfo(
//                     It.IsAny<string>(),
//                     It.IsAny<string>(),
//                     It.IsAny<string>()))
//                 .Returns(-1);
//
//             var mockEmailManager = new Mock<EmailManager>();
//             mockEmailManager.Setup(x => x.SendRegistrationEmail(It.IsAny<UserInfo>(), It.IsAny<string>()));
//
//             var registerController = new RegisterController(mockUserDataAccess.Object, mockEmailManager.Object);
//
//             var registerData = new RegisterData
//             {
//                 BaseUrl = "baseurl",
//                 Email = "email@email.email",
//                 FirstName = "First Name",
//                 LastName = "Last Name"
//             };
//
//             var result = registerController.Post(registerData);
//
//             Assert.That(result is BadRequestObjectResult);
//         }
//
//         [Test]
//         public void When_registering_new_user_and_error_occurs()
//         {
//             var mockUserDataAccess = new Mock<UserDataAccess>();
//             mockUserDataAccess.Setup(x => x.InsertUserInfo(
//                     It.IsAny<string>(),
//                     It.IsAny<string>(),
//                     It.IsAny<string>()))
//                 .Returns(0);
//
//             var mockEmailManager = new Mock<EmailManager>();
//             mockEmailManager.Setup(x => x.SendRegistrationEmail(It.IsAny<UserInfo>(), It.IsAny<string>()));
//
//             var registerController = new RegisterController(mockUserDataAccess.Object, mockEmailManager.Object);
//
//             var registerData = new RegisterData
//             {
//                 BaseUrl = "baseurl",
//                 Email = "email@email.email",
//                 FirstName = "First Name",
//                 LastName = "Last Name"
//             };
//
//             var result = registerController.Post(registerData);
//
//             Assert.That(result is ObjectResult);
//         }
//     }
// }

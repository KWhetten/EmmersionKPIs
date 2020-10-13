// using KPIWebApp.Controllers;
// using Microsoft.AspNetCore.Mvc;
// using Moq;
// using NUnit.Framework;
//
// namespace KPIDataExtractor.UnitTests.Tests.KPIWebApp
// {
//     public class PasswordChangeControllerTests
//     {
//         [Test]
//         public void When_changing_password()
//         {
//             var mockUserDataAccess = new Mock<UserDataAccess>();
//             mockUserDataAccess.Setup(x => x.InsertPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(1);
//
//             var passwordChangeController = new PasswordChangeController(mockUserDataAccess.Object);
//
//             var changePasswordData = new ChangePasswordData
//             {
//                 Email = "email@email.email",
//                 Password = "Password1"
//             };
//
//             var result = passwordChangeController.Post(changePasswordData);
//
//             Assert.That(result is OkResult);
//         }
//
//         [Test]
//         public void When_changing_password_with_duplicate_email()
//         {
//             var mockUserDataAccess = new Mock<UserDataAccess>();
//             mockUserDataAccess.Setup(x => x.InsertPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(-1);
//
//             var passwordChangeController = new PasswordChangeController(mockUserDataAccess.Object);
//
//             var changePasswordData = new ChangePasswordData
//             {
//                 Email = "email@email.email",
//                 Password = "Password1"
//             };
//
//             var result = passwordChangeController.Post(changePasswordData);
//
//             Assert.That(result is BadRequestObjectResult);
//         }
//
//         [Test]
//         public void When_changing_password_and_error_occurs()
//         {
//             var mockUserDataAccess = new Mock<UserDataAccess>();
//             mockUserDataAccess.Setup(x => x.InsertPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(0);
//
//             var passwordChangeController = new PasswordChangeController(mockUserDataAccess.Object);
//
//             var changePasswordData = new ChangePasswordData
//             {
//                 Email = "email@email.email",
//                 Password = "Password1"
//             };
//
//             var result = passwordChangeController.Post(changePasswordData);
//
//             Assert.That(result is ObjectResult);
//         }
//     }
// }

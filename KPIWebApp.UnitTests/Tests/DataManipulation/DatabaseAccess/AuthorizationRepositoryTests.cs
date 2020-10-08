using System;
using DataAccess.DatabaseAccess;
using DataAccess.Objects;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.DataManipulation.DatabaseAccess
{
    public class AuthorizationRepositoryTests
    {
        private UserRepository userRepository;
        private UserInfo userInfo;


        [SetUp]
        public void SetUp()
        {
            userRepository = new UserRepository();

            userInfo = new UserInfo()
            {
                Email = "email@email.email",
                FirstName = "First",
                LastName = "Last",
                Password = "Password1",
                Guid = Guid.NewGuid()
            };

            userRepository.InsertUserInfo(userInfo.FirstName, userInfo.LastName, userInfo.Email);
            userRepository.InsertPassword(userInfo.Email, userInfo.Password);
        }

        [TearDown]
        public void TearDown()
        {
            userRepository.RemoveUserInfo(userInfo);
        }

        [Test]
        public void When_inserting_user_info()
        {
            var result = userRepository.GetUserInfoByEmail(userInfo.Email);

            Assert.That(result.Email, Is.EqualTo(userInfo.Email));
            Assert.That(result.FirstName, Is.EqualTo(userInfo.FirstName));
            Assert.That(result.LastName, Is.EqualTo(userInfo.LastName));
            Assert.That(result.Password, Is.EqualTo(userInfo.Password));
        }

        [Test]
        public void When_trying_to_insert_user_info_with_existing_email()
        {
            var userInfo2 = new UserInfo()
            {
                Email = "email@email.email",
                FirstName = "First2",
                LastName = "Last2"
            };

            userRepository.InsertUserInfo(userInfo.FirstName, userInfo.LastName, userInfo.Email);
            userRepository.InsertUserInfo(userInfo2.FirstName, userInfo2.LastName, userInfo2.Email);

            var result = userRepository.GetUserInfoByEmail(userInfo.Email);

            Assert.That(result.Email, Is.EqualTo(userInfo.Email));
            Assert.That(result.FirstName, Is.EqualTo(userInfo.FirstName));
            Assert.That(result.LastName, Is.EqualTo(userInfo.LastName));
        }

        [Test]
        public void When_authorizing_user()
        {
            var result = userRepository.AuthorizeUser(userInfo);

            Assert.That(result, Is.True);
        }

        [Test]
        public void When_verifying_password()
        {
            var result = userRepository.VerifyPassword(userInfo);

            Assert.That(result, Is.True);
        }

        [Test]
        public void When_trying_to_authorize_user_that_does_not_exist()
        {
            var result = userRepository.AuthorizeUser(new UserInfo
            {
                Email = "this is not an email in the system",
            });

            Assert.That(result, Is.False);
        }
    }
}

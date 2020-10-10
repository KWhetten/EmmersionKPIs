using System;
using System.Threading.Tasks;
using DataAccess.DatabaseAccess;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using DataManipulation.DatabaseAccess;
using NUnit.Framework;

namespace KPIDataExtractor.UnitTests.Tests.DataManipulation.DatabaseAccess
{
    public class AuthorizationRepositoryTests
    {
        private UserRepository userRepository;
        private UserInfo userInfo;


        [SetUp]
        public async Task SetUp()
        {
            userRepository = new UserRepository(new DatabaseConnection());

            userInfo = new UserInfo()
            {
                Email = "email@email.email",
                FirstName = "First",
                LastName = "Last",
                Password = "Password1",
                Guid = Guid.NewGuid()
            };

            await userRepository.InsertUserInfoAsync(userInfo.FirstName, userInfo.LastName, userInfo.Email);
            await userRepository.InsertPasswordAsync(userInfo.Email, userInfo.Password);
        }

        [TearDown]
        public void TearDown()
        {
            userRepository.RemoveUserInfoAsync(userInfo);
        }

        [Test]
        public async Task When_inserting_user_info()
        {
            var result = await userRepository.GetUserInfoByEmailAsync(userInfo.Email);

            Assert.That(result.Email, Is.EqualTo(userInfo.Email));
            Assert.That(result.FirstName, Is.EqualTo(userInfo.FirstName));
            Assert.That(result.LastName, Is.EqualTo(userInfo.LastName));
            Assert.That(result.Password, Is.EqualTo(userInfo.Password));
        }

        [Test]
        public async Task When_trying_to_insert_user_info_with_existing_email()
        {
            var userInfo2 = new UserInfo()
            {
                Email = "email@email.email",
                FirstName = "First2",
                LastName = "Last2"
            };

            await userRepository.InsertUserInfoAsync(userInfo.FirstName, userInfo.LastName, userInfo.Email);
            await userRepository.InsertUserInfoAsync(userInfo2.FirstName, userInfo2.LastName, userInfo2.Email);

            var result = await userRepository.GetUserInfoByEmailAsync(userInfo.Email);

            Assert.That(result.Email, Is.EqualTo(userInfo.Email));
            Assert.That(result.FirstName, Is.EqualTo(userInfo.FirstName));
            Assert.That(result.LastName, Is.EqualTo(userInfo.LastName));
        }

        [Test]
        public async Task When_authorizing_user()
        {
            var result = await userRepository.AuthorizeUserAsync(userInfo);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task When_verifying_password()
        {
            var result = await userRepository.VerifyPasswordAsync(userInfo);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task When_trying_to_authorize_user_that_does_not_exist()
        {
            var result = await userRepository.AuthorizeUserAsync(new UserInfo
            {
                Email = "this is not an email in the system",
            });

            Assert.That(result, Is.False);
        }
    }
}

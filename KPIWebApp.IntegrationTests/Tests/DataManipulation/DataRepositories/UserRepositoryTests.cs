using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using NUnit.Framework;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.DataRepositories
{
    public class UserRepositoryTests
    {
        UserRepository userRepository = new UserRepository();

        [Test]
        public async Task When_inserting_user_with_duplicate_email()
        {
            var user = new UserInfo
            {
                FirstName = "Kevin",
                LastName = "Whetten",
                Email = "kevin.whetten@emmersion.ai"
            };
            var result = await userRepository.InsertUserAsync(user.FirstName, user.LastName, user.Email);

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public async Task When_inserting_password()
        {
            var user = new UserInfo
            {
                FirstName = "FirstName",
                LastName = "LastName",
                Email = "email@email.email",
                Password = "Password1"
            };
            try
            {
                await userRepository.InsertUserAsync(user.FirstName, user.LastName, user.Email);

                await userRepository.InsertPasswordAsync(user.Email, user.Password);

                var result = await userRepository.GetUserPasswordAsync(user);

                Assert.That(result, Is.EqualTo(user.Password));
            }
            finally
            {
                await userRepository.RemoveUserAsync(user);
            }
        }

        [Test]
        public async Task When_getting_user_by_email()
        {
            var user = new UserInfo
            {
                FirstName = "Kevin",
                LastName = "Whetten",
                Email = "kevin.whetten@emmersion.ai"
            };

            var result = await userRepository.GetUserByEmailAsync(user.Email);

            Assert.That(result.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(result.LastName, Is.EqualTo(user.LastName));
        }

        [Test]
        public async Task When_verifying_password()
        {
            var user = new UserInfo
            {
                FirstName = "FirstName",
                LastName = "LastName",
                Email = "email@email.email",
                Password = "Password1"
            };
            try
            {
                await userRepository.InsertUserAsync(user.FirstName, user.LastName, user.Email);

                await userRepository.InsertPasswordAsync(user.Email, user.Password);

                var result = await userRepository.VerifyPasswordAsync(user);

                Assert.True(result);
            }
            finally
            {
                await userRepository.RemoveUserAsync(user);
            }
        }

        [Test]
        public async Task When_verifying_password_is_incorrect()
        {
            var user = new UserInfo
            {
                FirstName = "FirstName",
                LastName = "LastName",
                Email = "email@email.email",
                Password = "Password1"
            };
            try
            {
                await userRepository.InsertUserAsync(user.FirstName, user.LastName, user.Email);

                await userRepository.InsertPasswordAsync(user.Email, user.Password + "extension");

                var result = await userRepository.VerifyPasswordAsync(user);

                Assert.False(result);
            }
            finally
            {
                await userRepository.RemoveUserAsync(user);
            }
        }
    }
}

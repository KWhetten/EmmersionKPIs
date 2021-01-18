using System;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using NUnit.Framework;

namespace KPIWebApp.IntegrationTests.Tests.DataManipulation.DataRepositories
{
    public class SessionRepositoryTests
    {
        private SessionsRepository sessionsRepository = new SessionsRepository();
        private UserRepository userRepository = new UserRepository();

        [Test]
        public async Task When_authorizing_session()
        {
            var userInfo = new UserInfo
            {
                Email = "email@email.email",
                FirstName = "FirstName1",
                Guid = Guid.NewGuid(),
                LastName = "LastName1",
                Password = "Password1"
            };

            await userRepository.InsertUserAsync(userInfo.FirstName, userInfo.LastName, userInfo.Email);

            var result = await sessionsRepository.AuthorizeSessionAsync(userInfo);

            Assert.True(result);

            await userRepository.RemoveUserAsync(userInfo);
            await sessionsRepository.RemoveSessionAsync(userInfo.Email);
        }

        [Test]
        public async Task When_authorizing_invalid_session()
        {
            UserInfo userInfo = null;

            var result = await sessionsRepository.AuthorizeSessionAsync(userInfo);

            Assert.False(result);
        }

        [Test]
        public async Task When_session_is_authorized()
        {
            var userInfo = new UserInfo
            {
                Email = "email@email.email",
                FirstName = "FirstName1",
                Guid = Guid.NewGuid(),
                LastName = "LastName1",
                Password = "Password1"
            };

            await userRepository.InsertUserAsync(userInfo.FirstName, userInfo.LastName, userInfo.Email);

            await sessionsRepository.AuthorizeSessionAsync(userInfo);

            var result = await sessionsRepository.SessionIsAuthorized(userInfo.Guid);

            Assert.True(result);

            await userRepository.RemoveUserAsync(userInfo);
            await sessionsRepository.RemoveSessionAsync(userInfo.Email);
        }

        [Test]
        public async Task When_session_is_not_authorized()
        {
            var result = await sessionsRepository.SessionIsAuthorized(Guid.NewGuid());

            Assert.False(result);
        }

        [Test]
        public async Task When_session_is_null()
        {
            var result = await sessionsRepository.SessionIsAuthorized(null);

            Assert.False(result);
        }
    }
}

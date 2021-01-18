using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;
using Microsoft.VisualStudio.Services.ClientNotification;

namespace KPIWebApp.Helpers
{
    public class LoginUserHelper
    {
        private readonly IUserRepository userRepository;
        private readonly ISessionsRepository sessionsRepository;

        public LoginUserHelper()
        {
            userRepository = new UserRepository();
            sessionsRepository = new SessionsRepository();
        }

        public LoginUserHelper(IUserRepository userRepository, ISessionsRepository sessionsRepository)
        {
            this.userRepository = userRepository;
            this.sessionsRepository = sessionsRepository;
        }

        public async Task<UserInfo> LoginUserAsync(LoginData data)
        {
                data.Guid = Guid.NewGuid();

                var userInfo = await userRepository.GetUserByEmailAsync(data.Email);
                var verified = await userRepository.VerifyPasswordAsync(userInfo);

                if (!verified) return new UserInfo();

                var result = await sessionsRepository.AuthorizeSessionAsync(userInfo);

                if (result)
                {
                    return userInfo;
                }

                throw new NotAuthorizedException("Session was not created.");
        }

        public class LoginData
        {
            public string Email { get; set; }
            public Guid Guid { get; set; }
        }
    }
}

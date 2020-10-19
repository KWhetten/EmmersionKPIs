using System;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using DataAccess.Objects;

namespace KPIWebApp.Helpers
{
    public class LoginUserHelper
    {
        private readonly IUserRepository userRepository;

        public LoginUserHelper()
        {
            userRepository = new UserRepository(new DatabaseConnection());
        }

        public LoginUserHelper(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        public async Task<UserInfo> LoginUser(LoginData data)
        {
            data.Guid = Guid.NewGuid();

            var userInfo = await userRepository.GetUserInfoByEmailAsync(data.Email);
            var verified = await userRepository.VerifyPasswordAsync(userInfo);

            if (!verified) return new UserInfo();

            await userRepository.AuthorizeUserAsync(userInfo);
            return userInfo;
        }

        public class LoginData
        {
            public string Email { get; set; }
            public Guid Guid { get; set; }
        }
    }
}

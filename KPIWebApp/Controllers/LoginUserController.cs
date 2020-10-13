using System;
 using System.Threading.Tasks;
 using DataAccess.DataRepositories;
 using DataAccess.Objects;
 using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("login-user")]
    public class LoginUserController : ControllerBase
    {
        private readonly UserRepository authorizationData;

        public LoginUserController()
        {
            authorizationData = new UserRepository(new DatabaseConnection());
        }

        // USED FOR TESTING
        // public LoginUserController(UserDataAccess authorizationData)
        // {
        //     this.authorizationData = authorizationData;
        // }

        [HttpPost]
        public async Task<UserInfo> Post(LoginData data)
        {
            data.Guid = Guid.NewGuid();

            var userInfo = await authorizationData.GetUserInfoByEmailAsync(data.Email);
            var verified = await authorizationData.VerifyPasswordAsync(userInfo);

            if (!verified) return new UserInfo();

            await authorizationData.AuthorizeUserAsync(userInfo);
            return userInfo;
        }
    }

    public class LoginData
    {
        public string Email { get; set; }
        public Guid Guid { get; set; }
    }
}

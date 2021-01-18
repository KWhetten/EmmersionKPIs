using System.Threading.Tasks;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("login-user")]
    public class LoginUserController : ControllerBase
    {

        [HttpPost]
        public async Task<UserInfo> Post(LoginUserHelper.LoginData data)
        {
            var loginUserHelper = new LoginUserHelper();
            return await loginUserHelper.LoginUserAsync(data);
        }
    }
}

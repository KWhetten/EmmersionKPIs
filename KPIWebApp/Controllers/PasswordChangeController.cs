using System.Threading.Tasks;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("change-password")]
    public class PasswordChangeController : ControllerBase
    {
        private const int DuplicateEmail = -1;
        private const int RegistrationError = 0;

        [HttpPost]
        public async Task<IActionResult> Post(PasswordChangeHelper.ChangePasswordData data)
        {
            var passwordChangeHelper = new PasswordChangeHelper();
            return await passwordChangeHelper.UpdatePassword(data.Email, data.Password) switch
            {
                DuplicateEmail => BadRequest("User with that email already exists."),
                RegistrationError => StatusCode(500, "An error occurred while trying to set password. Please try again later."),
                _ => Ok()
            };
        }
    }
}

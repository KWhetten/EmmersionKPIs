using System.Threading.Tasks;
using DataAccess.Objects;
using KPIWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegisterController : ControllerBase
    {
        private const int DuplicateEmail = -1;
        private const int RegistrationError = 0;

        [HttpPost]
        public async Task<IActionResult> Post(RegisterHelper.RegisterData data)
        {
            var registerHelper = new RegisterHelper();
            var emailHelper = new EmailHelper();

            var result = await registerHelper.RegisterUser(data);

            if (result != DuplicateEmail && result != RegistrationError)
            {
                var userInfo = new UserInfo
                {
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    Email = data.Email
                };
                emailHelper.SendRegistrationEmail(userInfo, data.BaseUrl);
            }

            return result switch
            {
                DuplicateEmail => BadRequest("User with that email already exists."),
                RegistrationError => StatusCode(500, "An error occurred while trying to register. Please try again later."),
                _ => Ok()
            };
        }
    }


}

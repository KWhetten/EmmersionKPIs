using DataAccess.DatabaseAccess;
using DataManipulation.DatabaseAccess;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("password-change")]
    public class PasswordChangeController : ControllerBase
    {
        private readonly UserDataAccess userDataAccess;
        private const int DuplicateEmail = -1;
        private const int RegistrationError = 0;

        public PasswordChangeController()
        {
            userDataAccess = new UserDataAccess();
        }

        // USED FOR TESTING
        // public PasswordChangeController(UserDataAccess userDataAccess)
        // {
        //     this.userDataAccess = userDataAccess;
        // }

        [HttpPost]
        public IActionResult Post(ChangePasswordData data)
        {
            var result = userDataAccess.InsertPassword(data.Email, data.Password);

            return result switch
            {
                DuplicateEmail => BadRequest("User with that email already exists."),
                RegistrationError => StatusCode(500, "An error occurred while trying to register. Please try again later."),
                _ => Ok()
            };
        }
    }

    public class ChangePasswordData
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

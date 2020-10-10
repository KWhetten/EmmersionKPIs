using System.Threading.Tasks;
 using DataAccess.DatabaseAccess;
using DataAccess.DataRepositories;
using DataAccess.Objects;
 using DataManipulation.DatabaseAccess;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly UserRepository userRepository;
        private readonly EmailManager emailManager;
        private const int DuplicateEmail = -1;
        private const int RegistrationError = 0;

        public RegisterController()
        {
            userRepository = new UserRepository(new DatabaseConnection());
            emailManager = new EmailManager();
        }

        // USED FOR TESTING:
        // public RegisterController(UserDataAccess userDataAccess, EmailManager emailManager)
        // {
        //     this.userDataAccess = userDataAccess;
        //     this.emailManager = emailManager;
        // }

        [HttpPost]
        public async Task<IActionResult> Post(RegisterData data)
        {
            var result = await userRepository.InsertUserInfoAsync(data.FirstName, data.LastName, data.Email);

            var userInfo = new UserInfo
            {
                Email = data.Email,
                FirstName = data.FirstName,
                LastName = data.LastName
            };

            if (result != DuplicateEmail && result != RegistrationError)
            {
                emailManager.SendRegistrationEmail(userInfo, data.BaseUrl);
            }

            return result switch
            {
                DuplicateEmail => BadRequest("User with that email already exists."),
                RegistrationError => StatusCode(500, "An error occurred while trying to register. Please try again later."),
                _ => Ok()
            };
        }
    }

    public class RegisterData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string BaseUrl { get; set; }
    }
}

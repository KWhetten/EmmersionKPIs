using DataAccess.DatabaseAccess;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegisterController : ControllerBase
    {
        [HttpPost]
        public bool Post(string firstName, string lastName, string email, string password)
        {
            var databaseWrapper = new DatabaseWrapper();
            return databaseWrapper.InsertUserInfo(firstName, lastName, email, password);
        }
    }
}

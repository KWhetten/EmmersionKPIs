using System;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("authorize-user")]
    public class AuthorizeUserController
    {
        public bool Get(string guid)
        {
            var userRepository = new UserRepository();

            try
            {
                return userRepository.SessionIsAuthorized(Guid.Parse(guid));
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

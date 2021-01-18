using System;
using System.Threading.Tasks;
using DataAccess.DataRepositories;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("authorize-user")]
    public class AuthorizeSessionController
    {
        public async Task<bool> Get(string guid)
        {
            var userRepository = new SessionsRepository();

            try
            {
                return await userRepository.SessionIsAuthorized(Guid.Parse(guid));
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

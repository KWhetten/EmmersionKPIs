using System;
 using System.Threading.Tasks;
 using KPIWebApp.Helpers;
 using Microsoft.AspNetCore.Mvc;

 namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ForgotPasswordController : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> Post(string email)
        {
            try
            {
                var forgotPasswordHelper = new ForgotPasswordHelper();
                var result = await forgotPasswordHelper.SendEmail(email);
                if (result == false)
                    return StatusCode(500,
                        "An error occurred while trying to send the password reset email. Please try again later.");
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("User with that email does not exist.");
            }
        }
    }
}

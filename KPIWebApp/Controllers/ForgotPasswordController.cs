﻿using System;
 using System.Threading.Tasks;
 using DataAccess.DataRepositories;
 using Microsoft.AspNetCore.Mvc;

 namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly UserRepository userRepository;
        private readonly EmailManager emailManager;

        public ForgotPasswordController()
        {
            userRepository = new UserRepository(new DatabaseConnection());
            emailManager = new EmailManager();
        }

        // USED FOR TESTING
        // public ForgotPasswordController(UserDataAccess userDataAccess, EmailManager emailManager)
        // {
        //     this.emailManager = emailManager;
        //     this.userDataAccess = userDataAccess;
        // }

        [HttpPost]
        public async Task<IActionResult> Post(string email)
        {
            try
            {
                var userInfo = await userRepository.GetUserInfoByEmailAsync(email);
                var result = emailManager.SendForgotPasswordEmail(userInfo, "");

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

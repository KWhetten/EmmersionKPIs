﻿using System;
using DataAccess.DatabaseAccess;
 using DataAccess.Objects;
 using DataObjects.Objects;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace KPIWebApp.Controllers
{
    [ApiController]
    [Route("login-user")]
    public class LoginUserController : ControllerBase
    {
        private readonly UserDataAccess authorizationData;

        public LoginUserController()
        {
            authorizationData = new UserDataAccess();
        }

        // USED FOR TESTING
        // public LoginUserController(UserDataAccess authorizationData)
        // {
        //     this.authorizationData = authorizationData;
        // }

        [HttpPost]
        public UserInfo Post(LoginData data)
        {
            data.Guid = Guid.NewGuid();

            var userInfo = authorizationData.GetUserInfoByEmail(data.Email);
            var verified = authorizationData.VerifyPassword(userInfo);

            if (!verified) return new UserInfo();

            authorizationData.AuthorizeUser(userInfo);
            return userInfo;
        }
    }

    public class LoginData
    {
        public string Email { get; set; }
        public Guid Guid { get; set; }
    }
}

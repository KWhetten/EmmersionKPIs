using System;

 namespace DataAccess.Objects
{
    public class UserInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Guid Guid { get; set; }
    }
}

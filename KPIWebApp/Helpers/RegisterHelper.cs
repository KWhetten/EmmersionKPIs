using System.Threading.Tasks;
using DataAccess.DataRepositories;

namespace KPIWebApp.Helpers
{
    public class RegisterHelper
    {
        private readonly IUserRepository userRepository;

        public RegisterHelper()
        {
            userRepository = new UserRepository();
        }
        public RegisterHelper(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<int> RegisterUser(RegisterData data)
        {
            return await userRepository.InsertUserInfoAsync(data.FirstName, data.LastName, data.Email);
        }

        public class RegisterData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string BaseUrl { get; set; }
        }
    }
}

using System.Threading.Tasks;
using DataAccess.DataRepositories;

namespace KPIWebApp.Helpers
{
    public class PasswordChangeHelper
    {
        private readonly IUserRepository userRepository;

        public PasswordChangeHelper()
        {
            userRepository = new UserRepository();
        }
        public PasswordChangeHelper(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        public async Task<int> UpdatePassword(string email, string password)
        {
            return await userRepository.InsertPasswordAsync(email, password);
        }

        public class ChangePasswordData {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}

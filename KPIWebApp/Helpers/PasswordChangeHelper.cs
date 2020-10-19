using System.Threading.Tasks;
using DataAccess.DataRepositories;

namespace KPIWebApp.Helpers
{
    public class PasswordChangeHelper
    {
        private readonly IUserRepository userRepository;

        public PasswordChangeHelper()
        {
            userRepository = new UserRepository(new DatabaseConnection());
        }
        public PasswordChangeHelper(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        public async Task<int> UpdatePassword(ChangePasswordData data)
        {
            return await userRepository.InsertPasswordAsync(data.Email, data.Password);
        }

        public class ChangePasswordData
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}

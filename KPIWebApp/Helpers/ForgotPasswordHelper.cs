using System.Threading.Tasks;
using DataAccess.DataRepositories;

namespace KPIWebApp.Helpers
{
    public class ForgotPasswordHelper
    {
        private readonly IUserRepository userRepository;
        private readonly EmailHelper emailHelper;

        public ForgotPasswordHelper()
        {
            userRepository = new UserRepository();
            emailHelper = new EmailHelper();
        }

        public ForgotPasswordHelper(IUserRepository userRepository, EmailHelper emailHelper)
        {
            this.userRepository = userRepository;
            this.emailHelper = emailHelper;
        }

        public async Task<bool> SendEmail(string email)
        {
            var userInfo = await userRepository.GetUserByEmailAsync(email);
            return emailHelper.SendForgotPasswordEmail(userInfo, "");
        }
    }
}

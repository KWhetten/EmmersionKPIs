using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccess.Objects;

namespace DataAccess.DataRepositories
{
    public interface ISessionsRepository
    {
        Task<bool> AuthorizeSessionAsync(UserInfo userInfo);
        Task<bool> SessionIsAuthorized(Guid? guid);
        Task RemoveSessionAsync(string email);
    }

    public class SessionsRepository : ISessionsRepository
    {
        private readonly DatabaseConnection databaseConnection;
        private IUserRepository userRepository;

        public SessionsRepository()
        {
            databaseConnection = new DatabaseConnection();
            userRepository = new UserRepository();
        }

        public virtual async Task<bool> AuthorizeSessionAsync(UserInfo userInfo)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                try
                {
                    var sql = $"DELETE FROM Sessions WHERE Expires < GETDATE();";
                    await databaseConnection.DbConnection.ExecuteAsync(sql);

                    var guid = userInfo.Guid.ToString();
                    var email = userInfo.Email;
                    var now = DateTimeOffset.Now.AddHours(2).ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'ff");
                    sql = $"INSERT INTO Sessions (Guid, Email, Expires) " +
                          $"VALUES (@guid, @email, @now);";
                    await databaseConnection.DbConnection.ExecuteAsync(sql, new {guid, email, now});

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public async Task<bool> SessionIsAuthorized(Guid? guid)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM Sessions WHERE Guid = @guid";
                var result = await databaseConnection.DbConnection.QueryAsync(sql, new {guid});
                return result.Any();
            }
        }

        public async Task RemoveSessionAsync(string email)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"DELETE FROM Sessions WHERE email = @email;";
                await databaseConnection.DbConnection.ExecuteAsync(sql, new {email});
            }
        }
    }
}

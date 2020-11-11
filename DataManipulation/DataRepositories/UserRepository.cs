using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccess.Objects;

namespace DataAccess.DataRepositories
{
    public interface IUserRepository
    {
        Task<int> InsertUserInfoAsync(string firstName, string lastName, string email);
        Task<int> InsertPasswordAsync(string email, string password);
        public Task<bool> AuthorizeUserAsync(UserInfo userInfo);
        public Task<UserInfo> GetUserInfoByEmailAsync(string email);
        public Task RemoveUserInfoAsync(UserInfo userInfo);
        public Task<bool> VerifyPasswordAsync(UserInfo userInfo);
    }

    public class UserRepository : IUserRepository
    {
        private readonly DatabaseConnection databaseConnection;

        public UserRepository()
        {
            databaseConnection = new DatabaseConnection();
        }
        public UserRepository(DatabaseConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }
        public virtual async Task<int> InsertUserInfoAsync(string firstName, string lastName, string email)
        {
            databaseConnection.GetNewConnection();
            const int duplicateEmailCode = -1;
            await using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM UserInfo WHERE Email = @email;";
                var result = await databaseConnection.DbConnection.QueryAsync(sql, new {email});
                if (result.Any())
                {
                    return duplicateEmailCode;
                }

                sql = $"INSERT INTO UserInfo (FirstName, LastName, Email)" +
                      $"VALUES (@firstName, @lastName, @email)";
                return await databaseConnection.DbConnection.ExecuteAsync(sql, new {firstName, lastName, email});
            }
        }

        public virtual async Task<int> InsertPasswordAsync(string email, string password)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"UPDATE UserInfo SET Password = @password WHERE email = @email";
                return await databaseConnection.DbConnection.ExecuteAsync(sql, new {password, email});
            }
        }

        public virtual async Task<bool> AuthorizeUserAsync(UserInfo userInfo)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                try
                {
                    var sql = $"DELETE FROM AuthorizedUsers WHERE Expires < GETDATE();";
                    await databaseConnection.DbConnection.ExecuteAsync(sql);

                    var guid = userInfo.Guid.ToString();
                    var email = userInfo.Email;
                    var now = DateTimeOffset.Now.AddHours(2).ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'ff");
                    sql = $"INSERT INTO AuthorizedUsers (Guid, Email, Expires) " +
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

        public virtual async Task<UserInfo> GetUserInfoByEmailAsync(string email)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM UserInfo WHERE email = @email;";
                var infos = await databaseConnection.DbConnection.QueryAsync<UserInfo>(sql, new {email});
                var info = infos.First();
                info.Guid = Guid.NewGuid();
                return info;
            }
        }

        private async Task<string> GetUserPasswordAsync(UserInfo userInfo)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var email = userInfo.Email;
                var sql = $"SELECT Password FROM UserInfo WHERE email = @email;";
                var passwords = await databaseConnection.DbConnection.QueryAsync<string>(sql, new {email});
                return passwords.First();
            }
        }

        public async Task RemoveUserInfoAsync(UserInfo userInfo)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var email = userInfo.Email;
                var sql = $"DELETE FROM AuthorizedUsers WHERE email = @email;";
                await databaseConnection.DbConnection.ExecuteAsync(sql, new {email});

                sql = $"DELETE FROM UserInfo WHERE email = @email;";
                await databaseConnection.DbConnection.ExecuteAsync(sql, new {email});
            }
        }

        public virtual async Task<bool> VerifyPasswordAsync(UserInfo userInfo)
        {
            var password = await GetUserPasswordAsync(userInfo);
            return userInfo.Password == password;
        }
    }
}

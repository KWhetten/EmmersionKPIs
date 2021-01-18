using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccess.Objects;

namespace DataAccess.DataRepositories
{
    public interface IUserRepository
    {
        Task<int> InsertUserAsync(string firstName, string lastName, string email);
        Task<int> InsertPasswordAsync(string email, string password);
        public Task<UserInfo> GetUserByEmailAsync(string email);
        public Task RemoveUserAsync(UserInfo userInfo);
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
        public virtual async Task<int> InsertUserAsync(string firstName, string lastName, string email)
        {
            databaseConnection.GetNewConnection();
            const int duplicateEmailCode = -1;
            await using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM Users WHERE Email = @email;";
                var result = await databaseConnection.DbConnection.QueryAsync(sql, new {email});
                if (result.Any())
                {
                    return duplicateEmailCode;
                }

                sql = $"INSERT INTO Users (FirstName, LastName, Email)" +
                      $"VALUES (@firstName, @lastName, @email)";
                return await databaseConnection.DbConnection.ExecuteAsync(sql, new {firstName, lastName, email});
            }
        }

        public virtual async Task<int> InsertPasswordAsync(string email, string password)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"UPDATE Users SET Password = @password WHERE email = @email";
                return await databaseConnection.DbConnection.ExecuteAsync(sql, new {password, email});
            }
        }

        public virtual async Task<UserInfo> GetUserByEmailAsync(string email)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM Users WHERE email = @email;";
                var infos = await databaseConnection.DbConnection.QueryAsync<UserInfo>(sql, new {email});
                var info = infos.First();
                info.Guid = Guid.NewGuid();
                return info;
            }
        }

        public async Task<string> GetUserPasswordAsync(UserInfo userInfo)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var email = userInfo.Email;
                var sql = $"SELECT Password FROM Users WHERE email = @email;";
                var passwords = (await databaseConnection.DbConnection.QueryAsync<string>(sql, new {email})).ToList();
                return passwords.First();
            }
        }

        public async Task RemoveUserAsync(UserInfo userInfo)
        {
            var authorizedUsersRepository = new SessionsRepository();

            var email = userInfo.Email;

            await authorizedUsersRepository.RemoveSessionAsync(email);

            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"DELETE FROM Users WHERE email = @email;";
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

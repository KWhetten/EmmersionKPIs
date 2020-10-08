using System;
using System.Linq;
using Dapper;
using DataAccess.Objects;
using DataManipulation.DatabaseAccess;

namespace DataAccess.DatabaseAccess
{
    public interface IUserRepository
    {
        int InsertUserInfo(string firstName, string lastName, string email);
        int InsertPassword(string email, string password);
        public bool AuthorizeUser(UserInfo userInfo);
        public UserInfo GetUserInfoByEmail(string email);
        public void RemoveUserInfo(UserInfo userInfo);
        public bool VerifyPassword(UserInfo userInfo);
    }

    public class UserRepository : IUserRepository
    {
        public virtual int InsertUserInfo(string firstName, string lastName, string email)
        {
            DatabaseConnection.GetNewConnection();
            const int duplicateEmailCode = -1;
            using (DatabaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM UserInfo WHERE Email = '{email}';";
                var result = DatabaseConnection.DbConnection.Query(sql);
                if (result.Any())
                {
                    return duplicateEmailCode;
                }

                sql = $"INSERT INTO UserInfo (FirstName, LastName, Email)" +
                      $"VALUES ('{firstName}', '{lastName}', '{email}')";
                return DatabaseConnection.DbConnection.Execute(sql);
            }
        }

        public virtual int InsertPassword(string email, string password)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                var sql = $"UPDATE UserInfo SET Password = '{password}' WHERE email = '{email}'";
                return DatabaseConnection.DbConnection.Execute(sql);
            }
        }

        public virtual bool AuthorizeUser(UserInfo userInfo)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                try
                {
                    var sql = $"DELETE FROM AuthorizedUsers WHERE Expires < GETDATE();";
                    DatabaseConnection.DbConnection.Execute(sql);

                    sql = $"INSERT INTO AuthorizedUsers (Guid, Email, Expires) " +
                          $"VALUES ('{userInfo.Guid.ToString()}', '{userInfo.Email}', '{DateTime.Now.AddHours(2):yyyy'-'MM'-'dd HH':'mm':'ss'.'ff}');";
                    DatabaseConnection.DbConnection.Execute(sql);

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public virtual UserInfo GetUserInfoByEmail(string email)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM UserInfo WHERE email = '{email}';";
                var info = DatabaseConnection.DbConnection.Query<UserInfo>(sql).First();
                info.Guid = Guid.NewGuid();
                return info;
            }
        }

        private string GetUserPassword(UserInfo userInfo)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                var sql = $"SELECT Password FROM UserInfo WHERE email = '{userInfo.Email}';";
                return DatabaseConnection.DbConnection.Query<string>(sql).First();
            }
        }

        public void RemoveUserInfo(UserInfo userInfo)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                var sql = $"DELETE FROM AuthorizedUsers WHERE email = '{userInfo.Email}';";
                DatabaseConnection.DbConnection.Execute(sql);

                sql = $"DELETE FROM UserInfo WHERE email = '{userInfo.Email}';";
                DatabaseConnection.DbConnection.Execute(sql);
            }
        }

        public virtual bool VerifyPassword(UserInfo userInfo)
        {
            var password = GetUserPassword(userInfo);
            return userInfo.Password == password;
        }
    }
}

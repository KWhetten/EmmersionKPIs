using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DataAccess.DataRepositories
{
    public interface IDatabaseConnection
    {
        void GetNewConnection();
        DbConnection GetDbConnection();
    }

    public class DatabaseConnection : IDatabaseConnection
    {
        public SqlConnection DbConnection;

        public DatabaseConnection()
        {
            GetNewConnection();
        }

        public virtual DbConnection GetDbConnection()
        {
            return DbConnection;
        }

        public virtual void GetNewConnection()
        {
            DbConnection = new SqlConnection(File.ReadLines($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/EmmersionKPI/databaseConnectionString.txt").First());
        }
    }
}

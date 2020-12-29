using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DataAccess.DataRepositories
{
    public interface IDatabaseConnection
    {
    }

    public class DatabaseConnection : IDatabaseConnection
    {
        public SqlConnection DbConnection;

        public DatabaseConnection()
        {
            GetNewConnection();
        }

        public void GetNewConnection()
        {
            DbConnection = new SqlConnection(File.ReadLines($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/EmmersionKPI/databaseConnectionString.txt").First());
        }
    }
}

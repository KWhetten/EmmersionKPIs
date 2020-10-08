using System.Data.SqlClient;
using DataWrapper.DatabaseAccess;

namespace DataManipulation.DatabaseAccess
{
    public interface IDatabaseConnection
    {
    }

    public abstract class DatabaseConnection : IDatabaseConnection
    {
        public static SqlConnection DbConnection;

        static DatabaseConnection()
        {
            DbConnection =
                new SqlConnection(
                    Helper.ConnectionValue("EmmersionMetrics"));
        }

        public static void GetNewConnection()
        {
            DbConnection =
                new SqlConnection(
                    Helper.ConnectionValue("EmmersionMetrics"));
        }
    }
}

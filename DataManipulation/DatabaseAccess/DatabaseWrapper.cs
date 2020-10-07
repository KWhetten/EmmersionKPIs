using System.Data.SqlClient;

namespace DataManipulation.DatabaseAccess
{
    public interface IDatabaseWrapper
    {
    }

    public abstract class DatabaseWrapper : IDatabaseWrapper
    {
        public static SqlConnection DbConnection;

        static DatabaseWrapper()
        {
            DbConnection =
                new SqlConnection(
                    "Server=localhost,14330;Database=EmmersionMetrics;User Id=sa;Password=truenorth123!;;");
        }

        public static void GetNewConnection()
        {
            DbConnection =
                new SqlConnection(
                    "Server=localhost,14330;Database=EmmersionMetrics;User Id=sa;Password=truenorth123!;;");
        }
    }
}

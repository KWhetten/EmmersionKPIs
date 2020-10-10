using System.Data.SqlClient;
using DataAccess.DatabaseAccess;

namespace DataManipulation.DatabaseAccess
{
    public interface IDatabaseConnection
    {
    }

    public class DatabaseConnection : IDatabaseConnection
    {
        public SqlConnection DbConnection;

        public DatabaseConnection()
        {
            DbConnection =
                new SqlConnection("Server=localhost,14330;Database=EmmersionMetrics;User Id=sa;Password=truenorth123!;MultipleActiveResultSets=True;");
        }

        public void GetNewConnection()
        {
            DbConnection =
                new SqlConnection("Server=localhost,14330;Database=EmmersionMetrics;User Id=sa;Password=truenorth123!;MultipleActiveResultSets=True;");
        }
    }
}

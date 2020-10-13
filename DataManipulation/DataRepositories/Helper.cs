using System.Configuration;

namespace DataAccess.DataRepositories
{
    public static class Helper
    {
        public static string ConnectionValue(string databaseName)
        {
            return ConfigurationManager.ConnectionStrings[databaseName].ConnectionString;
        }
    }
}

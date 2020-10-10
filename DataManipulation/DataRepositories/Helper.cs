using System.Configuration;

 namespace DataAccess.DatabaseAccess
{
    public static class Helper
    {
        public static string ConnectionValue(string databaseName)
        {
            return ConfigurationManager.ConnectionStrings[databaseName].ConnectionString;
        }
    }
}

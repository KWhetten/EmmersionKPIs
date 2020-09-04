using System.Configuration;

namespace KPIDataExtractor.DatabaseAccess
{
    public static class Helper
    {
        public static string ConnectionValue(string databaseName)
        {
            return ConfigurationManager.ConnectionStrings[databaseName].ConnectionString;
        }
    }
}

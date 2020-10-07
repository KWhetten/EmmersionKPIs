﻿using System.Configuration;

namespace DataWrapper.DatabaseAccess
{
    public static class Helper
    {
        public static string ConnectionValue(string databaseName)
        {
            return ConfigurationManager.ConnectionStrings[databaseName].ConnectionString;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DataObjects.Objects;
using Microsoft.TeamFoundation.Common;

namespace DataManipulation.DatabaseAccess
{
    public abstract class DatabaseWrapper
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

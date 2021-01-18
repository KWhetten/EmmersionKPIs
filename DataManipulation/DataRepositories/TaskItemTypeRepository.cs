using System.Linq;
using Dapper;
using DataAccess.Objects;

namespace DataAccess.DataRepositories
{
    public class TaskItemTypeRepository
    {
        private DatabaseConnection databaseConnection;
        public TaskItemTypeRepository()
        {
            databaseConnection = new DatabaseConnection();
        }

        public TaskItemType[] GetTaskItemTypes()
        {
            databaseConnection.GetNewConnection();
            using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT Id FROM TaskItemTypes";
                var taskItemTypes = databaseConnection.DbConnection
                    .Query<TaskItemType>(sql);
                return taskItemTypes.ToArray();
            }
        }
    }
}

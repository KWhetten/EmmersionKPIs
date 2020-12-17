using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace DataAccess.DataRepositories
{
    public class DeveloperRepository
    {
        private readonly DatabaseConnection databaseConnection;

        public DeveloperRepository()
        {
            databaseConnection = new DatabaseConnection();
        }

        public async Task SaveDeveloperAsync(string developerName)
        {
            var sql = @"IF NOT EXISTS(SELECT * FROM Developers WHERE Name = @developerName)
                            INSERT INTO Developers (Name)
                            VALUES (@developerName)";

            databaseConnection.GetNewConnection();
            await databaseConnection.DbConnection.ExecuteAsync(sql, new {developerName});
        }

        public async Task<string> GetDeveloperByNameAsync(string name)
        {
            var sql = "SELECT * FROM Developers WHERE Name = @name";

            databaseConnection.GetNewConnection();
            var result = await databaseConnection.DbConnection.QueryAsync(sql, new {name});
            return result.First().Name;
        }

        public async Task RemoveDeveloperByNameAsync(string name)
        {
            var sql = "DELETE FROM Developers WHERE Name = @name";

            databaseConnection.GetNewConnection();
            await databaseConnection.DbConnection.ExecuteAsync(sql, new {name});
        }
    }
}

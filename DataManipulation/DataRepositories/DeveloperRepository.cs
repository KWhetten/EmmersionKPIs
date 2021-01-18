using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccess.Objects;

namespace DataAccess.DataRepositories
{
    public class DeveloperRepository
    {
        private readonly DatabaseConnection databaseConnection;

        public DeveloperRepository()
        {
            databaseConnection = new DatabaseConnection();
        }

        public async Task InsertDeveloperAsync(string developerName)
        {
            var sql = @"IF NOT EXISTS(SELECT * FROM Developers WHERE Name = @developerName)
                            INSERT INTO Developers (Name)
                            VALUES (@developerName)";

            databaseConnection.GetNewConnection();
            await databaseConnection.DbConnection.ExecuteAsync(sql, new {developerName});
        }

        public async Task<Developer> GetDeveloperByNameAsync(string name)
        {
            var sql = "SELECT * FROM Developers WHERE Name = @name";

            databaseConnection.GetNewConnection();
            var result = await databaseConnection.DbConnection.QueryAsync(sql, new {name});
            return new Developer
            {
                Id = result.First().Id,
                Name = result.First().Name
            };
        }

        public async Task RemoveDeveloperByNameAsync(string name)
        {
            var sql = "DELETE FROM Developers WHERE Name = @name";

            databaseConnection.GetNewConnection();
            await databaseConnection.DbConnection.ExecuteAsync(sql, new {name});
        }

        public async Task<Developer> GetDeveloperByIdAsync(int id)
        {
            var sql = "SELECT * FROM Developers WHERE Id = @id";

            databaseConnection.GetNewConnection();
            var result = await databaseConnection.DbConnection.QueryAsync(sql, new {id});
            return new Developer
            {
                Id = result.First().Id,
                Name = result.First().Name
            };
        }
    }
}

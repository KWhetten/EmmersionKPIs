using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccess.Objects;

namespace DataAccess.DataRepositories
{
    public class DevelopmentTeamsRepository
    {
        DatabaseConnection databaseConnection;

        public DevelopmentTeamsRepository()
        {
            databaseConnection = new DatabaseConnection();
        }
        public async Task InsertDevTeamAsync(int id, string name)
        {
            var sql = @"IF NOT EXISTS (SELECT * FROM DevelopmentTeams WHERE Id = @id)
                            INSERT INTO DevelopmentTeams (Id, Name)
                            VALUES (@id, @name)";

            await databaseConnection.DbConnection.ExecuteAsync(sql, new { id, name });
        }

        public async Task<DevelopmentTeam> GetTeamAsync(int id)
        {
            var sql = "SELECT * FROM DevelopmentTeams WHERE Id = @id";

            var result = (await databaseConnection.DbConnection.QueryAsync<DevelopmentTeam>(sql, new {id})).First();

            return result;
        }

        public async Task RemoveTeam(int id)
        {
            var sql = "DELETE FROM DevelopmentTeams WHERE Id = @id";

            await databaseConnection.DbConnection.ExecuteAsync(sql, new { id });
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace DataAccess.DataRepositories
{
    public class DevelopmentTeamsRepository
    {
        DatabaseConnection databaseConnection;

        public DevelopmentTeamsRepository()
        {
            databaseConnection = new DatabaseConnection();
        }
        public async Task SaveTeamAsync(int id, string name)
        {
            var sql = @"IF NOT EXISTS (SELECT * FROM DevelopmentTeams WHERE Id = @id)
                            INSERT INTO DevelopmentTeams (Id, Name)
                            VALUES (@id, @name)";

            await databaseConnection.DbConnection.ExecuteAsync(sql, new { id, name });
        }

        public async Task<DevTeam> GetTeam(int id)
        {
            var sql = "SELECT * FROM DevelopmentTeams WHERE Id = @id";

            return (await databaseConnection.DbConnection.QueryAsync<DevTeam>(sql, new { id })).First();
        }

        public async Task RemoveTeam(int id)
        {
            var sql = "DELETE FROM DevelopmentTeams WHERE Id = @id";

            await databaseConnection.DbConnection.ExecuteAsync(sql, new { id });
        }
    }

    public class DevTeam
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

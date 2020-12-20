﻿using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccess.Objects;

namespace DataAccess.DataRepositories
{
    public interface IReleaseEnvironmentRepository
    {
        Task<ReleaseEnvironment> GetReleaseEnvironmentByIdAsync(int releaseEnvironmentId);
        Task RemoveReleaseEnvironmentById(int releaseEnvironmentId);
        Task SaveReleaseEnvironmentAsync(int id, string name);
    }

    public class ReleaseEnvironmentRepository : IReleaseEnvironmentRepository
    {
        private readonly DatabaseConnection databaseConnection;

        public ReleaseEnvironmentRepository()
        {
            databaseConnection = new DatabaseConnection();
        }

        public ReleaseEnvironmentRepository(DatabaseConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        public async Task<ReleaseEnvironment> GetReleaseEnvironmentByIdAsync(int releaseEnvironmentId)
        {
            var sql = $"SELECT * FROM ReleaseEnvironment WHERE Id = @releaseEnvironmentId;";
            var releaseEnvironment = await databaseConnection.DbConnection
                .QueryAsync<ReleaseEnvironment>(sql, new {releaseEnvironmentId});
            return releaseEnvironment.First();
        }

        public async Task SaveReleaseEnvironmentAsync(int id, string name)
        {
            var sql = @"IF NOT EXISTS(SELECT * FROM ReleaseEnvironment WHERE Id = @id)
                            INSERT INTO ReleaseEnvironment (Id, Name)
                            VALUES (@id, @name);
                        ELSE
                            UPDATE ReleaseEnvironment SET Name = @name WHERE Id = @id";
            await databaseConnection.DbConnection.ExecuteAsync(sql, new {id, name});
        }

        public async Task RemoveReleaseEnvironmentById(int releaseEnvironmentId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"DELETE FROM ReleaseEnvironment WHERE Id = @releaseEnvironmentId";
                await databaseConnection.DbConnection.ExecuteAsync(sql, new {releaseEnvironmentId});
            }
        }
    }
}

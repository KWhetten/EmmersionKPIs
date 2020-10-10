using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;

namespace DataAccess.DataRepositories
{
    public interface IReleaseRepository
    {
        Task InsertReleaseListAsync(IEnumerable<Release> releases);
        Task<IEnumerable<Release>> GetReleaseListAsync(DateTime startDate, DateTime endDate);
        Task InsertReleaseAsync(Release release);
        Task<List<Release>> GetReleasesBeforeDateAsync(DateTime finishTime);
        Task<ReleaseEnvironment> GetReleaseEnvironmentByIdAsync(int releaseEnvironmentId);
        Task<Release> GetReleaseByIdAsync(int releaseId);
        Task RemoveReleaseByIdAsync(int releaseId);
        Task RemoveReleaseEnvironmentById(int releaseEnvironmentId);
    }

    public class ReleaseRepository : IReleaseRepository
    {
        private readonly DatabaseConnection databaseConnection;

        public ReleaseRepository(DatabaseConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        public async Task InsertReleaseListAsync(IEnumerable<Release> releases)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                if (releases == null) return;
                foreach (var release in releases) await InsertReleaseAsync(release);
            }
        }

        public async Task<IEnumerable<Release>> GetReleaseListAsync(DateTime startDate, DateTime endDate)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var startDateString = ($"{startDate:s}").Replace("T", " ");
                var endDateString = ($"{endDate:s}").Replace("T", " ");
                var sql = $"SELECT * FROM Release WHERE FinishTime > @startDateString AND FinishTime < @endDateString";
                var releases = await databaseConnection.DbConnection
                    .QueryAsync<Release>(sql, new {startDateString, endDateString});

                var releaseList = releases.ToList();

                foreach (var release in releaseList)
                {
                    try
                    {
                        sql = $"SELECT ReleaseEnvironmentId FROM Release WHERE Id = @release.Id";
                        var releaseEnvironmentId = await databaseConnection.DbConnection
                            .QueryAsync<int>(sql, new {release.Id});
                        sql = $"SELECT * FROM ReleaseEnvironment WHERE Id = @releaseEnvironmentId";
                        var environment = await databaseConnection.DbConnection
                            .QueryAsync<ReleaseEnvironment>(sql, new {releaseEnvironmentId});
                        release.ReleaseEnvironment = environment.First();
                    }
                    catch (Exception ex)
                    {
                        release.ReleaseEnvironment = new ReleaseEnvironment();
                    }
                }

                return releaseList;
            }
        }

        public async Task InsertReleaseAsync(Release release)
        {
            var releaseEnvironmentName = release.ReleaseEnvironment.Name;
            if (!releaseEnvironmentName.Contains("Production") || release.Name.Contains("MeritBucks") ||
                release.Name.Contains("Insights")) return;

            var releaseEnvironmentId = release.ReleaseEnvironment.Id;
            var sql = $"IF NOT EXISTS (SELECT * FROM ReleaseEnvironment WHERE Id = @releaseEnvironmentId) " +
                      $"INSERT INTO ReleaseEnvironment VALUES (@releaseEnvironmentId, @releaseEnvironmentName)";
            await databaseConnection.DbConnection.ExecuteAsync(sql, new {releaseEnvironmentId, releaseEnvironmentName});

            var finishTime = release.FinishTime != DateTime.MaxValue
                ? release.FinishTime
                : (DateTime?) null;
            sql = $"IF EXISTS(SELECT * FROM Release WHERE ID = @id) " +
                  $"UPDATE Release SET Status = @status, " +
                  $"ReleaseEnvironmentId = @releaseEnvironmentId, " +
                  $"StartTime = @startTime, " +
                  $"FinishTime = @finishTime, " +
                  $"Name = @name, " +
                  $"Attempts = @attempts " +
                  $"WHERE Id = @id " +
                  "ELSE " +
                  $"INSERT INTO Release VALUES (@id, @status, @releaseEnvironmentId, @startTime, @finishTime, @name, @attempts);";
            await databaseConnection.DbConnection.ExecuteAsync(sql, new
            {
                id = release.Id, status = release.Status, releaseEnvironmentId, startTime = release.StartTime,
                finishTime, name = release.Name, attempts = release.Attempts
            });
            Console.WriteLine($"Inserted or Updated Release: {release.Id}");
        }

        public async Task<List<Release>> GetReleasesBeforeDateAsync(DateTime finishTime)
        {
            if (finishTime == DateTime.MaxValue)
                return new List<Release>
                {
                    new Release()
                };
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var startTimeString = finishTime.AddDays(-7).ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'fff");
                var finishTimeString = finishTime.ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'fff");
                var sql = $"SELECT * FROM Release WHERE FinishTime " +
                          $"BETWEEN @startTimeString " +
                          $"AND @finishTimeString " +
                          "ORDER BY FinishTime DESC";
                var releasesObjects =
                    await databaseConnection.DbConnection.QueryAsync(sql, new {startTimeString, finishTimeString});
                var releasesObject = releasesObjects.ToList();

                var releases = new List<Release>();
                foreach (var item in releasesObject)
                {
                    var release = new Release
                    {
                        Attempts = item.Attempts,
                        Id = item.Id,
                        Name = item.Name,
                        Status = item.Status,
                        FinishTime = item.FinishTime,
                        ReleaseEnvironment = await GetReleaseEnvironmentByIdAsync(item.ReleaseEnvironmentId),
                        StartTime = item.StartTime
                    };

                    releases.Add(release);
                }

                return releases;
            }
        }

        public async Task<ReleaseEnvironment> GetReleaseEnvironmentByIdAsync(int releaseEnvironmentId)
        {
            var sql = $"SELECT * FROM ReleaseEnvironment WHERE Id = @releaseEnvironmentId;";
            var releaseEnvironment = await databaseConnection.DbConnection
                .QueryAsync<ReleaseEnvironment>(sql, new {releaseEnvironmentId});
            return releaseEnvironment.First();
        }

        public async Task<Release> GetReleaseByIdAsync(int releaseId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM Release WHERE Id = @releaseId";
                var results = await databaseConnection.DbConnection
                    .QueryAsync<Release>(sql, new {releaseId});
                var result = results.First();

                sql = $"SELECT ReleaseEnvironmentId FROM Release WHERE Id = @releaseId";
                var releaseEnvironmentIds = await databaseConnection.DbConnection
                    .QueryAsync<int>(sql, new {releaseId});
                var releaseEnvironmentId = releaseEnvironmentIds.First();

                sql = $"SELECT * FROM ReleaseEnvironment WHERE Id = @releaseEnvironmentId";
                var releaseEnvironment = await databaseConnection.DbConnection
                    .QueryAsync<ReleaseEnvironment>(sql, new {releaseEnvironmentId});
                result.ReleaseEnvironment = releaseEnvironment.First();

                return result;
            }
        }

        public async Task RemoveReleaseByIdAsync(int releaseId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"DELETE FROM Release WHERE Id = @releaseId";
                await databaseConnection.DbConnection.ExecuteAsync(sql, new {releaseId});
            }
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

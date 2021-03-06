﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccess.Objects;

namespace DataAccess.DataRepositories
{
    public interface IReleaseRepository
    {
        Task InsertReleaseListAsync(IEnumerable<Release> releases);
        Task<IEnumerable<Release>> GetReleaseListAsync(DateTimeOffset startDate, DateTimeOffset endDate);
        Task InsertReleaseAsync(Release release);
        Task<Release> GetFirstReleaseBeforeDateAsync(DateTimeOffset? finishTime);
        Task<Release> GetReleaseByIdAsync(int releaseId);
        Task RemoveReleaseByIdAsync(int releaseId);
        bool ReleaseIsFinishedInDatabase(int id);
    }

    public class ReleaseRepository : IReleaseRepository
    {
        private readonly DatabaseConnection databaseConnection;

        public ReleaseRepository()
        {
            databaseConnection = new DatabaseConnection();
        }

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

        public virtual async Task<IEnumerable<Release>> GetReleaseListAsync(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var startDateString = ($"{startDate:s}").Replace("T", " ");
                var endDateString = ($"{endDate:s}").Replace("T", " ");
                var sql = $"SELECT r.Id, " +
                          "r.state, " +
                          "r.ReleaseEnvironmentId, " +
                          "re.Name as ReleaseEnvironmentName, " +
                          "r.StartTime, " +
                          "r.FinishTime, " +
                          "r.Name, " +
                          "r.Attempts " +
                          "FROM Releases r JOIN ReleaseEnvironments re ON r.ReleaseEnvironmentId = re.Id " +
                          "WHERE FinishTime > @startDateString AND FinishTime < @endDateString " +
                          "ORDER BY r.StartTime";
                var releases = (await databaseConnection.DbConnection
                    .QueryAsync<ReleaseInfo>(sql, new {startDateString, endDateString})).ToList();

                return GetListOfReleasesFromReleaseInfo(releases);
            }
        }

        public async Task InsertReleaseAsync(Release release)
        {
            var releaseEnvironmentName = release.ReleaseEnvironment.Name;
            if (!releaseEnvironmentName.Contains("Production") || release.Name.Contains("MeritBucks") ||
                release.Name.Contains("Insights")) return;

            var releaseEnvironmentId = release.ReleaseEnvironment.Id;
            var sql = "IF NOT EXISTS (SELECT * FROM ReleaseEnvironments WHERE Id = @releaseEnvironmentId) " +
                      $"INSERT INTO ReleaseEnvironments VALUES (@releaseEnvironmentId, @releaseEnvironmentName)";
            await databaseConnection.DbConnection.ExecuteAsync(sql, new {releaseEnvironmentId, releaseEnvironmentName});

            var finishTime = release.FinishTime;
            sql = "IF EXISTS(SELECT * FROM Releases WHERE ID = @id) " +
                  $"UPDATE Releases SET state = @state, " +
                  "ReleaseEnvironmentId = @releaseEnvironmentId, " +
                  "StartTime = @startTime, " +
                  "FinishTime = @finishTime, " +
                  "Name = @name, " +
                  "Attempts = @attempts " +
                  "WHERE Id = @id " +
                  "ELSE " +
                  $"INSERT INTO Releases VALUES (@id, @state, @releaseEnvironmentId, @startTime, @finishTime, @name, @attempts);";
            await databaseConnection.DbConnection.ExecuteAsync(sql, new
            {
                id = release.Id, state = release.State, releaseEnvironmentId, startTime = release.StartTime,
                finishTime, name = release.Name, attempts = release.Attempts
            });
            Console.WriteLine($"Updated Release: {release.Id}");
        }

        public virtual async Task<Release> GetFirstReleaseBeforeDateAsync(DateTimeOffset? finishTime)
        {
            if (finishTime == null)
                return new Release();
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var startTimeString = finishTime.Value.AddDays(-30).ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'fff");
                var finishTimeString = finishTime.Value.ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'fff");
                var sql = $"SELECT r.Id, " +
                          "r.state, " +
                          "r.ReleaseEnvironmentId, " +
                          "re.Name as ReleaseEnvironmentName, " +
                          "r.StartTime, " +
                          "r.FinishTime, " +
                          "r.Name, " +
                          "r.Attempts " +
                          "FROM Releases r JOIN ReleaseEnvironments re ON r.ReleaseEnvironmentId = re.Id " +
                          "WHERE FinishTime " +
                          "BETWEEN @startTimeString " +
                          "AND @finishTimeString " +
                          "ORDER BY FinishTime DESC";
                var releasesObjects =
                    await databaseConnection.DbConnection.QueryAsync<ReleaseInfo>(sql,
                        new {startTimeString, finishTimeString});
                var releasesObject = releasesObjects.ToList();

                return GetListOfReleasesFromReleaseInfo(releasesObject).FirstOrDefault();
            }
        }

        public async Task<Release> GetReleaseByIdAsync(int releaseId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql =
                    $"SELECT r.Id, " +
                    "r.state, " +
                    "r.ReleaseEnvironmentId, " +
                    "re.Name as ReleaseEnvironmentName, " +
                    "r.StartTime, " +
                    "r.FinishTime, " +
                    "r.Name, " +
                    "r.Attempts " +
                    "FROM Releases r JOIN ReleaseEnvironments re ON r.ReleaseEnvironmentId = re.Id " +
                    "WHERE r.Id = @releaseId;";
                var info = (await databaseConnection.DbConnection
                    .QueryAsync<ReleaseInfo>(sql, new {releaseId})).First();
                return new Release
                {
                    Id = info.Id,
                    State = info.state,
                    ReleaseEnvironment = new ReleaseEnvironment
                    {
                        Id = info.ReleaseEnvironmentId,
                        Name = info.ReleaseEnvironmentName
                    },
                    StartTime = info.StartTime,
                    FinishTime = info.FinishTime,
                    Name = info.Name,
                    Attempts = info.Attempts
                };
            }
        }

        public async Task RemoveReleaseByIdAsync(int releaseId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"DELETE FROM Releases WHERE Id = @releaseId";
                await databaseConnection.DbConnection.ExecuteAsync(sql, new {releaseId});
            }
        }

        public virtual bool ReleaseIsFinishedInDatabase(int id)
        {
            databaseConnection.GetNewConnection();
            using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM Releases WHERE Id = @id";
                var result = databaseConnection.DbConnection.Query(sql, new {id});
                return result.Any();
            }
        }

        private static List<Release> GetListOfReleasesFromReleaseInfo(List<ReleaseInfo> releasesObject)
        {
            return releasesObject.Select(item => new Release
                {
                    Attempts = item.Attempts,
                    Id = item.Id,
                    Name = item.Name,
                    State = item.state,
                    FinishTime = item.FinishTime,
                    ReleaseEnvironment = new ReleaseEnvironment {Id = item.ReleaseEnvironmentId, Name = item.ReleaseEnvironmentName},
                    StartTime = item.StartTime
                })
                .ToList();
        }
    }

    public class ReleaseInfo
    {
        public int Id { get; set; }
        public string state { get; set; }
        public int ReleaseEnvironmentId { get; set; }
        public string ReleaseEnvironmentName { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset FinishTime { get; set; }
        public string Name { get; set; }
        public int Attempts { get; set; }
    }
}

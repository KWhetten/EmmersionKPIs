﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;

namespace DataAccess.DatabaseAccess
{
    public interface IReleaseRepository
    {
        void InsertReleaseList(IEnumerable<Release> releases);
        IEnumerable<Release> GetReleaseList(DateTime startDate, DateTime endDate);
        void InsertRelease(Release release);
        List<Release> GetReleasesBeforeDate(DateTime finishTime);
        ReleaseEnvironment GetReleaseEnvironmentById(int releaseEnvironmentId);
        Release GetReleaseById(int releaseId);
        void RemoveReleaseById(int releaseId);
        void RemoveReleaseEnvironmentById(int releaseEnvironmentId);
    }

    public class ReleaseRepository : IReleaseRepository
    {
        public void InsertReleaseList(IEnumerable<Release> releases)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                if (releases == null) return;
                foreach (var release in releases) InsertRelease(release);
            }
        }

        public IEnumerable<Release> GetReleaseList(DateTime startDate, DateTime endDate)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                var startDateString = ($"{startDate:s}").Replace("T", " ");
                var endDateString = ($"{endDate:s}").Replace("T", " ");
                var releaseList = DatabaseConnection.DbConnection
                    .Query<Release>($"SELECT * FROM Release " +
                                    $"WHERE FinishTime > '{startDateString}' " +
                                    $"AND FinishTime < '{endDateString}'").ToList();

                foreach (var release in releaseList)
                {
                    try
                    {
                        var releaseEnvironmentId = DatabaseConnection.DbConnection
                            .Query<int>($"SELECT ReleaseEnvironmentId FROM Release WHERE Id = {release.Id}").ToList();
                        release.ReleaseEnvironment = DatabaseConnection.DbConnection
                            .Query<ReleaseEnvironment>(
                                $"SELECT * FROM ReleaseEnvironment WHERE Id = {releaseEnvironmentId}").First();
                    }
                    catch (Exception ex)
                    {
                        release.ReleaseEnvironment = new ReleaseEnvironment();
                    }
                }

                return releaseList;
            }
        }

        public void InsertRelease(Release release)
        {
            if (!release.ReleaseEnvironment.Name.Contains("Production") || release.Name.Contains("MeritBucks") ||
                release.Name.Contains("Insights")) return;

            DatabaseConnection.DbConnection.Execute(
                $"IF NOT EXISTS (SELECT * FROM ReleaseEnvironment WHERE Id = {release.ReleaseEnvironment.Id}) " +
                $"INSERT INTO ReleaseEnvironment VALUES ({release.ReleaseEnvironment.Id}, '{release.ReleaseEnvironment.Name}')");

            var finishTime = release.FinishTime != DateTime.MaxValue
                ? $"'{release.FinishTime}'"
                : "null";
            var sql = $"IF EXISTS(SELECT * FROM Release WHERE ID = {release.Id}) " +
                      $"UPDATE Release SET Status = '{release.Status}', " +
                      $"ReleaseEnvironmentId = {release.ReleaseEnvironment.Id}, " +
                      $"StartTime = '{release.StartTime}', " +
                      $"FinishTime = '{release.FinishTime}', " +
                      $"Name = '{release.Name}', " +
                      $"Attempts = {release.Attempts} " +
                      $"WHERE Id = {release.Id} " +
                      "ELSE " +
                      $"INSERT INTO Release VALUES ({release.Id}, '{release.Status}', {release.ReleaseEnvironment.Id}, '{release.StartTime}', {finishTime}, '{release.Name}', {release.Attempts});";
            DatabaseConnection.DbConnection.Execute(sql);
            Console.WriteLine($"Inserted or Updated Release: {release.Id}");
        }

        public List<Release> GetReleasesBeforeDate(DateTime finishTime)
        {
            if (finishTime == DateTime.MaxValue)
                return new List<Release>
                {
                    new Release()
                };
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                var query = $"SELECT * FROM Release WHERE FinishTime " +
                            $"BETWEEN '{finishTime.AddDays(-7):yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}' " +
                            $"AND '{finishTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}' " +
                            "ORDER BY FinishTime DESC";
                var releasesObject = DatabaseConnection.DbConnection.Query(query).ToList();


                return releasesObject.Select(item => new Release
                    {
                        Attempts = item.Attempts,
                        Id = item.Id,
                        Name = item.Name,
                        Status = item.Status,
                        FinishTime = item.FinishTime,
                        ReleaseEnvironment = GetReleaseEnvironmentById(item.ReleaseEnvironmentId),
                        StartTime = item.StartTime
                    })
                    .ToList();
            }
        }

        public ReleaseEnvironment GetReleaseEnvironmentById(int releaseEnvironmentId)
        {
            return DatabaseConnection.DbConnection
                .Query<ReleaseEnvironment>($"SELECT * FROM ReleaseEnvironment WHERE Id = {releaseEnvironmentId};")
                .First();
        }

        public Release GetReleaseById(int releaseId)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                var result = DatabaseConnection.DbConnection.Query<Release>($"SELECT * FROM Release WHERE Id = {releaseId}").First();
                var releaseEnvironmentId = DatabaseConnection.DbConnection
                    .Query<int>($"SELECT ReleaseEnvironmentId FROM Release WHERE Id = {releaseId}").First();

                result.ReleaseEnvironment = DatabaseConnection.DbConnection
                    .Query<ReleaseEnvironment>($"SELECT * FROM ReleaseEnvironment WHERE Id = {releaseEnvironmentId}")
                    .First();

                return result;
            }
        }

        public void RemoveReleaseById(int releaseId)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                DatabaseConnection.DbConnection.Execute($"DELETE FROM Release WHERE Id = {releaseId}");
            }
        }

        public void RemoveReleaseEnvironmentById(int releaseEnvironmentId)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                DatabaseConnection.DbConnection.Execute($"DELETE FROM Release WHERE Id = {releaseEnvironmentId}");
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DataObjects.Objects;
using Microsoft.TeamFoundation.Common;

namespace DataManipulation.DatabaseAccess
{
    public interface IDatabaseWrapper
    {
        void InsertReleaseList(IEnumerable<Release> releases);
        void InsertWorkItemCardList(IEnumerable<WorkItemCard> getWorkItemCardList);
        List<Release> GetReleasesBeforeDate(DateTime finishTime);
    }

    public class DatabaseWrapper : IDatabaseWrapper
    {
        private IDbConnection dbConnection;

        public DatabaseWrapper()
        {
            dbConnection =
                new SqlConnection(
                    "Server=localhost,14330;Database=EmmersionMetrics;User Id=sa;Password=truenorth123!;;");
        }

        public void InsertReleaseList(IEnumerable<Release> releases)
        {
            GetNewConnection();
            using (dbConnection)
            {
                if (releases == null) return;
                foreach (var release in releases) InsertRelease(release);
            }
        }

        public void InsertRelease(Release release)
        {
            if (!release.ReleaseEnvironment.Name.Contains("Production") || release.Name.Contains("MeritBucks") ||
                release.Name.Contains("Insights")) return;

            dbConnection.Execute(
                $"IF NOT EXISTS (SELECT * FROM ReleaseEnvironment WHERE Id = {release.ReleaseEnvironment.Id}) " +
                $"INSERT INTO ReleaseEnvironment VALUES ({release.ReleaseEnvironment.Id}, '{release.ReleaseEnvironment.Name}')");

            var finishTime = release.FinishTime != DateTime.MaxValue
                ? $"'{release.FinishTime}'"
                : null;
            dbConnection.Execute($"IF EXISTS(SELECT * FROM Release WHERE ID = {release.Id}) " +
                                 $"UPDATE Release SET Status = '{release.Status}', " +
                                 $"ReleaseEnvironmentId = {release.ReleaseEnvironment.Id}, " +
                                 $"StartTime = '{release.StartTime}', " +
                                 $"FinishTime = '{release.FinishTime}', " +
                                 $"Name = '{release.Name}', " +
                                 $"Attempts = {release.Attempts} " +
                                 $"WHERE Id = {release.Id} " +
                                 "ELSE " +
                                 $"INSERT INTO Release VALUES ({release.Id}, '{release.Status}', {release.ReleaseEnvironment.Id}, '{release.StartTime}', {finishTime}, '{release.Name}', {release.Attempts});");
            Console.WriteLine($"Inserted or Updated Release: {release.Id}");
        }

        public void InsertWorkItemCardList(IEnumerable<WorkItemCard> getWorkItemCardList)
        {
            GetNewConnection();
            using (dbConnection)
            {
                foreach (var item in getWorkItemCardList)
                {
                    InsertWorkItemCard(item);
                }
            }
        }

        public void InsertWorkItemCard(WorkItemCard workItemCard)
        {
            try
            {
                if (workItemCard.Release != null && workItemCard.Release.Id != 0)
                {
                    InsertRelease(workItemCard.Release);
                }
                var sql = $"IF EXISTS(SELECT * FROM WorkItemCard WHERE Id = {workItemCard.Id}) " +
                          $"UPDATE WorkItemCard SET Title = '{workItemCard.Title.Replace("\'", "\'\'")}', " +
                          $"StartTime = {(workItemCard.StartTime != DateTime.MinValue ? $"'{workItemCard.StartTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"FinishTime = {(workItemCard.FinishTime != DateTime.MaxValue ? $"'{workItemCard.FinishTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"WorkItemCardTypeId = {(int) workItemCard.Type}, " +
                          $"DevelopmentTeamName = {(!workItemCard.DevelopmentTeamName.IsNullOrEmpty() ? $"'{workItemCard.DevelopmentTeamName}'" : "null")}, " +
                          $"CreatedOn = '{workItemCard.CreatedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"CreatedBy = {(!workItemCard.CreatedBy.IsNullOrEmpty() ? $"'{workItemCard.CreatedBy}'" : "null")}, " +
                          $"LastChangedOn = '{workItemCard.LastChangedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"LastChangedBy = {(!workItemCard.LastChangedBy.IsNullOrEmpty() ? $"'{workItemCard.LastChangedBy}'" : "null")}, " +
                          $"CurrentBoardColumn = {(!workItemCard.CurrentBoardColumn.IsNullOrEmpty() ? $"'{workItemCard.CurrentBoardColumn}'" : "null")}, " +
                          $"CardState = {(!workItemCard.CardState.IsNullOrEmpty() ? $"'{workItemCard.CardState}'" : "null")}, " +
                          $"Impact = {(!workItemCard.Impact.IsNullOrEmpty() ? $"'{workItemCard.Impact}'" : "null")}, " +
                          $"CommentCount = {workItemCard.CommentCount}, " +
                          $"NumRevisions = {workItemCard.NumRevisions}, " +
                          $"ReleaseId = {(workItemCard.Release != null && workItemCard.Release.Id != 0 ? workItemCard.Release.Id.ToString() : "null")} " +
                          $"WHERE Id = {workItemCard.Id} " +
                          "ELSE " +
                          $"INSERT INTO WorkItemCard (Id, Title, StartTime, FinishTime, WorkItemCardTypeId, DevelopmentTeamName, CreatedOn, CreatedBy, " +
                          $"LastChangedOn, LastChangedBy, CurrentBoardColumn, CardState, Impact, CommentCount, NumRevisions, ReleaseId) " +
                          $"VALUES (" +
                          $"{workItemCard.Id}, " +
                          $"'{workItemCard.Title.Replace("\'", "\'\'")}', " +
                          $"{(workItemCard.StartTime != DateTime.MinValue ? $"'{workItemCard.StartTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"{(workItemCard.FinishTime != DateTime.MaxValue ? $"'{workItemCard.FinishTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"{(int) workItemCard.Type}, " +
                          $"{(!workItemCard.DevelopmentTeamName.IsNullOrEmpty() ? $"'{workItemCard.DevelopmentTeamName}'" : "null")}, " +
                          $"'{workItemCard.CreatedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"{(!workItemCard.CreatedBy.IsNullOrEmpty() ? $"'{workItemCard.CreatedBy}'" : "null")}, " +
                          $"'{workItemCard.LastChangedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"{(!workItemCard.LastChangedBy.IsNullOrEmpty() ? $"'{workItemCard.LastChangedBy}'" : "null")}, " +
                          $"{(!workItemCard.CurrentBoardColumn.IsNullOrEmpty() ? $"'{workItemCard.CurrentBoardColumn}'" : "null")}, " +
                          $"{(!workItemCard.CardState.IsNullOrEmpty() ? $"'{workItemCard.CardState}'" : "null")}, " +
                          $"{(!workItemCard.Impact.IsNullOrEmpty() ? $"'{workItemCard.Impact}'" : "null")}, " +
                          $"{workItemCard.CommentCount}, " +
                          $"{workItemCard.NumRevisions}, " +
                          $"{(workItemCard.Release != null && workItemCard.Release.Id != 0 ? workItemCard.Release.Id.ToString() : "null")});";
                dbConnection.Execute(sql);

                Console.WriteLine($"Inserted or Updated Card: {workItemCard.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to insert card: {workItemCard.Id} - " + ex.Message);
            }
        }

        public List<Release> GetReleasesBeforeDate(DateTime finishTime)
        {
            if (finishTime != DateTime.MaxValue)
            {
                GetNewConnection();
                using (dbConnection)
                {
                    var query = $"SELECT * FROM Release WHERE FinishTime " +
                                                $"BETWEEN '{finishTime.AddDays(-7):yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}' " +
                                                $"AND '{finishTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}' " +
                                                "ORDER BY FinishTime DESC";
                    var releasesObject = dbConnection.Query(query).ToList();


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
            return new List<Release>
            {
                new Release()
            };
        }

        public ReleaseEnvironment GetReleaseEnvironmentById(int releaseEnvironmentId)
        {
            return dbConnection
                .Query<ReleaseEnvironment>($"SELECT * FROM ReleaseEnvironment WHERE Id = {releaseEnvironmentId};")
                .First();
        }

        public Release GetReleaseById(int releaseId)
        {
            GetNewConnection();
            using (dbConnection)
            {
                var result = dbConnection.Query<Release>($"SELECT * FROM Release WHERE Id = {releaseId}").First();
                var releaseEnvironmentId = dbConnection
                    .Query<int>($"SELECT ReleaseEnvironmentId FROM Release WHERE Id = {releaseId}").First();

                result.ReleaseEnvironment = dbConnection
                    .Query<ReleaseEnvironment>($"SELECT * FROM ReleaseEnvironment WHERE Id = {releaseEnvironmentId}")
                    .First();

                return result;
            }
        }

        public void RemoveReleaseById(int releaseId)
        {
            GetNewConnection();
            using (dbConnection)
            {
                dbConnection.Execute($"DELETE FROM Release WHERE Id = {releaseId}");
            }
        }

        public void RemoveReleaseEnvironmentById(int releaseEnvironmentId)
        {
            GetNewConnection();
            using (dbConnection)
            {
                dbConnection.Execute($"DELETE FROM Release WHERE Id = {releaseEnvironmentId}");
            }
        }

        public WorkItemCard GetCardById(in int cardId)
        {
            GetNewConnection();
            using (dbConnection)
            {
                var result = dbConnection.Query<WorkItemCard>($"SELECT * FROM WorkItemCard WHERE Id = {cardId}")
                    .First();

                var typeId = dbConnection.Query<int>($"SELECT WorkItemCardTypeId FROM WorkItemCard WHERE Id = {cardId}")
                    .First();
                result.Type = dbConnection
                    .Query<WorkItemCardType>($"SELECT * FROM WorkItemCardType WHERE Id = {typeId}").First();

                var releaseId = dbConnection.Query<int>($"SELECT ReleaseId FROM WorkItemCard WHERE Id = {cardId}")
                    .First();
                result.Release = dbConnection
                    .Query<Release>($"SELECT * FROM Release WHERE Id = {releaseId}").First();
                var releaseEnvironmentId = dbConnection
                    .Query<int>($"SELECT ReleaseEnvironmentId FROM Release WHERE Id = {releaseId}").First();

                result.Release.ReleaseEnvironment = dbConnection
                    .Query<ReleaseEnvironment>($"SELECT * FROM ReleaseEnvironment WHERE Id = {releaseEnvironmentId}")
                    .First();

                return result;
            }
        }

        public void RemoveWorkItemCardById(int cardId)
        {
            GetNewConnection();
            using (dbConnection)
            {
                dbConnection.Execute($"DELETE FROM WorkItemCard WHERE Id = {cardId}");
            }
        }

        public void GetNewConnection()
        {
            dbConnection =
                new SqlConnection(
                    "Server=localhost,14330;Database=EmmersionMetrics;User Id=sa;Password=truenorth123!;;");
        }

        public List<WorkItemCard> GetWorkItemCardList(DateTime startDate, DateTime endDate)
        {
            GetNewConnection();
            using (dbConnection)
            {
                var startDateString = ($"{startDate:s}").Replace("T", " ");
                var endDateString = ($"{endDate:s}").Replace("T", " ");
                var workItemCardList = dbConnection
                    .Query<WorkItemCard>($"SELECT * FROM WorkItemCard " +
                                         $"WHERE FinishTime > '{startDateString}' " +
                                         $"AND FinishTime < '{endDateString}'").ToList();

                foreach (var workItemCard in workItemCardList)
                {
                    try
                    {
                        var releaseId = dbConnection
                            .Query<int>(
                                $"SELECT ReleaseId " +
                                $"FROM WorkItemCard " +
                                $"WHERE Id = {workItemCard.Id}")
                            .ToList();
                        workItemCard.Release = dbConnection
                            .Query<Release>($"SELECT * " +
                                            $"FROM Release " +
                                            $"WHERE Id = {releaseId.First()}")
                            .First();
                    }
                    catch (Exception ex)
                    {
                        workItemCard.Release = new Release();
                    }
                }

                return workItemCardList;
            }
        }

        public IEnumerable<Release> GetReleaseList(DateTime startDate, DateTime endDate)
        {
            GetNewConnection();
            using (dbConnection)
            {
                var startDateString = ($"{startDate:s}").Replace("T", " ");
                var endDateString = ($"{endDate:s}").Replace("T", " ");
                var releaseList = dbConnection
                    .Query<Release>($"SELECT * FROM Release " +
                                    $"WHERE FinishTime > '{startDateString}' " +
                                    $"AND FinishTime < '{endDateString}'").ToList();

                foreach (var release in releaseList)
                {
                    try
                    {
                        var releaseEnvironmentId = dbConnection
                            .Query<int>($"SELECT ReleaseEnvironmentId FROM Release WHERE Id = {release.Id}").ToList();
                        release.ReleaseEnvironment = dbConnection
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

        public bool InsertUserInfo(string firstName, string lastName, string email, string password)
        {
            return true;
        }
    }
}

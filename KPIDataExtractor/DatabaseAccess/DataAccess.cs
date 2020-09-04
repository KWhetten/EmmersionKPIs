using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using KPIDataExtractor.Objects;
using Microsoft.TeamFoundation.Common;

namespace KPIDataExtractor.DatabaseAccess
{
    public interface IDataAccess
    {
        void InsertReleaseList(IEnumerable<Release> releases);
        void InsertRelease(Release release);
        void InsertWorkItemCardList(IEnumerable<WorkItemCard> getWorkItemCardList);
        void InsertWorkItemCard(WorkItemCard workItemCard);
        List<Release> GetReleasesBeforeDate(DateTime finishTime);
        ReleaseEnvironment GetReleaseEnvironmentById(int releaseEnvironmentId);
    }

    public class DataAccess : IDataAccess
    {
        private IDbConnection dbConnection;

        public DataAccess()
        {
            dbConnection = new SqlConnection("Server=localhost,14330;Database=EmmersionMetrics;User Id=sa;Password=truenorth123!;;");
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
                ? "\'" + release.FinishTime + "\'"
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
                          $"StartTime = {(workItemCard.StartTime != DateTime.MinValue ? "\'" + workItemCard.StartTime + "\'" : "null")}, " +
                          $"FinishTime = {(workItemCard.FinishTime != DateTime.MaxValue ? "\'" + workItemCard.FinishTime + "\'" : "null")}, " +
                          $"WorkItemCardTypeId = {(int) workItemCard.Type}, " +
                          $"DevelopmentTeamName = {(!workItemCard.DevelopmentTeamName.IsNullOrEmpty() ? "\'" + workItemCard.DevelopmentTeamName + "\'" : "null")}, " +
                          $"CreatedOn = '{workItemCard.CreatedOn}', " +
                          $"CreatedBy = {(!workItemCard.CreatedBy.IsNullOrEmpty() ? "\'" + workItemCard.CreatedBy + "\'" : "null")}, " +
                          $"LastChangedOn = '{workItemCard.LastChangedOn}', " +
                          $"LastChangedBy = {(!workItemCard.LastChangedBy.IsNullOrEmpty() ? "\'" + workItemCard.LastChangedBy + "\'" : "null")}, " +
                          $"CurrentBoardColumn = {(!workItemCard.CurrentBoardColumn.IsNullOrEmpty() ? "\'" + workItemCard.CurrentBoardColumn + "\'" : "null")}, " +
                          $"CardState = {(!workItemCard.CardState.IsNullOrEmpty() ? "\'" + workItemCard.CardState + "\'" : "null")}, " +
                          $"Impact = {(!workItemCard.Impact.IsNullOrEmpty() ? "\'" + workItemCard.Impact + "\'" : "null")}, " +
                          $"CommentCount = {workItemCard.CommentCount}, " +
                          $"NumRevisions = {workItemCard.NumRevisions}, " +
                          $"ReleaseId = {(workItemCard.Release.Id != 0 ? workItemCard.Release.Id.ToString() : "null")} " +
                          $"WHERE Id = {workItemCard.Id} " +
                          "ELSE " +
                          $"INSERT INTO WorkItemCard VALUES ({workItemCard.Id}, " +
                          $"'{workItemCard.Title.Replace("\'", "\'\'")}', " +
                          $"{(workItemCard.StartTime != DateTime.MinValue ? "\'" + workItemCard.StartTime + "\'" : "null")}, " +
                          $"{(workItemCard.FinishTime != DateTime.MaxValue ? "\'" + workItemCard.FinishTime + "\'" : "null")}, " +
                          $"{(int) workItemCard.Type}, " +
                          $"{(!workItemCard.DevelopmentTeamName.IsNullOrEmpty() ? "\'" + workItemCard.DevelopmentTeamName + "\'" : "null")}, " +
                          $"'{workItemCard.CreatedOn}', " +
                          $"{(!workItemCard.CreatedBy.IsNullOrEmpty() ? "\'" + workItemCard.CreatedBy + "\'" : "null")}, " +
                          $"{(!workItemCard.CurrentBoardColumn.IsNullOrEmpty() ? "\'" + workItemCard.CurrentBoardColumn + "\'" : "null")}, " +
                          $"{(!workItemCard.CardState.IsNullOrEmpty() ? "\'" + workItemCard.CardState + "\'" : "null")}, " +
                          $"{(!workItemCard.Impact.IsNullOrEmpty() ? "\'" + workItemCard.Impact + "\'" : "null")}, " +
                          $"{workItemCard.CommentCount}, " +
                          $"{workItemCard.NumRevisions}, " +
                          $"{(workItemCard.Release.Id != 0 ? workItemCard.Release.Id.ToString() : "null")}, " +
                          $"'{workItemCard.LastChangedOn}', " +
                          $"{(!workItemCard.LastChangedBy.IsNullOrEmpty() ? "\'" + workItemCard.LastChangedBy + "\'" : "null")});";
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
            GetNewConnection();
            using (dbConnection)
            {
                var releasesObject = dbConnection.Query($"SELECT * FROM Release WHERE FinishTime " +
                                                        $"BETWEEN '{finishTime.AddDays(-7):yyyy'-'MM'-'dd HH':'mm'.'ss}' " +
                                                        $"AND '{finishTime:yyyy'-'MM'-'dd HH':'mm'.'ss}' " +
                                                        "ORDER BY FinishTime DESC").ToList();


                var newReleases = new List<Release>();
                foreach (var item in releasesObject)
                {
                    var release = new Release();
                    release.Attempts = item.Attempts;
                    release.Id = item.Id;
                    release.Name = item.Name;
                    release.Status = item.Status;
                    release.FinishTime = item.FinishTime;
                    release.ReleaseEnvironment = GetReleaseEnvironmentById(item.ReleaseEnvironmentId);
                    release.StartTime = item.StartTime;

                    newReleases.Add(release);
                }

                return newReleases;
            }
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
            dbConnection = new SqlConnection("Server=localhost,14330;Database=EmmersionMetrics;User Id=sa;Password=truenorth123!;;");
        }
    }
}

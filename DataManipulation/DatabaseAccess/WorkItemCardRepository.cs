using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;
using Microsoft.TeamFoundation.Common;

namespace DataAccess.DatabaseAccess
{
    public interface ITaskItemRepository
    {
        List<TaskItem> GetTaskItemList(DateTime startDate, DateTime endDate);
        void InsertTaskItemList(IEnumerable<TaskItem> getTaskItemList);
        void InsertTaskItem(TaskItem TaskItem);
        TaskItem GetCardById(in int cardId);
        void RemoveTaskItemById(int cardId);
    }

    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly ReleaseRepository releaseRepository = new ReleaseRepository();

        public virtual List<TaskItem> GetTaskItemList(DateTime startDate, DateTime endDate)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                var startDateString = ($"{startDate:s}").Replace("T", " ");
                var endDateString = ($"{endDate:s}").Replace("T", " ");
                var TaskItemList = DatabaseConnection.DbConnection
                    .Query<TaskItem>($"SELECT * FROM TaskItem " +
                                         $"WHERE FinishTime > '{startDateString}' " +
                                         $"AND FinishTime < '{endDateString}'").ToList();

                foreach (var TaskItem in TaskItemList)
                {
                    try
                    {
                        var releaseId = DatabaseConnection.DbConnection
                            .Query<int>(
                                "SELECT ReleaseId " +
                                "FROM TaskItem " +
                                $"WHERE Id = {TaskItem.Id}")
                            .ToList();
                        TaskItem.Release = DatabaseConnection.DbConnection
                            .Query<Release>("SELECT * " +
                                            "FROM Release " +
                                            $"WHERE Id = {releaseId.First()}")
                            .First();

                        TaskItem.Type = (TaskItemType) (DatabaseConnection.DbConnection
                            .Query<int>("SELECT TaskItemTypeId " +
                                        "FROM TaskItem " +
                                        $"WHERE Id = {TaskItem.Id}")
                            .ToList().First() - 1);
                    }
                    catch (Exception ex)
                    {
                        TaskItem.Release = null;
                    }
                }

                return TaskItemList;
            }
        }

        public void InsertTaskItemList(IEnumerable<TaskItem> getTaskItemList)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                foreach (var item in getTaskItemList)
                {
                    InsertTaskItem(item);
                }
            }
        }

        public void InsertTaskItem(TaskItem TaskItem)
        {
            try
            {
                if (TaskItem.Release != null && TaskItem.Release.Id != 0)
                {
                    releaseRepository.InsertRelease(TaskItem.Release);
                }

                var sql = $"IF EXISTS(SELECT * FROM TaskItem WHERE Id = {TaskItem.Id}) " +
                          $"UPDATE TaskItem SET Title = '{TaskItem.Title.Replace("\'", "\'\'")}', " +
                          $"StartTime = {(TaskItem.StartTime != DateTime.MinValue ? $"'{TaskItem.StartTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"FinishTime = {(TaskItem.FinishTime != DateTime.MaxValue ? $"'{TaskItem.FinishTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"TaskItemTypeId = {(int) TaskItem.Type}, " +
                          $"DevelopmentTeamName = {(!TaskItem.DevelopmentTeamName.IsNullOrEmpty() ? $"'{TaskItem.DevelopmentTeamName}'" : "null")}, " +
                          $"CreatedOn = '{TaskItem.CreatedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"CreatedBy = {(!TaskItem.CreatedBy.IsNullOrEmpty() ? $"'{TaskItem.CreatedBy}'" : "null")}, " +
                          $"LastChangedOn = '{TaskItem.LastChangedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"LastChangedBy = {(!TaskItem.LastChangedBy.IsNullOrEmpty() ? $"'{TaskItem.LastChangedBy}'" : "null")}, " +
                          $"CurrentBoardColumn = {(!TaskItem.CurrentBoardColumn.IsNullOrEmpty() ? $"'{TaskItem.CurrentBoardColumn}'" : "null")}, " +
                          $"CardState = {(!TaskItem.CardState.IsNullOrEmpty() ? $"'{TaskItem.CardState}'" : "null")}, " +
                          $"Impact = {(!TaskItem.Impact.IsNullOrEmpty() ? $"'{TaskItem.Impact}'" : "null")}, " +
                          $"CommentCount = {TaskItem.CommentCount}, " +
                          $"NumRevisions = {TaskItem.NumRevisions}, " +
                          $"ReleaseId = {(TaskItem.Release != null && TaskItem.Release.Id != 0 ? TaskItem.Release.Id.ToString() : "null")} " +
                          $"WHERE Id = {TaskItem.Id} " +
                          "ELSE " +
                          $"INSERT INTO TaskItem (Id, Title, StartTime, FinishTime, TaskItemTypeId, DevelopmentTeamName, CreatedOn, CreatedBy, " +
                          "LastChangedOn, LastChangedBy, CurrentBoardColumn, CardState, Impact, CommentCount, NumRevisions, ReleaseId) " +
                          "VALUES (" +
                          $"{TaskItem.Id}, " +
                          $"'{TaskItem.Title.Replace("\'", "\'\'")}', " +
                          $"{(TaskItem.StartTime != DateTime.MinValue ? $"'{TaskItem.StartTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"{(TaskItem.FinishTime != DateTime.MaxValue ? $"'{TaskItem.FinishTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}'" : "null")}, " +
                          $"{(int) TaskItem.Type}, " +
                          $"{(!TaskItem.DevelopmentTeamName.IsNullOrEmpty() ? $"'{TaskItem.DevelopmentTeamName}'" : "null")}, " +
                          $"'{TaskItem.CreatedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"{(!TaskItem.CreatedBy.IsNullOrEmpty() ? $"'{TaskItem.CreatedBy}'" : "null")}, " +
                          $"'{TaskItem.LastChangedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}', " +
                          $"{(!TaskItem.LastChangedBy.IsNullOrEmpty() ? $"'{TaskItem.LastChangedBy}'" : "null")}, " +
                          $"{(!TaskItem.CurrentBoardColumn.IsNullOrEmpty() ? $"'{TaskItem.CurrentBoardColumn}'" : "null")}, " +
                          $"{(!TaskItem.CardState.IsNullOrEmpty() ? $"'{TaskItem.CardState}'" : "null")}, " +
                          $"{(!TaskItem.Impact.IsNullOrEmpty() ? $"'{TaskItem.Impact}'" : "null")}, " +
                          $"{TaskItem.CommentCount}, " +
                          $"{TaskItem.NumRevisions}, " +
                          $"{(TaskItem.Release != null && TaskItem.Release.Id != 0 ? TaskItem.Release.Id.ToString() : "null")});";
                DatabaseConnection.DbConnection.Execute(sql);

                Console.WriteLine($"Inserted or Updated Card: {TaskItem.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to insert card: {TaskItem.Id} - " + ex.Message);
            }
        }

        public TaskItem GetCardById(in int cardId)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                var result = DatabaseConnection.DbConnection
                    .Query<TaskItem>($"SELECT * FROM TaskItem WHERE Id = {cardId}")
                    .First();

                var typeId = DatabaseConnection.DbConnection
                    .Query<int>($"SELECT TaskItemTypeId FROM TaskItem WHERE Id = {cardId}")
                    .First();
                result.Type = DatabaseConnection.DbConnection
                    .Query<TaskItemType>($"SELECT * FROM TaskItemType WHERE Id = {typeId}").First();

                var releaseId = DatabaseConnection.DbConnection
                    .Query<int>($"SELECT ReleaseId FROM TaskItem WHERE Id = {cardId}")
                    .First();
                result.Release = DatabaseConnection.DbConnection
                    .Query<Release>($"SELECT * FROM Release WHERE Id = {releaseId}").First();
                var releaseEnvironmentId = DatabaseConnection.DbConnection
                    .Query<int>($"SELECT ReleaseEnvironmentId FROM Release WHERE Id = {releaseId}").First();

                result.Release.ReleaseEnvironment = DatabaseConnection.DbConnection
                    .Query<ReleaseEnvironment>($"SELECT * FROM ReleaseEnvironment WHERE Id = {releaseEnvironmentId}")
                    .First();

                return result;
            }
        }

        public void RemoveTaskItemById(int cardId)
        {
            DatabaseConnection.GetNewConnection();
            using (DatabaseConnection.DbConnection)
            {
                DatabaseConnection.DbConnection.Execute($"DELETE FROM TaskItem WHERE Id = {cardId}");
            }
        }
    }
}

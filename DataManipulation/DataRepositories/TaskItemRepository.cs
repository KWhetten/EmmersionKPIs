using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataManipulation.DatabaseAccess;
using DataObjects.Objects;
using Microsoft.TeamFoundation.Common;

namespace DataAccess.DataRepositories
{
    public interface ITaskItemRepository
    {
        Task<List<TaskItem>> GetTaskItemListAsync(DateTime startDate, DateTime endDate);
        Task InsertTaskItemListAsync(IEnumerable<TaskItem> getTaskItemList);
        Task InsertTaskItemAsync(TaskItem taskItem);
        Task<TaskItem> GetCardByIdAsync(int cardId);
        Task RemoveTaskItemByIdAsync(int cardId);
    }

    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly DatabaseConnection databaseConnection;

        public TaskItemRepository(DatabaseConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        private readonly ReleaseRepository releaseRepository = new ReleaseRepository(new DatabaseConnection());

        public virtual async Task<List<TaskItem>> GetTaskItemListAsync(DateTime startDate, DateTime endDate)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var startDateString = $"{startDate:s}".Replace("T", " ");
                var endDateString = $"{endDate:s}".Replace("T", " ");

                var sql = $"SELECT * FROM TaskItem WHERE FinishTime > @startDateString AND FinishTime < @endDateString";

                var taskItems = await databaseConnection.DbConnection
                    .QueryAsync<TaskItem>(sql, new {startDateString, endDateString});
                var taskItemList = taskItems.ToList();

                foreach (var taskItem in taskItemList)
                {
                    try
                    {
                        sql = $"SELECT ReleaseId FROM TaskItem WHERE Id = @taskItem.Id";
                        var releaseId = await databaseConnection.DbConnection
                            .QueryAsync<int>(sql, new {taskItem.Id});
                        var releaseIdList = releaseId.ToList();

                        sql = $"SELECT * FROM Release WHERE Id = @releaseId";
                        var release = await databaseConnection.DbConnection
                            .QueryAsync<Release>(sql, new {releaseId = releaseIdList.First()});
                        taskItem.Release = release.First();

                        sql = $"SELECT TaskItemTypeId FROM TaskItem WHERE Id = @taskItem.Id";
                        var type = await (databaseConnection.DbConnection
                            .QueryAsync<int>(sql, new {taskItem.Id}));
                        taskItem.Type = (TaskItemType) type.ToList().First() - 1;
                    }
                    catch (Exception ex)
                    {
                        taskItem.Release = null;
                    }
                }

                return taskItemList;
            }
        }

        public async Task InsertTaskItemListAsync(IEnumerable<TaskItem> getTaskItemList)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                foreach (var item in getTaskItemList)
                {
                    await InsertTaskItemAsync(item);
                }
            }
        }

        public async Task InsertTaskItemAsync(TaskItem taskItem)
        {
            try
            {
                if (taskItem.Release != null && taskItem.Release.Id != 0)
                {
                    await releaseRepository.InsertReleaseAsync(taskItem.Release);
                }

                var taskItemId = taskItem.Id;
                var title = taskItem.Title.Replace("\'", "\'\'");
                var startTime = taskItem.StartTime != DateTime.MinValue
                    ? $"{taskItem.StartTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}"
                    : null;
                var finishTime = taskItem.FinishTime != DateTime.MaxValue
                    ? $"{taskItem.FinishTime:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}"
                    : null;
                var taskItemTypeId = (int) taskItem.Type;
                var developmentTeamName = !taskItem.DevelopmentTeamName.IsNullOrEmpty()
                    ? taskItem.DevelopmentTeamName
                    : null;
                var createdOn = $"{taskItem.CreatedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}";
                var createdBy = !taskItem.CreatedBy.IsNullOrEmpty()
                    ? taskItem.CreatedBy
                    : null;
                var lastChangedOn = $"{taskItem.LastChangedOn:yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}";
                var lastChangedBy = !taskItem.LastChangedBy.IsNullOrEmpty()
                    ? taskItem.LastChangedBy
                    : null;
                var currentBoardColumn = !taskItem.CurrentBoardColumn.IsNullOrEmpty()
                    ? taskItem.CurrentBoardColumn
                    : null;
                var cardState = !taskItem.CardState.IsNullOrEmpty()
                    ? taskItem.CardState
                    : null;
                var impact = !taskItem.Impact.IsNullOrEmpty()
                    ? taskItem.Impact
                    : null;
                var commentCount = taskItem.CommentCount;
                var numRevisions = taskItem.NumRevisions;
                var releaseId = taskItem.Release != null && taskItem.Release.Id != 0
                    ? taskItem.Release.Id
                    : (int?) null;

                var sql = $"IF EXISTS(SELECT * FROM TaskItem WHERE Id = @taskItemId) " +
                          $"UPDATE TaskItem SET " +
                          "Title = @title, " +
                          "StartTime = @startTime, " +
                          "FinishTime = @finishTime, " +
                          "TaskItemTypeId = @taskItemTypeId, " +
                          "DevelopmentTeamName = @developmentTeamName, " +
                          "CreatedOn = @createdOn, " +
                          "CreatedBy = @createdBy, " +
                          "LastChangedOn = @lastChangedOn, " +
                          "LastChangedBy = @lastChangedBy, " +
                          "CurrentBoardColumn = @currentBoardColumn, " +
                          "CardState = @cardState, " +
                          "Impact = @impact, " +
                          "CommentCount = @commentCount, " +
                          "NumRevisions = @numRevisions, " +
                          "ReleaseId = @releaseId " +
                          "WHERE Id = @taskItemId " +
                          "ELSE " +
                          $"INSERT INTO TaskItem (Id, Title, StartTime, FinishTime, TaskItemTypeId, DevelopmentTeamName, CreatedOn, CreatedBy, " +
                          "LastChangedOn, LastChangedBy, CurrentBoardColumn, CardState, Impact, CommentCount, NumRevisions, ReleaseId) " +
                          "VALUES (" +
                          "@taskItemId, " +
                          "@title, " +
                          "@startTime, " +
                          "@finishTime, " +
                          "@taskItemTypeId, " +
                          "@developmentTeamName, " +
                          "@createdOn, " +
                          "@createdBy, " +
                          "@lastChangedOn, " +
                          "@lastChangedBy, " +
                          "@currentBoardColumn, " +
                          "@cardState, " +
                          "@impact, " +
                          "@commentCount, " +
                          "@numRevisions, " +
                          "@releaseId)";
                await databaseConnection.DbConnection.ExecuteAsync(sql,
                    new
                    {
                        taskItemId, title, startTime, finishTime, taskItemTypeId, developmentTeamName, createdOn,
                        createdBy, lastChangedOn, lastChangedBy, currentBoardColumn, cardState, impact, commentCount,
                        numRevisions, releaseId
                    });

                Console.WriteLine($"Inserted or Updated Card: {taskItem.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to insert card: {taskItem.Id} - " + ex.Message);
            }
        }

        public async Task<TaskItem> GetCardByIdAsync(int cardId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM TaskItem WHERE Id = @cardId";
                var taskItems = await databaseConnection.DbConnection
                    .QueryAsync<TaskItem>(sql, new {cardId});
                var taskItem = taskItems.First();

                sql = $"SELECT TaskItemTypeId FROM TaskItem WHERE Id = @cardId";
                var taskTypeIds = await databaseConnection.DbConnection
                    .QueryAsync<int>(sql, new {cardId});
                var taskTypeId = taskTypeIds.First();

                sql = $"SELECT * FROM TaskItemType WHERE Id = @taskTypeId";
                var taskType = await databaseConnection.DbConnection
                    .QueryAsync<TaskItemType>(sql, new {taskTypeId});
                taskItem.Type = taskType.First();

                sql = $"SELECT ReleaseId FROM TaskItem WHERE Id = @cardId";
                var releaseIds = await databaseConnection.DbConnection
                    .QueryAsync<int>(sql, new {cardId});
                var releaseId = releaseIds.First();

                sql = $"SELECT * FROM Release WHERE Id = @releaseId";
                var release = await databaseConnection.DbConnection
                    .QueryAsync<Release>(sql, new {releaseId});
                taskItem.Release = release.First();

                sql = $"SELECT ReleaseEnvironmentId FROM Release WHERE Id = @releaseId";
                var releaseEnvironmentIds = await databaseConnection.DbConnection
                    .QueryAsync<int>(sql, new {releaseId});
                var releaseEnvironmentId = releaseEnvironmentIds.First();

                sql = $"SELECT * FROM ReleaseEnvironment WHERE Id = @releaseEnvironmentId";
                var releaseEnvironment = await databaseConnection.DbConnection
                    .QueryAsync<ReleaseEnvironment>(sql, new {releaseEnvironmentId});
                taskItem.Release.ReleaseEnvironment = releaseEnvironment.First();

                return taskItem;
            }
        }

        public async Task RemoveTaskItemByIdAsync(int cardId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"DELETE FROM TaskItem WHERE Id = @cardId";
                await databaseConnection.DbConnection.ExecuteAsync(sql,
                    new {cardId});
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccess.Objects;
using Microsoft.TeamFoundation.Common;

namespace DataAccess.DataRepositories
{
    public interface ITaskItemRepository
    {
        Task<List<TaskItem>> GetTaskItemListAsync(DateTime startDate, DateTime endDate);
        Task InsertTaskItemListAsync(IEnumerable<TaskItem> getTaskItemList);
        Task InsertTaskItemAsync(TaskItem taskItem);
        Task<TaskItem> GetTaskItemByIdAsync(int taskItemId);
        Task<TaskItemType[]> GetTaskItemTypesAsync();
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

                var sql = $"SELECT ti.Id, " +
                          "ti.Title, " +
                          "ti.StartTime, " +
                          "ti.FinishTime, " +
                          "ti.TaskItemTypeId, " +
                          "tit.Name as TaskItemTypeName, " +
                          "ti.DevelopmentTeamName, " +
                          "ti.CreatedOn, " +
                          "ti.CreatedBy, " +
                          "ti.LastChangedOn, " +
                          "ti.LastChangedBy, " +
                          "ti.CurrentBoardColumn, " +
                          "ti.CardState, " +
                          "ti.Impact, " +
                          "ti.CommentCount, " +
                          "ti.NumRevisions, " +
                          "ti.ReleaseId, " +
                          "r.Status as ReleaseStatus, " +
                          "r.ReleaseEnvironmentId, " +
                          "re.Name as ReleaseEnvironmentName, " +
                          "r.StartTime as ReleaseStartTime, " +
                          "r.FinishTime as ReleaseFinishTime, " +
                          "r.Name as ReleaseName, " +
                          "r.Attempts as ReleaseAttempts " +
                          "FROM TaskItem ti LEFT JOIN TaskItemType tit ON ti.TaskItemTypeId = tit.Id " +
                          "LEFT JOIN Release r ON ti.ReleaseId = r.Id " +
                          "LEFT JOIN ReleaseEnvironment re ON r.ReleaseEnvironmentId = re.Id " +
                          "WHERE ti.FinishTime > @startDateString AND ti.FinishTime < @endDateString";

                var taskItems = await databaseConnection.DbConnection
                    .QueryAsync<TaskItemInfo>(sql, new {startDateString, endDateString});

                return GetTaskItemListFromTaskItemInfo(taskItems);
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

                var sql = "IF EXISTS(SELECT * FROM TaskItem WHERE Id = @taskItemId) " +
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

        public async Task<TaskItem> GetTaskItemByIdAsync(int taskItemId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT ti.Id," +
                          "ti.Title, " +
                          "ti.StartTime, " +
                          "ti.FinishTime, " +
                          "ti.TaskItemTypeId, " +
                          "tit.Name as TaskItemTypeName, " +
                          "ti.DevelopmentTeamName, " +
                          "ti.CreatedOn, " +
                          "ti.CreatedBy, " +
                          "ti.LastChangedOn, " +
                          "ti.LastChangedBy, " +
                          "ti.CurrentBoardColumn, " +
                          "ti.CardState, " +
                          "ti.Impact, " +
                          "ti.CommentCount, " +
                          "ti.NumRevisions, " +
                          "ti.ReleaseId, " +
                          "r.Status as ReleaseStatus, " +
                          "r.ReleaseEnvironmentId, " +
                          "re.Name as ReleaseEnvironmentName, " +
                          "r.StartTime as ReleaseStartTime, " +
                          "r.FinishTime as ReleaseFinishTime, " +
                          "r.Name as ReleaseName, " +
                          "r.Attempts as ReleaseAttempts " +
                          "FROM TaskItem ti LEFT JOIN TaskItemType tit ON ti.TaskItemTypeId = tit.Id " +
                          "LEFT JOIN Release r ON ti.ReleaseId = r.Id " +
                          "LEFT JOIN ReleaseEnvironment re ON r.ReleaseEnvironmentId = re.Id " +
                          "WHERE ti.Id = @taskItemId";
                var taskItemInfo = (await databaseConnection.DbConnection.QueryAsync<TaskItemInfo>(sql, new {taskItemId}));

                return GetTaskItemListFromTaskItemInfo(taskItemInfo.ToList()).First();
            }
        }

        public async Task<TaskItemType[]> GetTaskItemTypesAsync()
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT Id FROM TaskItemType";
                var taskItemTypes = await databaseConnection.DbConnection
                    .QueryAsync<TaskItemType>(sql);
                return taskItemTypes.ToArray();
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

        private static List<TaskItem> GetTaskItemListFromTaskItemInfo(IEnumerable<TaskItemInfo> taskItems)
        {
            return taskItems.Select(taskItem => new TaskItem
                {
                    Id = taskItem.Id,
                    Title = taskItem.Title,
                    StartTime = taskItem.StartTime,
                    FinishTime = taskItem.FinishTime,
                    Type = (TaskItemType) taskItem.TaskItemTypeId,
                    DevelopmentTeamName = taskItem.DevelopmentTeamName,
                    CreatedOn = taskItem.CreatedOn,
                    CreatedBy = taskItem.CreatedBy,
                    LastChangedOn = taskItem.LastChangedOn,
                    LastChangedBy = taskItem.LastChangedBy,
                    CurrentBoardColumn = taskItem.CurrentBoardColumn,
                    CardState = taskItem.CardState,
                    Impact = taskItem.Impact,
                    CommentCount = taskItem.CommentCount,
                    NumRevisions = taskItem.NumRevisions,
                    Release = new Release
                    {
                        Id = taskItem.ReleaseId,
                        Status = taskItem.ReleaseStatus,
                        ReleaseEnvironment = new ReleaseEnvironment
                            {Id = taskItem.ReleaseEnvironmentId, Name = taskItem.ReleaseEnvironmentName},
                        StartTime = taskItem.ReleaseStartTime,
                        FinishTime = taskItem.ReleaseFinishTime,
                        Name = taskItem.ReleaseName,
                        Attempts = taskItem.ReleaseAttempts
                    }
                })
                .ToList();
        }

    }

    public class TaskItemInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public int TaskItemTypeId { get; set; }
        public string DevelopmentTeamName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastChangedOn { get; set; }
        public string LastChangedBy { get; set; }
        public string CurrentBoardColumn { get; set; }
        public string CardState { get; set; }
        public string Impact { get; set; }
        public int CommentCount { get; set; }
        public int NumRevisions { get; set; }
        public int ReleaseId { get; set; }
        public string ReleaseStatus { get; set; }
        public int ReleaseEnvironmentId { get; set; }
        public string ReleaseEnvironmentName { get; set; }
        public DateTime ReleaseStartTime { get; set; }
        public DateTime ReleaseFinishTime { get; set; }
        public string ReleaseName { get; set; }
        public int ReleaseAttempts { get; set; }
    }
}

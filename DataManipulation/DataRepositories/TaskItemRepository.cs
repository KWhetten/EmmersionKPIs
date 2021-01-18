using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccess.Deserialize.Kanbanize;
using DataAccess.Objects;
using Microsoft.TeamFoundation.Common;

namespace DataAccess.DataRepositories
{
    public interface ITaskItemRepository
    {
        Task<List<TaskItem>> GetTaskItemListAsync(DateTimeOffset? startDate, DateTimeOffset? finishDate);
        Task InsertTaskItemListAsync(IEnumerable<TaskItem> getTaskItemList);
        Task InsertTaskItemAsync(TaskItem taskItem);
        Task<TaskItem> GetTaskItemByIdAsync(int taskItemId);
        Task RemoveTaskItemByIdAsync(int cardId);
        Task<bool> TaskItemHasBeenReleasedAsync(int? id);
    }

    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly IDatabaseConnection databaseConnection;
        private readonly IReleaseRepository releaseRepository;

        public TaskItemRepository()
        {
            databaseConnection = new DatabaseConnection();
            releaseRepository = new ReleaseRepository();
        }

        public TaskItemRepository(IDatabaseConnection databaseConnection, IReleaseRepository releaseRepository)
        {
            this.databaseConnection = databaseConnection;
            this.releaseRepository = releaseRepository;
        }

        public virtual async Task<List<TaskItem>> GetTaskItemListAsync(DateTimeOffset? startDate,
            DateTimeOffset? finishDate)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.GetDbConnection())
            {
                var startDateString = $"{startDate:s}".Replace("T", " ");
                var endDateString = $"{finishDate:s}".Replace("T", " ");

                var sql = $"SELECT ti.Id, " +
                          "ti.Title, " +
                          "ti.StartTime, " +
                          "ti.FinishTime, " +
                          "ti.TaskItemTypeId, " +
                          "tit.Name as TaskItemTypeName, " +
                          "ti.developmentTeamId, " +
                          "ti.CreatedOn, " +
                          "ti.CreatedById, " +
                          "ti.LastChangedOn, " +
                          "ti.LastChangedById, " +
                          "ti.CurrentBoardColumn, " +
                          "ti.State, " +
                          "ti.NumRevisions, " +
                          "ti.ReleaseId, " +
                          "r.State as ReleaseState, " +
                          "r.ReleaseEnvironmentId, " +
                          "re.Name as ReleaseEnvironmentName, " +
                          "r.StartTime as ReleaseStartTime, " +
                          "r.FinishTime as ReleaseFinishTime, " +
                          "r.Name as ReleaseName, " +
                          "r.Attempts as ReleaseAttempts " +
                          "FROM TaskItems ti LEFT JOIN TaskItemTypes tit ON ti.TaskItemTypeId = tit.Id " +
                          "LEFT JOIN Releases r ON ti.ReleaseId = r.Id " +
                          "LEFT JOIN ReleaseEnvironments re ON r.ReleaseEnvironmentId = re.Id " +
                          "WHERE ti.FinishTime > @startDateString AND ti.FinishTime < @endDateString " +
                          "ORDER BY ti.CreatedOn";

                var taskItems = await databaseConnection.GetDbConnection()
                    .QueryAsync<TaskItemInfo>(sql, new {startDateString, endDateString});

                return await GetTaskItemListFromTaskItemInfoAsync(taskItems);
            }
        }

        public async Task InsertTaskItemListAsync(IEnumerable<TaskItem> getTaskItemList)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.GetDbConnection())
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
                var startTime = taskItem.StartTime;
                var finishTime = taskItem.FinishTime;
                var taskItemTypeId = (int) taskItem.Type;
                var developmentTeamId = taskItem.DevelopmentTeam.Id;
                var createdOn = taskItem.CreatedOn;
                var createdBy = taskItem.CreatedBy.Id;
                var lastChangedOn = taskItem.LastChangedOn;
                var lastChangedBy = taskItem.LastChangedBy.Id;
                var currentBoardColumn = taskItem.CurrentBoardColumn;
                var state = (int) taskItem.State;
                var numRevisions = taskItem.NumRevisions;
                var releaseId = taskItem.Release != null && taskItem.Release.Id != 0
                    ? taskItem.Release.Id
                    : (int?) null;

                var sql = "IF EXISTS(SELECT * FROM TaskItems WHERE Id = @taskItemId) " +
                          $"UPDATE TaskItems SET " +
                          "Title = @title, " +
                          "StartTime = @startTime, " +
                          "FinishTime = @finishTime, " +
                          "TaskItemTypeId = @taskItemTypeId, " +
                          "DevelopmentTeamId = @developmentTeamId, " +
                          "CreatedOn = @createdOn, " +
                          "CreatedById = @CreatedById, " +
                          "LastChangedOn = @lastChangedOn, " +
                          "LastChangedById = @LastChangedById, " +
                          "CurrentBoardColumn = @currentBoardColumn, " +
                          "State = @state, " +
                          "NumRevisions = @numRevisions, " +
                          "ReleaseId = @releaseId " +
                          "WHERE Id = @taskItemId " +
                          "ELSE " +
                          $"INSERT INTO TaskItems (Id, Title, StartTime, FinishTime, TaskItemTypeId, developmentTeamId, CreatedOn, CreatedById, " +
                          "LastChangedOn, LastChangedById, CurrentBoardColumn, State, NumRevisions, ReleaseId) " +
                          "VALUES (" +
                          "@taskItemId, " +
                          "@title, " +
                          "@startTime, " +
                          "@finishTime, " +
                          "@taskItemTypeId, " +
                          "@developmentTeamId, " +
                          "@createdOn, " +
                          "@CreatedById, " +
                          "@lastChangedOn, " +
                          "@LastChangedById, " +
                          "@currentBoardColumn, " +
                          "@state, " +
                          "@numRevisions, " +
                          "@releaseId)";
                await databaseConnection.GetDbConnection().ExecuteAsync(sql,
                    new
                    {
                        taskItemId, title, startTime, finishTime, taskItemTypeId, developmentTeamId, createdOn,
                        CreatedById = createdBy, lastChangedOn, LastChangedById = lastChangedBy, currentBoardColumn,
                        state, numRevisions, releaseId
                    });

                var historyEventsRepository = new HistoryEventRepository();
                await historyEventsRepository.InsertHistoryEventsAsync(taskItem);

                Console.WriteLine($"Updated Task: {taskItem.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to update task: {taskItem.Id} - " + ex.Message);
            }
        }

        public async Task<TaskItem> GetTaskItemByIdAsync(int taskItemId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.GetDbConnection())
            {
                var sql = $"SELECT ti.Id, " +
                          "ti.Title, " +
                          "ti.StartTime, " +
                          "ti.FinishTime, " +
                          "ti.TaskItemTypeId, " +
                          "tit.Name as TaskItemTypeName, " +
                          "ti.developmentTeamId, " +
                          "ti.CreatedOn, " +
                          "ti.CreatedById, " +
                          "ti.LastChangedOn, " +
                          "ti.LastChangedById, " +
                          "ti.CurrentBoardColumn, " +
                          "ti.State, " +
                          "ti.NumRevisions, " +
                          "ti.ReleaseId, " +
                          "r.State as ReleaseState, " +
                          "r.ReleaseEnvironmentId, " +
                          "re.Name as ReleaseEnvironmentName, " +
                          "r.StartTime as ReleaseStartTime, " +
                          "r.FinishTime as ReleaseFinishTime, " +
                          "r.Name as ReleaseName, " +
                          "r.Attempts as ReleaseAttempts " +
                          "FROM TaskItems ti LEFT JOIN TaskItemTypes tit ON ti.TaskItemTypeId = tit.Id " +
                          "LEFT JOIN Releases r ON ti.ReleaseId = r.Id " +
                          "LEFT JOIN ReleaseEnvironments re ON r.ReleaseEnvironmentId = re.Id " +
                          "WHERE ti.Id = @taskItemId;";

                var taskItemInfo =
                    (await databaseConnection.GetDbConnection().QueryAsync<TaskItemInfo>(sql, new {taskItemId}))
                    .ToList();

                return (await GetTaskItemListFromTaskItemInfoAsync(taskItemInfo.ToList())).First();
            }
        }

        public virtual async Task<bool> TaskItemHasBeenReleasedAsync(int? id)
        {
            try
            {
                var sql = $"SELECT * FROM TaskItems WHERE Id = @id AND State = 3";
                var result = (await databaseConnection.GetDbConnection().QueryAsync<TaskItem>(sql, new {id})).ToList();
                return result.Any();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task RemoveTaskItemByIdAsync(int cardId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.GetDbConnection())
            {
                var sql = $"DELETE FROM TaskItems WHERE Id = @cardId";
                await databaseConnection.GetDbConnection().ExecuteAsync(sql,
                    new {cardId});
            }
        }

        public static async Task<List<TaskItem>> GetTaskItemListFromTaskItemInfoAsync(
            IEnumerable<TaskItemInfo> taskItems)
        {
            try
            {
                var developmentTeamRepository = new DevelopmentTeamsRepository();
                var developerRepository = new DeveloperRepository();
                var newTaskItems = new List<TaskItem>();
                foreach (var taskItem in taskItems)
                {
                    var newTaskItem = new TaskItem
                    {
                        Id = taskItem.Id,
                        Title = taskItem.Title,
                        StartTime = taskItem.StartTime,
                        FinishTime = taskItem.FinishTime,
                        Type = (TaskItemType) taskItem.TaskItemTypeId,
                        DevelopmentTeam = await developmentTeamRepository.GetTeamAsync(taskItem.DevelopmentTeamId),
                        CreatedOn = taskItem.CreatedOn,
                        CreatedBy = await developerRepository.GetDeveloperByIdAsync(taskItem.CreatedById),
                        LastChangedOn = taskItem.LastChangedOn,
                        LastChangedBy = await developerRepository.GetDeveloperByIdAsync(taskItem.LastChangedById),
                        CurrentBoardColumn = taskItem.CurrentBoardColumn,
                        State = taskItem.State,
                        NumRevisions = taskItem.NumRevisions,
                        Release = new Release
                        {
                            Id = taskItem.ReleaseId,
                            State = taskItem.ReleaseState,
                            ReleaseEnvironment = new ReleaseEnvironment
                                {Id = taskItem.ReleaseEnvironmentId, Name = taskItem.ReleaseEnvironmentName},
                            StartTime = taskItem.ReleaseStartTime,
                            FinishTime = taskItem.ReleaseFinishTime,
                            Name = taskItem.ReleaseName,
                            Attempts = taskItem.ReleaseAttempts
                        }
                    };

                    newTaskItems.Add(newTaskItem);
                }

                return newTaskItems.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
            }

            return new List<TaskItem>();
        }
    }
}

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
        Task InsertHistoryEventsAsync(TaskItem taskItem);
        Task<TaskItem> GetTaskItemByIdAsync(int taskItemId);
        TaskItemType[] GetTaskItemTypes();
        Task RemoveTaskItemByIdAsync(int cardId);
        Task RemoveHistoryItemByIdAsync(int id);
        Task<List<HistoryEvent>> GetHistoryEventsByTaskIdAsync(int taskItemId);
    }

    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly DatabaseConnection databaseConnection;

        public TaskItemRepository()
        {
            databaseConnection = new DatabaseConnection();
        }

        public TaskItemRepository(DatabaseConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        private readonly ReleaseRepository releaseRepository = new ReleaseRepository();

        public virtual async Task<List<TaskItem>> GetTaskItemListAsync(DateTimeOffset? startDate, DateTimeOffset? finishDate)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var startDateString = $"{startDate:s}".Replace("T", " ");
                var endDateString = $"{finishDate:s}".Replace("T", " ");

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
                          "FROM TaskItem ti LEFT JOIN TaskItemType tit ON ti.TaskItemTypeId = tit.Id " +
                          "LEFT JOIN Release r ON ti.ReleaseId = r.Id " +
                          "LEFT JOIN ReleaseEnvironment re ON r.ReleaseEnvironmentId = re.Id " +
                          "WHERE ti.FinishTime > @startDateString AND ti.FinishTime < @endDateString " +
                          "ORDER BY ti.CreatedOn";

                try
                {
                    var taskItems = await databaseConnection.DbConnection
                        .QueryAsync<TaskItemInfo>(sql, new {startDateString, endDateString});

                    return GetTaskItemListFromTaskItemInfo(taskItems);

                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    return new List<TaskItem>();
                }

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
                var startTime = taskItem.StartTime;
                var finishTime = taskItem.FinishTime;
                var taskItemTypeId = (int) taskItem.Type;
                var developmentTeamName = !taskItem.DevelopmentTeam.IsNullOrEmpty()
                    ? taskItem.DevelopmentTeam
                    : "";
                var createdOn = taskItem.CreatedOn;
                var createdBy = !taskItem.CreatedBy.IsNullOrEmpty()
                    ? taskItem.CreatedBy
                    : null;
                var lastChangedOn = taskItem.LastChangedOn;
                var lastChangedBy = !taskItem.LastChangedBy.IsNullOrEmpty()
                    ? taskItem.LastChangedBy
                    : null;
                var currentBoardColumn = !taskItem.CurrentBoardColumn.IsNullOrEmpty()
                    ? taskItem.CurrentBoardColumn
                    : null;
                var state = (int) taskItem.State;
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
                          "State = @state, " +
                          "NumRevisions = @numRevisions, " +
                          "ReleaseId = @releaseId " +
                          "WHERE Id = @taskItemId " +
                          "ELSE " +
                          $"INSERT INTO TaskItem (Id, Title, StartTime, FinishTime, TaskItemTypeId, DevelopmentTeamName, CreatedOn, CreatedBy, " +
                          "LastChangedOn, LastChangedBy, CurrentBoardColumn, State, NumRevisions, ReleaseId) " +
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
                          "@state, " +
                          "@numRevisions, " +
                          "@releaseId)";
                await databaseConnection.DbConnection.ExecuteAsync(sql,
                    new
                    {
                        taskItemId, title, startTime, finishTime, taskItemTypeId, developmentTeamName, createdOn,
                        createdBy, lastChangedOn, lastChangedBy, currentBoardColumn, state, numRevisions, releaseId
                    });

                await InsertHistoryEventsAsync(taskItem);

                Console.WriteLine($"Inserted or Updated Task: {taskItem.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to insert task: {taskItem.Id} - " + ex.Message);
            }
        }

        public async Task InsertHistoryEventsAsync(TaskItem taskItem)
        {
            foreach (var historyEvent in taskItem.HistoryEvents)
            {
                try
                {
                    var taskItemDeserializer = new KanbanizeTaskItemDeserializer();
                    var historyEventId = historyEvent.Id;
                    var eventDate =
                        historyEvent.EventDate;
                    var taskItemColumn = historyEvent.TaskItemColumn;
                    var taskItemState = taskItemDeserializer.GetTaskItemState(taskItemColumn);
                    var eventType = historyEvent.EventType;
                    var taskItemId = taskItem.Id;
                    var author = historyEvent.Author;

                    var sql = $"IF EXISTS(SELECT * FROM HistoryEvents WHERE Id = @historyEventId) " +
                              $"UPDATE HistoryEvents SET " +
                              "EventDate = @eventDate, " +
                              "TaskItemColumn = @taskItemColumn, " +
                              "TaskItemState = @taskItemState, " +
                              "EventType = @eventType, " +
                              "Author = @author " +
                              "WHERE Id = @historyEventId " +
                              "ELSE " +
                              $"INSERT INTO HistoryEvents (Id, EventDate, TaskItemColumn, TaskItemState, EventType, Author, TaskItemId) " +
                              "VALUES(@historyEventId, @eventDate, @taskItemColumn, @taskItemState, @eventType, @author, @taskItemId);";

                    await databaseConnection.DbConnection.ExecuteAsync(sql,
                        new
                        {
                            historyEventId, eventDate, taskItemColumn, taskItemState, eventType, taskItemId, author
                        });

                    Console.WriteLine($"Inserted History Event {historyEventId} for Task {taskItemId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to insert or update HistoryEvent #{historyEvent.Id} for Task: #{taskItem.Id}: {ex.Message}");
                }
            }
        }

        public async Task<TaskItem> GetTaskItemByIdAsync(int taskItemId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
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
                          "FROM TaskItem ti LEFT JOIN TaskItemType tit ON ti.TaskItemTypeId = tit.Id " +
                          "LEFT JOIN Release r ON ti.ReleaseId = r.Id " +
                          "LEFT JOIN ReleaseEnvironment re ON r.ReleaseEnvironmentId = re.Id " +
                          "WHERE ti.Id = @taskItemId;";

                var taskItemInfo =
                    (await databaseConnection.DbConnection.QueryAsync<TaskItemInfo>(sql, new {taskItemId}))
                    .ToList();

                return GetTaskItemListFromTaskItemInfo(taskItemInfo.ToList()).First();
            }
        }

        public bool TaskItemHasAlreadyBeenReleasedAsync(int id)
        {
            try
            {
                var sql = $"SELECT * FROM TaskItem WHERE Id = @id AND State = 3";
                var result = databaseConnection.DbConnection.Query<TaskItem>(sql, new {id}).First();
                return result != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public TaskItemType[] GetTaskItemTypes()
        {
            databaseConnection.GetNewConnection();
            using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT Id FROM TaskItemType";
                var taskItemTypes = databaseConnection.DbConnection
                    .Query<TaskItemType>(sql);
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

        public async Task RemoveHistoryItemByIdAsync(int id)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"DELETE FROM HistoryEvents WHERE Id = @id";
                await databaseConnection.DbConnection.ExecuteAsync(sql,
                    new {id});
            }
        }

        private static List<TaskItem> GetTaskItemListFromTaskItemInfo(IEnumerable<TaskItemInfo> taskItems)
        {
            try
            {
                return taskItems.Select(taskItem => new TaskItem
                    {
                        Id = taskItem.Id,
                        Title = taskItem.Title,
                        StartTime = taskItem.StartTime,
                        FinishTime = taskItem.FinishTime,
                        Type = (TaskItemType) taskItem.TaskItemTypeId,
                        DevelopmentTeam = taskItem.DevelopmentTeamName,
                        CreatedOn = taskItem.CreatedOn,
                        CreatedBy = taskItem.CreatedBy,
                        LastChangedOn = taskItem.LastChangedOn,
                        LastChangedBy = taskItem.LastChangedBy,
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
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
            }
            return new List<TaskItem>();
        }

        public virtual async Task<List<HistoryEvent>> GetHistoryEventsByTaskIdAsync(int taskItemId)
        {
            databaseConnection.GetNewConnection();
            await using (databaseConnection.DbConnection)
            {
                var sql = $"SELECT * FROM HistoryEvents WHERE TaskItemId = @taskItemId";
                var result = (await databaseConnection.DbConnection.QueryAsync<HistoryEvent>(sql, new {taskItemId}))
                    .ToList();
                return result;
            }
        }
    }

    public class  TaskItemInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset FinishTime { get; set; }
        public int TaskItemTypeId { get; set; }
        public string DevelopmentTeamName { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset LastChangedOn { get; set; }
        public string LastChangedBy { get; set; }
        public string CurrentBoardColumn { get; set; }
        public TaskItemState State { get; set; }
        public int NumRevisions { get; set; }
        public int ReleaseId { get; set; }
        public string ReleaseState { get; set; }
        public int ReleaseEnvironmentId { get; set; }
        public string ReleaseEnvironmentName { get; set; }
        public DateTimeOffset ReleaseStartTime { get; set; }
        public DateTimeOffset ReleaseFinishTime { get; set; }
        public string ReleaseName { get; set; }
        public int ReleaseAttempts { get; set; }
    }
}

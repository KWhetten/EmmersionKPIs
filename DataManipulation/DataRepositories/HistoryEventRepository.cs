using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DataAccess.Deserialize.Kanbanize;
using DataAccess.Objects;

namespace DataAccess.DataRepositories
{
    public interface IHistoryEventRepository
    {
        Task<bool> InsertHistoryEventsAsync(TaskItem taskItem);
        Task RemoveHistoryItemByIdAsync(int id);
        Task<List<HistoryEvent>> GetHistoryEventsByTaskIdAsync(int taskItemId);
    }
    public class HistoryEventRepository : IHistoryEventRepository
    {
        private readonly DatabaseConnection databaseConnection;

        public HistoryEventRepository()
        {
            databaseConnection = new DatabaseConnection();
        }

        public async Task<bool> InsertHistoryEventsAsync(TaskItem taskItem)
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

                    Console.WriteLine($"Updated History Event {historyEventId} for Task {taskItemId}");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to update HistoryEvent #{historyEvent.Id} for Task: #{taskItem.Id}: {ex.Message}");
                    return false;
                }
            }

            Console.WriteLine($"There are no History Events for Task: #{taskItem.Id}");
            return true;
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
}

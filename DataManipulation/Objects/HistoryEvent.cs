using System;

namespace DataAccess.Objects
{
    public class HistoryEvent
    {
        public int Id { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public string EventType { get; set; }
        public string TaskItemColumn { get; set; }
        public TaskItemState TaskItemState { get; set; }
        public string Author { get; set; }
        public int TaskId { get; set; }
    }
}

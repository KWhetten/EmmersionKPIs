using System;

namespace DataAccess.Objects
{
    public class TaskItemInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset FinishTime { get; set; }
        public int TaskItemTypeId { get; set; }
        public int DevelopmentTeamId { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public int CreatedById { get; set; }
        public DateTimeOffset LastChangedOn { get; set; }
        public int LastChangedById { get; set; }
        public BoardColumn CurrentBoardColumn { get; set; }
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

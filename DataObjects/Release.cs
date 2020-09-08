using System;

namespace DataObjects
{
    public class Release
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public ReleaseEnvironment ReleaseEnvironment { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public string Name { get; set; }
        public int Attempts { get; set; }
    }
}

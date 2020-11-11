﻿using System;

 namespace DataAccess.Objects
{
    public class Release
    {
        public int Id { get; set; }
        public string State { get; set; }
        public ReleaseEnvironment ReleaseEnvironment { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? FinishTime { get; set; }
        public string Name { get; set; }
        public int Attempts { get; set; }
    }
}

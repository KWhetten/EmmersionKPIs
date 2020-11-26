using System;
using System.Collections.Generic;

namespace KPIDataExtractor.UnitTests.TestObjects.Kanbanize
{
    public class JsonHistoryEvent
    {
        public int taskid { get; set; }
        public int historyid { get; set; }
        public string historyevent { get; set; }
        public string details { get; set; }
        public DateTimeOffset entrydate { get; set; }
        public string author { get; set; }
    }
}

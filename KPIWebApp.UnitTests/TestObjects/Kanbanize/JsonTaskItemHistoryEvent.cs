using System;

namespace KPIDataExtractor.UnitTests.TestObjects.Kanbanize
{
    public class JsonHistoryEvent
    {
        public int historyid { get; set; }
        public string historyevent { get; set; }
        public string details { get; set; }
        public DateTime entrydate { get; set; }
        public string author { get; set; }
    }
}

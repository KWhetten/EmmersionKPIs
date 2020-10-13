using System;

namespace KPIDataExtractor.UnitTests.TestObjects.Kanbanize
{
    public class JsonTaskItemHistory
    {
        public string historyevent { get; set; }
        public string details { get; set; }
        public DateTime entrydate { get; set; }
        public string author { get; set; }
    }
}

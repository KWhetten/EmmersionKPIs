using System;

namespace KPIDataExtractor.UnitTests.Objects.Kanbanize
{
    public class JsonWorkItemCardHistory
    {
        public string historyevent { get; set; }
        public string details { get; set; }
        public DateTime entrydate { get; set; }
        public string author { get; set; }
    }
}

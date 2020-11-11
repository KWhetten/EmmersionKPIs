using System;

namespace KPIDataExtractor.UnitTests.TestObjects.Kanbanize
{
    public class JsonTaskItem
    {
        public int taskid { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public DateTimeOffset createdat { get; set; }
        public DateTimeOffset updatedat { get; set; }
        public string columnid { get; set; }
        public string columnname { get; set; }
        public string priority { get; set; }
        public string[] comments { get; set; }
    }
}

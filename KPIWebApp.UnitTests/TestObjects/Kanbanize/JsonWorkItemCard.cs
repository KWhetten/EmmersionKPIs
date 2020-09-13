using System;

namespace KPIDataExtractor.UnitTests.Objects.Kanbanize
{
    public class JsonWorkItemCard
    {
        public int taskid { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public DateTime createdat { get; set; }
        public string reporter { get; set; }
        public DateTime updatedat { get; set; }
        public string columnid { get; set; }
        public string columnname { get; set; }
        public string priority { get; set; }
        public string[] comments { get; set; }
        public JsonWorkItemCardLinks links { get; set; }
    }

    public class JsonWorkItemCardLinks
    {
        public int child { get; set; }
    }
}

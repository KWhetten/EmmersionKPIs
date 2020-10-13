using System;

namespace KPIWebApp.IntegrationTests.TestObjects.Kanbanize
{
    public class JsonTaskItem
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
        public JsonTaskItemLinks links { get; set; }
    }

    public class JsonTaskItemLinks
    {
        public int child { get; set; }
    }
}

using System;

namespace KPIDataExtractor.UnitTests.Objects.DevOps
{
    public class JsonWorkItemCard
    {
        public int id { get; set; }
        public JsonWorkItemCardFields fields { get; set; }
        public int rev { get; set; }
    }

    public class JsonWorkItemCardFields
    {
        public string SysTitle { get; set; }
        public string SysWorkItemType { get; set; }
        public string SysBoardLane { get; set; }
        public DateTime SysCreatedDate { get; set; }
        public JsonWorkItemCardCreatedBy SysCreatedBy { get; set; }
        public DateTime SysChangedDate { get; set; }
        public JsonWorkItemCardChangedBy SysChangedBy { get; set; }
        public string SysBoardColumn { get; set; }
        public string SysState { get; set; }
        public string CusImpact { get; set; }
        public int SysCommentCount { get; set; }
    }

    public class JsonWorkItemCardCreatedBy
    {
        public string displayName { get; set; }
    }

    public class JsonWorkItemCardChangedBy
    {
        public string displayName { get; set; }
    }
}

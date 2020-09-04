using System;

namespace KPIDataExtractor.UnitTests.Objects.DevOps
{
    public class JsonWorkItemCardUpdate
    {
        public JsonWorkItemCardUpdateFields fields { get; set; }
    }

    public class JsonWorkItemCardUpdateFields
    {
        public JsonWorkItemCardBoardColumn SysBoardColumn { get; set; }
        public JsonWorkItemCardState SysState { get; set; }
        public JsonWorkItemCardChangedDate SysChangedDate { get; set; }
    }

    public class JsonWorkItemCardState
    {
        public string newValue { get; set; }
    }

    public class JsonWorkItemCardChangedDate
    {
        public DateTime newValue { get; set; }
    }

    public class JsonWorkItemCardBoardColumn
    {
        public string oldValue { get; set; }
    }
}

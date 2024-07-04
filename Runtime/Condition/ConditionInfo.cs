namespace SaintsField.Condition
{
    public struct ConditionInfo
    {
        // ReSharper disable InconsistentNaming
        public string Target;
        public LogicCompare Compare;
        public object Value;
        public bool ValueIsCallback;
        public bool Reverse;
        // ReSharper enable InconsistentNaming
    }
}

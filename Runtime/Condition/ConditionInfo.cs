namespace SaintsField.Condition
{
    public struct ConditionInfo
    {
        public object Target;
        public LogicCompare Compare;
        public object Value;
        public bool ValueIsCallback;
        public bool Reverse;
    }
}

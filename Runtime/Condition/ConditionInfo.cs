namespace SaintsField.Condition
{
    public struct ConditionInfo
    {
        public object Target;
        public LogicCompare Compare;
        public object Value;
        public bool ValueIsCallback;
        public bool Reverse;

        public override string ToString()
        {
            return $"<Condition Target={Target} Compare={Compare} Value={Value} ValueIsCallback={ValueIsCallback} Reverse={Reverse} />";
        }
    }
}

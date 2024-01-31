﻿namespace SaintsField
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
    public class PostFieldButtonAttribute : DecButtonAttribute
    {
        public PostFieldButtonAttribute(string funcName, string buttonLabel=null, bool isCallback = false, string groupBy = "") : base(funcName, buttonLabel, isCallback, groupBy)
        {
        }
    }
}

using System;

namespace ExtInspector
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DOTweenPreviewAttribute: Attribute
    {
        public readonly string text;

        public enum StopAction
        {
            None,
            Complete,
            Rewind,
        }

        public readonly StopAction onManualStop;

        public DOTweenPreviewAttribute(string text = null, StopAction stopAction = StopAction.Rewind)
        {
            this.text = text;
            onManualStop = stopAction;
        }
    }
}

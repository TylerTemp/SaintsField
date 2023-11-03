using System;

namespace SaintsField.Editor.Core
{
    public class InsideSaintsFieldScoop: IDisposable
    {
        public InsideSaintsFieldScoop()
        {
            SaintsPropertyDrawer.IsSubDrawer = true;
        }

        public void Dispose()
        {
            SaintsPropertyDrawer.IsSubDrawer = false;
        }
    }
}

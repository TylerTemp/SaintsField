using System;

namespace SaintsField.Editor.Core
{
    public class InsideSaintsFieldScoop: IDisposable
    {
        public InsideSaintsFieldScoop()
        {
            SaintsPropertyDrawer.isSubDrawer = true;
        }

        public void Dispose()
        {
            SaintsPropertyDrawer.isSubDrawer = false;
        }
    }
}

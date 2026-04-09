using System.Collections;
using UnityEngine;

namespace SaintsField.Editor.Utils.WaitableUtils
{
    public class Waiter
    {
        public readonly IEnumerator Enumerator;
        public IWaitable Waitable;

        public Waiter(IEnumerator enumerator)
        {
            Enumerator = enumerator;
        }

        public void CheckCurrent()
        {
            switch (Enumerator.Current)
            {
                case null:
                    Waitable = null;
                    return;
                case WaitForSeconds ws:
                    Waitable = new WaitableWaitForSeconds(ws);
                    return;
                case WaitForSecondsRealtime ws:
                    Waitable = new WaitableWaitForSeconds(ws);
                    return;
                case WaitUntil wu:
                    Waitable = new WaitableWaitForCallback(wu);
                    return;
                case WaitWhile ww:
                    Waitable = new WaitableWaitForCallback(ww);
                    return;
                case AsyncOperation asyncOperation:
                    Waitable = new WaitableWaitForAsyncOperation(asyncOperation);
                    return;
                default:
                    Waitable = null;
                    return;
            }
        }

        public void Update()
        {
            if (!Done())
            {
                Waitable.Update();
            }
        }

        public bool Done()
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (Waitable is null)
            {
                return true;
            }
            return Waitable.Done;
        }
    }
}

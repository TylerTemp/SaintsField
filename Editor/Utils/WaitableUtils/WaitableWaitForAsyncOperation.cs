using UnityEngine;

namespace SaintsField.Editor.Utils.WaitableUtils
{
    public class WaitableWaitForAsyncOperation: IWaitable
    {
        private readonly AsyncOperation _asyncOperation;
        public WaitableWaitForAsyncOperation(AsyncOperation asyncOp)
        {
            _asyncOperation = asyncOp;
        }

        public bool Done => _asyncOperation.isDone;
        public void Update()
        {
        }
    }
}

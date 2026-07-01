using System.Threading.Tasks;
using UnityEngine;
#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
using Cysharp.Threading.Tasks;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class FieldButtonAsync : MonoBehaviour
    {
        [AboveButton(nameof(AsyncVoid))]
        [BelowButton(nameof(AsyncWithInt))]
#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
        [PostFieldButton(nameof(AsyncUniTaskBase), "<icon=star.png/>")]
        [BelowButton(nameof(AsyncUniTaskValue))]
#endif
        public bool ok;

        private async Task AsyncVoid()
        {
            Debug.Log("Async start");
            await Task.Delay(1000);
            Debug.Log("Async end");
        }

        private async Task<int> AsyncWithInt()
        {
            Debug.Log("Async start");
            await Task.Delay(1000);
            Debug.Log("Async end");
            return 100;
        }

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
        private async UniTask AsyncUniTaskBase()
        {
            Debug.Log("Async start");
            // await UniTask.Yield();
            await UniTask.WaitUntil(() => ok);
            // throw new Exception("xx");
            Debug.Log("Async end");
        }
        private async UniTask<string> AsyncUniTaskValue()
        {
            Debug.Log("Async start");
            // await UniTask.Yield();
            await UniTask.WaitUntil(() => ok);
            // throw new Exception("xx");
            return "fine";
        }
#endif
    }
}

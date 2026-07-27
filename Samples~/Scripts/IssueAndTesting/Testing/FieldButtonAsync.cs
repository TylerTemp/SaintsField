using System.Collections;
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

        [AboveButton(nameof(IeWait), "IEnumerator Above")]
        [BelowButton(nameof(IeWait), "IEnumerator Below")]
        [PostFieldButton(nameof(IeWait), "IE")]
        public bool okIe;

#if UNITY_6000_0_OR_NEWER
        [AboveButton(nameof(AsyncAwaitableBase), "Awaitable Above")]
        [BelowButton(nameof(AsyncAwaitableValue), "Awaitable<T> Below")]
        [PostFieldButton(nameof(AsyncAwaitableBase), "A")]
        public bool okAwaitable;
#endif

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

        private IEnumerator IeWait()
        {
            Debug.Log("IEnumerator start");
            yield return new WaitUntil(() => okIe);
            Debug.Log("IEnumerator end");
        }

#if UNITY_6000_0_OR_NEWER
        private async Awaitable AsyncAwaitableBase()
        {
            Debug.Log("Awaitable start");
            await Awaitable.WaitForSecondsAsync(1);
            Debug.Log("Awaitable end");
        }

        private async Awaitable<string> AsyncAwaitableValue()
        {
            Debug.Log("Awaitable<T> start");
            await Awaitable.WaitForSecondsAsync(1);
            Debug.Log("Awaitable<T> end");
            return "fine";
        }
#endif

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

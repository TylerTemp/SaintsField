using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
using Cysharp.Threading.Tasks;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing
{
    public class TickDropdown : MonoBehaviour
    {
        public bool showError = true;

        [Dropdown(nameof(BookDrop))] public string bookName;

        private async Task<Dropdown<string>> BookDrop()
        {
            await Task.Delay(600);

            if (showError)
            {
                throw new Exception("Some error happened here");
            }

            Dropdown<string> result = new Dropdown<string>();

            foreach (string item in new[]
                     {
                         "Hackers & Painters, Vol 1",
                         "Hackers & Painters, Vol 2",
                         "The Art of Unix Programming, Vol 1",
                         "The Art of Unix Programming, Vol 2",
                         "The Mythical Man-Month, Vol 1",
                         "The Mythical Man-Month, Vol 2",
                     })
            {
                result.Add(item, item);
            }

            return result;
        }

        [Dropdown(nameof(IEDrop))] public int ieDrop;

        private IEnumerator IEDrop()
        {
            yield return new WaitForSeconds(3);
            yield return new Dropdown<int>
            {
                { "One", 1 },
                { "Two", 2 },
                { "Three", 3 },
            };
        }

#if UNITY_6000_0_OR_NEWER
        [Dropdown(nameof(AwaitableDrop))] public Color awaitableDrop;

        private async Awaitable<Dropdown<Color>> AwaitableDrop()
        {
            await Awaitable.WaitForSecondsAsync(1);

            return new Dropdown<Color>
            {
                { "<color=red>Red", Color.red },
                { "<color=green>Green", Color.green },
                { "<color=blue>Blue", Color.blue },
            };
        }
#endif

#if SAINTSFIELD_UNITASK && !SAINTSFIELD_UNITASK_DISABLE
        [Dropdown(nameof(QuickDrop))] public float percent;

        private async UniTask<Dropdown<float>> QuickDrop()
        {
            await UniTask.Delay(600);

            Dropdown<float> result = new Dropdown<float>
            {
                { "20%", 0.2f },
                { "40%", 0.4f },
                { "60%", 0.6f },
            };
            // `Add` is supported
            result.Add("80%", 0.8f);
            // rich tag is supported
            result.Add($"<color={EColor.GoldenRod}>100%<icon=lightMeter/redLight/>", 1f);
            // disable is supported
            result.Add("120%", 1.2f, true);
            return result;
        }
#endif

        // [Space(150)]
        [ValueButtons(nameof(GetTransAdvanced), noFold: true)] public int transformAdvanced;

        private async Task<Dropdown<int>> GetTransAdvanced()
        {
            await Task.Delay(500);

            Dropdown<int> list = new Dropdown<int>();
            for (int number = 0; number < 10; number++)
            {
                list.Add($"{number}", number);
            }

            return list;
        }

#if UNITY_6000_0_OR_NEWER
        [ValueButtons(nameof(GetAwaitableBtn), noFold: true)] public int awaitableBtn;

        private async Awaitable<Dropdown<int>> GetAwaitableBtn()
        {
            await Awaitable.WaitForSecondsAsync(2);

            Dropdown<int> list = new Dropdown<int>();
            for (int number = 0; number < 10; number++)
            {
                list.Add($"{number}", number);
            }

            return list;
        }
#endif
    }
}

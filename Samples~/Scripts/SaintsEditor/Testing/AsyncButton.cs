using System.Threading.Tasks;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Testing
{
    public class AsyncButton : SaintsMonoBehaviour
    {
        [Button]
        private async Task<int> AsyncWithInt()
        {
            Debug.Log("Async start");
            await Task.Delay(1000);
            Debug.Log("Async end");
            return 100;
        }
    }
}

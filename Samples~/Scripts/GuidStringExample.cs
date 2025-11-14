using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GuidStringExample : SaintsMonoBehaviour
    {
        // example
        // 6d726691-a97d-4c4e-9caa-651de409e5db
        // [ShowInInspector] public static Guid guid = Guid.NewGuid();
        // [ShowInInspector] public static string guidString => guid.ToString();

        [OnValueChanged(nameof(OnValueChanged))]
        [Guid] public string guidString;

        [ShowInInspector, Guid]
        public string ShowGuidString
        {
            get => guidString;
            set => guidString = value;
        }

        private void OnValueChanged(string v) => Debug.Log(v);
    }
}

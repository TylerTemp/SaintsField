using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue309LayoutOverlap : SaintsMonoBehaviour
    {

		[LayoutStart("receiver",ELayout.Background)]
		public Transform _receiverTransform;

		// [HideIf(nameof(_receiverTransform)),HideIf(nameof(randomReceiver))]
		// private bool receiverWorldSpace;

		// [HideIf(nameof(_receiverTransform)),HideIf(nameof(receiverWorldSpace))]
		// public bool randomReceiver;


		// public bool receiverHitsRaycast;

		[ShowIf(false)]
		public Transform placeAtRaycastHit;

		// [HideIf(nameof(_receiverTransform)),HideIf(nameof(randomReceiver))]
		public Vector3 _receiverPosition = Vector3.right*5;

		// public Vector3 ReceiverPosition {get {return default;}set {}}

    }
}

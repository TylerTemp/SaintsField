using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue385XPathParentType : SaintsMonoBehaviour
    {
        [GetByXPath("//*@{GetComponent(Collider)}")] public GameObject[] colliderGameObjects;
        [GetByXPath("//*@{GetComponent(Collider2D)}")] public GameObject[] collider2DGameObjects;
    }
}

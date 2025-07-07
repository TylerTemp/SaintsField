using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Testing.XPathFilterBool
{
    public class XPathFilterBoolGetter : MonoBehaviour
    {
        [SerializeField, GetByXPath("asset:://Samples/RawResources/XPathFilterBool/*[@{_boolValue} = false]"), Expandable]
        private XPathFilterBoolValue _xPathFilterBoolFalse;
    }
}

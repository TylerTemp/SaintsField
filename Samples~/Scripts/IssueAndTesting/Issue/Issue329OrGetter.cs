using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue329OrGetter : MonoBehaviour
    {
        [GetByXPath(EXP.JustPicker, "scene:://[@{GetComponent(ParticleSystem)}]")]
        [GetByXPath(EXP.JustPicker, "scene:://[@{GetComponent(SpriteRenderer)}]")]
        [GetByXPath(EXP.JustPicker, "assets:://[@{GetComponent(ParticleSystem)}]")]
        [GetByXPath(EXP.JustPicker, "assets:://[@{GetComponent(SpriteRenderer)}]")]
        [Expandable]
        public GameObject[] particleOrSpriteRenderer;
    }
}

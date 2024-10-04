using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class GetComponentByPathExample: MonoBehaviour
    {
        // starting from root, search any object with name "Dummy"
        [GetComponentByPath("///Dummy")] public GameObject dummy;
        // first child of current object
        [GetComponentByPath("./*[1]")] public GameObject direct1;
        // child of current object which has index greater than 1
        [GetComponentByPath("./*[index() > 1]")] public GameObject directPosTg1;
        // last child of current object
        [GetComponentByPath("./*[last()]")] public GameObject directLast;
        // re-sign the target if mis-match
        [GetComponentByPath(EGetComp.NoResignButton | EGetComp.ForceResign, "./DirectSub")] public GameObject directSubWatched;
        // without "ForceResign", it'll display a reload button if mis-match
        // with multiple paths, it'll search from left to right
        [GetComponentByPath("/no", "./DirectSub1")] public GameObject directSubMulti;
        // if no match, it'll show an error message
        [GetComponentByPath("/no", "///sth/else/../what/.//ever[last()]/goes/here")] public GameObject notExists;

        [GetComponentByPath("/Sth")] public GameObject slashSth;
        [GetComponentByPath("//Sth")] public GameObject slash2Sth;
        [GetComponentByPath("///Sth")] public GameObject slash3Sth;

        [ReadOnly]
        [GetComponentByPath("/no", "./DirectSub1")] public GameObject directSubMultiDisabled;

        [Separator("GetByXPath")]
        [GetByXPath("scene:://Dummy")] public GameObject dummyXPath;
        [GetByXPath("./*[1]")] public GameObject direct1XPath;
        [GetByXPath("./*[index() > 1]")] public GameObject directPosTg1XPath;
        [GetByXPath("./*[last()]")] public GameObject directLastXPath;
        [GetByXPath("/no", "./DirectSub1")] public GameObject directSubMultiXPath;
        [Required]
        [GetByXPath("/no", "scene:://sth/else/../what/.//ever[last()]/goes/here")] public GameObject notExistsXPath;

        [GetByXPath("scene::/Sth")] public GameObject slashSthXPath;
        [GetByXPath("scene:://Sth")] public GameObject slash2SthXPath;
    }
}

using System;
using UnityEngine;

public interface ITestable
{

}

[Serializable]
public class Foo : ITestable
{
    public float FooValue;
}
[Serializable]
public class Bar : ITestable
{
    public bool BarValue;
}
[Serializable]
public class Baz : ITestable
{
    public string BazValue;
}

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue352ReferencePicker : MonoBehaviour
    {


        [SerializeReference, ReferencePicker] private ITestable _testable;
    }
}

using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting
{
    public class TestNaughtyDropdownBug : MonoBehaviour
    {
        [SerializeField] private TestClass testClass;
        [SerializeField] private TestStruct testStruct;
    }

    public static class TestStatic
    {
        public static DropdownList<string> TestDropdown = new DropdownList<string>{
            { "Default", "Default" },
            { "Value A", "Value A" },
            { "Value B", "Value B" },
        };
    }

    [Serializable]
    public class TestClass
    {
        [SerializeField, Dropdown(nameof(drop))] private string test;
        public DropdownList<string> drop => TestStatic.TestDropdown;
    }

    [Serializable]
    public struct TestStruct
    {
        [SerializeField, Dropdown(nameof(drop))] private string test;
        public DropdownList<string> drop => TestStatic.TestDropdown;
    }
}

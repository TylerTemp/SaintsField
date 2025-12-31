using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue352AbstractReferencePicker: MonoBehaviour
    {
        public interface ITestNode
        {
        }

        [Serializable]
        public abstract class CompositeNodeBase : ITestNode
        {
            [SerializeReference, ReferencePicker] private ITestNode[] _childNodes;
            [SerializeField] private bool _isTest;
        }

        [Serializable]
        public class FooCompositeNode : CompositeNodeBase
        {
            [SerializeField] private float _fooValue;
        }

        [Serializable]
        public class BarCompositeNode : CompositeNodeBase
        {
            [SerializeField] private float _barValue;
        }

        [Serializable]
        public class QuoxCompositeNode : CompositeNodeBase
        {
            [SerializeField] private float _quoxValue;
        }


        [Serializable]
        public class ANode : ITestNode
        {
            [SerializeField] private float _a;
        }


        [Serializable]
        public class BNode : ITestNode
        {
            [SerializeField] private float _b;
        }

        [Serializable]
        public class CNode : ITestNode
        {
            [SerializeField] private float _c;
        }

        [Serializable]
        public class DNode : ITestNode
        {
            [SerializeField] private float _d;
        }

        [Serializable]
        public class ENode : ITestNode
        {
            [SerializeField] private float _e;
        }

        [SerializeReference, ReferencePicker] private ITestNode[] _nodes;

    }
}

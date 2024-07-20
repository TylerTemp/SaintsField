using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues
{
    public class Issue57 : SaintsMonoBehaviour
    {
        [ShowInInspector]
        private Dictionary<int, int> kvInt => new Dictionary<int, int>
        {
            { 1, 2 },
            { 3, 4 },
        };

        private struct MyInfo
        {
            public string key1;
            public string key2;

            public override string ToString()
            {
                return $"{key1}&{key2}";
            }
        }

        [ShowInInspector]
        private Dictionary<MyInfo, MyInfo[]> kv => new Dictionary<MyInfo, MyInfo[]>
        {
            {new MyInfo {key1 = "key1.1", key2 = "key1.2"}, new[]
                {
                    new MyInfo
                    {
                        key1 = "key1.[0]v1", key2 = "key1.[0]v2",
                    },
                    new MyInfo
                    {
                        key1 = "key1.[1]v1", key2 = "key1.[1]v2",
                    },
                }
            },
            {new MyInfo {key1 = "key2.1", key2 = "key2.2"}, new MyInfo[] { } },
        };

        [ShowInInspector] private (int myInt, MyInfo myInfo) myTuple => (myInt: 1, myInfo: new MyInfo {key1 = "key1.1", key2 = "key1.2"});

        [Serializable]
        public struct MyStruct
        {
            [ShowInInspector]
            private Dictionary<int, int> kvInt => new Dictionary<int, int>
            {
                { 1, 2 },
                { 3, 4 },
            };

            [ShowInInspector]
            private Dictionary<MyInfo, MyInfo[]> kv => new Dictionary<MyInfo, MyInfo[]>
            {
                {new MyInfo {key1 = "key1.1", key2 = "key1.2"}, new[]
                    {
                        new MyInfo
                        {
                            key1 = "key1.[0]v1", key2 = "key1.[0]v2",
                        },
                        new MyInfo
                        {
                            key1 = "key1.[1]v1", key2 = "key1.[1]v2",
                        },
                    }
                },
                {new MyInfo {key1 = "key2.1", key2 = "key2.2"}, new MyInfo[] { } },
            };

            [ShowInInspector] private (int myInt, MyInfo myInfo) myTuple => (myInt: 1, myInfo: new MyInfo {key1 = "key1.1", key2 = "key1.2"});
        }

        [SaintsRow] public MyStruct myStruct;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using SaintsField.Playa;
#if UNITY_EDITOR
using SaintsField.SaintsXPathParser;
#endif
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.EPathExample
{
    public class EPathDebug: SaintsMonoBehaviour
    {
        [Serializable]
        public class PathAndResults
        {
            [ResizableTextArea, Ordered] public string xPath;

#if UNITY_EDITOR
            [Button("Test"), Ordered]
            private void Test()
            {
                List<string> toStrings = new List<string>();
                foreach (XPathStep xPathStep in XPathParser.Parse(xPath))
                {
                    Debug.Log(xPathStep);
                    toStrings.Add(xPathStep.ToString());
                }

                results = toStrings.ToArray();
            }
#endif

            [ResizableTextArea, ReadOnly, Ordered] public string[] results;

        }

        [SaintsRow] public PathAndResults[] pathAndResults;

        // [InfoBox("::scene-root/Issues*[last()]//[@{GetComponent(EPathDebug).enabled}]", EMessageType.None)]
        // [SaintsPath("::scene-root/Issues*[last()]//[@{GetComponent(EPathDebug).enabled}]")]
        // public EPathDebug myself;

        // [GetByXPath("Sub@{GetComponent(Dummy).GetTargetTransform()}")]
        // [GetByXPath("..")]
        // public Transform c;
        //
        // [Button("Test")]
        // private void T()
        // {
        //     var d = new Dictionary<string, EPathDebug>();
        //     // Debug.Log(d["Key"]);
        //     // Debug.Log(d["Key"]);
        //
        //     Type objType = d.GetType();
        //     foreach (Type interfaceType in objType.GetInterfaces())
        //     {
        //         // if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
        //         //     interfaceType.GetGenericArguments()[0].IsGenericType &&
        //         //     interfaceType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        //         // {
        //         //     Debug.Log(interfaceType.GetGenericArguments()[0].GetGenericArguments()[0]);
        //         //     Debug.Log("Found");
        //         // }
        //         // if (interfaceType.IsGenericType &&
        //         //     (interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>) || interfaceType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)))
        //         // {
        //         //     Debug.Log(interfaceType.GetGenericArguments()[0]);
        //         //     Debug.Log("Found");
        //         //
        //         //     if (d is IDictionary<string, object>)
        //         //     {
        //         //         Debug.Log("String Obj!");
        //         //     }
        //         //
        //         //     Debug.Log(d is IDictionary);
        //         // }
        //         Debug.Log(interfaceType == typeof(IDictionary));
        //     }
        //
        //     // foreach (var t in d.GetType().GetInterfaces())
        //     // {
        //     //     Debug.Log(t);
        //     // }
        // }
    }
}

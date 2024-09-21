using System;
using System.Collections.Generic;
using SaintsField.Playa;
using SaintsField.SaintsXPathParser;
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

            [ResizableTextArea, ReadOnly, Ordered] public string[] results;

        }

        [SaintsRow] public PathAndResults[] pathAndResults;

        // [InfoBox("::scene-root/Issues*[last()]//[@{GetComponent(EPathDebug).enabled}]", EMessageType.None)]
        // [SaintsPath("::scene-root/Issues*[last()]//[@{GetComponent(EPathDebug).enabled}]")]
        // public EPathDebug myself;

        [SaintsPath("Sub@{GetComponent(Dummy).GetTargetTransform()}")]
        public Transform c;
    }
}

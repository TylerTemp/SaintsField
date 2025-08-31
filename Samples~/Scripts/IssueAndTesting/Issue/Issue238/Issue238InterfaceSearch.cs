using System.Text;
using SaintsField.Playa;
using SaintsField.Samples.Scripts.SaintsEditor;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
#if SAINTSFIELD_AYELLOWWALLPAPER_SERIALIZE_INTERFACE
using AYellowpaper;
#endif

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue238
{
    public class Issue238InterfaceSearch : SaintsMonoBehaviour
    {
#if SAINTSFIELD_AYELLOWWALLPAPER_SERIALIZE_INTERFACE
        public InterfaceReference<IIssue238> yellowIssue238Interface;
#endif

        public SaintsObjInterface<IIssue238> saintsIssue238Interface;

#if UNITY_EDITOR
        [Button]
        private void SearchPrefabInterface()
        {
            HierarchyProperty property = new HierarchyProperty(HierarchyType.Assets, false);

            TypeCache.TypeCollection derivedTypes = TypeCache.GetTypesDerivedFrom(typeof(IIssue238));
            Debug.Log($"found types = {string.Join(" ", derivedTypes)}");
            StringBuilder sb = new StringBuilder();
            Type fieldType = typeof(UnityEngine.Object);
            foreach (Type type in derivedTypes)
            {
                if (fieldType.IsAssignableFrom(type))
                    sb.Append("t:" + type.FullName + " ");
            }
            // this makes sure we don't find anything if there's no type supplied
            if (sb.Length == 0)
                sb.Append("t:");

            string searchFilter = sb.ToString();
            Debug.Log($"search filter: {searchFilter}");
            property.SetSearchFilter(searchFilter, 0);
            while (property.Next(null))
            {
                Debug.Log(property.pptrValue);
            }
        }
#endif

    }
}

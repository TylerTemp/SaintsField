using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SaintsField.Samples.Scripts.Interface
{
    public class InterfaceDecExample : MonoBehaviour
    {
        [Serializable]
        public class Interface1 : SaintsInterface<Component, IInterface1>
        {
        }

        [AboveButton(nameof(MyFuncLog), buttonLabel: nameof(MyFunc), isCallback: true)]
        [AssetPreview]
        [GameObjectActive]
        public Interface1 myInherentInterface1;

        private void MyFuncLog(Interface1 interface1) => Debug.Log(MyFunc(interface1));

        private string MyFunc(Interface1 interface1)
        {
             string myName = interface1.I?.GetType().Name ?? "null";
             return myName;
        }

#if UNITY_EDITOR
        [AdvancedDropdown(nameof(AdvDropdown))]
#endif
        public Interface1 advDropdown;

#if UNITY_EDITOR
        private AdvancedDropdownList<Component> AdvDropdown()
        {
            GameObject g1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SaintsField/Samples/RawResources/PrefabInterface1.prefab");
            GameObject g2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SaintsField/Samples/RawResources/PrefabInterface12.prefab");
            return new AdvancedDropdownList<Component>("")
            {
                {g1.name, g1.GetComponent<MonoInter1>()},
                {g2.name, g2.GetComponent<MonoInter12>()},
            };
        }
#endif

#if UNITY_EDITOR
        [Dropdown(nameof(Dropdown))]
#endif
        public Interface1 dropdown;

#if UNITY_EDITOR
        private DropdownList<Component> Dropdown()
        {
            GameObject g1 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SaintsField/Samples/RawResources/PrefabInterface1.prefab");
            GameObject g2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SaintsField/Samples/RawResources/PrefabInterface12.prefab");
            return new DropdownList<Component>
            {
                {g1.name, g1.GetComponent<MonoInter1>()},
                {g2.name, g2.GetComponent<MonoInter12>()},
            };
        }
#endif

        [SerializeField, ColorToggle(nameof(myInherentInterface1))]
        private Color _onColor;

        [Serializable]
        public class InterfaceSo : SaintsInterface<ScriptableInter12, IInterface1>
        {
        }

        [Expandable]
        public InterfaceSo interfaceSo;

        [GetComponent, PostFieldButton(nameof(DebugInterface1), "D")] public Interface1 getComponent;
        [GetComponentByPath("."), PostFieldButton(nameof(DebugInterface1), "D")] public Interface1 getComponentByPath;

        private void DebugInterface1(Interface1 interface1)
        {
            Debug.Log(interface1.I?.GetType().Name ?? "null");
            Debug.Log(interface1.V);
        }

        [Serializable]
        public class InterfaceAny1 : SaintsInterface<UnityEngine.Object, IInterface1>
        {
        }

        [FindComponent("Sub")] public InterfaceAny1 findComponent;
        [ShowImage]
        [GetComponentInChildren] public InterfaceAny1 getComponentInChildren;
        [GetComponentInParent] public InterfaceAny1 getComponentInParent;
        [GetComponentInParents] public InterfaceAny1 getComponentInParents;
        [GetComponentInScene] public InterfaceAny1 getComponentInScene;
        [GetPrefabWithComponent] public InterfaceAny1 getPrefabWithComponent;
        [GetScriptableObject] public InterfaceAny1 getScriptableObject;

        [SpriteToggle(nameof(getComponentInChildren))] public Sprite sprite;
    }
}

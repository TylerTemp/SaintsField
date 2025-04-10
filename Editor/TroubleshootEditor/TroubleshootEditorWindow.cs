using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using PropertyAttribute = UnityEngine.PropertyAttribute;

namespace SaintsField.Editor.TroubleshootEditor
{
    public class TroubleshootEditorWindow: SaintsEditorWindow
    {
#if SAINTSFIELD_DEBUG
        [MenuItem("Saints/Troubleshoot")]
#else
        [MenuItem("Window/Saints/Troubleshoot")]
#endif
        public static void TestOpenWindow()
        {
            EditorWindow window = GetWindow<TroubleshootEditorWindow>(false, "SaintsField Troubleshoot");
            window.Show();
        }

        private bool _inProgress;

        private string ProgressLabel() => _inProgress ? "Checking..." : "Done";

        [DebugTool.WhichFramework]
        [Ordered, RichLabel("$" + nameof(ProgressLabel)), ReadOnly, ProgressBar(maxCallback: nameof(_maxCount))]
        public int progress;

        [Serializable]
        private struct PropertyTypeToDrawer
        {
            public string propertyType;
            public string drawerType;
        }

        [Ordered, ListDrawerSettings(searchable: true, numberOfItemsPerPage: 20, delayedSearch: true)]
        [SerializeField]
        private List<PropertyTypeToDrawer> _propertyTypeToDrawers = new List<PropertyTypeToDrawer>();

        private int _maxCount = 1;

        private (EMessageType, string) _uObjectMessage = (EMessageType.Warning, null);
        private (EMessageType, string) _uComp = (EMessageType.Warning, null);
        private (EMessageType, string) _uMono = (EMessageType.Warning, null);
        private (EMessageType, string) _uScriptableObject = (EMessageType.Warning, null);

        // [Button("Check")]
        [Ordered]
        [PlayaInfoBox("$" + nameof(_uObjectMessage))]
        [PlayaInfoBox("$" + nameof(_uComp))]
        [PlayaInfoBox("$" + nameof(_uMono))]
        [PlayaInfoBox("$" + nameof(_uScriptableObject))]
        private IEnumerator Check()
        {
            _inProgress = true;
            int total = 0;
            progress = 0;
            _maxCount = 1;
            bool uObjectFound = false;
            bool uCompFound = false;
            bool uMonoFound = false;
            bool uScriptableObjectFound = false;

            _propertyTypeToDrawers.Clear();

            foreach (Assembly asb in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] allTypes = asb.GetTypes();
                List<Type> allEditors = allTypes
                    .Where(type => type.IsSubclassOf(typeof(UnityEditor.Editor)))
                    .ToList();
                total += allEditors.Count;
                if(total > 0)
                {
                    _maxCount = total;
                }
                yield return null;

                foreach (Type eachEditorType in allEditors)
                {
                    progress++;
                    // if(progress % 50 == 0)
                    // {
                    //     yield return null;
                    // }
                    foreach (CustomEditor customEditor in ReflectCache.GetCustomAttributes<CustomEditor>(eachEditorType, true))
                    {
                        yield return null;
                        Type v = (Type)typeof(CustomEditor)
                            .GetField("m_InspectedType",
                                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy)
                            ?.GetValue(customEditor);
                        // Debug.Log($"Found editor: {eachEditorType} -> {v}");
                        if(v != null)
                        {
                            _propertyTypeToDrawers.Add(new PropertyTypeToDrawer
                            {
                                propertyType = $"{v.Name} ({v.Assembly.GetName().Name})",
                                drawerType = $"{eachEditorType.Name} ({eachEditorType.Assembly.GetName().Name})",
                            });
                        }
                        // if()
                        if (v == typeof(Object))
                        {
                            uObjectFound = true;
                            _uObjectMessage = TypeIsSaintsEditor(eachEditorType)
                                ? (EMessageType.None, "Your UnityEditor.Object is drawn by Saints Editor.")
                                : (EMessageType.Warning, $"Your UnityEditor.Object is drawn by {eachEditorType}, some attributes might not work.");
                            // EditorRefreshTarget();
                        }

                        if (v == typeof(Component))
                        {
                            uCompFound = true;
                            _uComp = TypeIsSaintsEditor(eachEditorType)
                                ? (EMessageType.None, "Your Component is drawn by Saints Editor.")
                                : (EMessageType.Warning, $"Your Component is drawn by {eachEditorType}, some attributes might not work.");
                            // EditorRefreshTarget();
                        }

                        if (v == typeof(MonoBehaviour))
                        {
                            uMonoFound = true;
                            _uMono = TypeIsSaintsEditor(eachEditorType)
                                ? (EMessageType.None, "Your MonoBehaviour is drawn by Saints Editor.")
                                : (EMessageType.Warning, $"Your MonoBehaviour is drawn by {eachEditorType}, some attributes might not work.");
                            // EditorRefreshTarget();
                        }

                        if (v == typeof(ScriptableObject))
                        {
                            uScriptableObjectFound = true;
                            _uScriptableObject = TypeIsSaintsEditor(eachEditorType)
                                ? (EMessageType.None, "Your ScriptableObject is drawn by Saints Editor.")
                                : (EMessageType.Warning, $"Your ScriptableObject is drawn by {eachEditorType}, some attributes might not work.");
                            // EditorRefreshTarget();
                        }
                    }
                }

            }

            _inProgress = false;


            if(!uObjectFound && !uCompFound)
            {
                _uComp = (EMessageType.Warning, "Your Component is drawn by Unity default inspector, some attributes might not work.");
            }
            else if(!uObjectFound && !uMonoFound)
            {
                _uMono = (EMessageType.Warning, "Your MonoBehaviour is drawn by Unity default inspector, some attributes might not work.");
            }
            else if (!uObjectFound)
            {
                _uObjectMessage = (EMessageType.Warning, "Your UnityEditor.Object is drawn by Unity default inspector, some attributes might not work.");
            }

            if(!uObjectFound && !uScriptableObjectFound)
            {
                _uScriptableObject = (EMessageType.Warning, "Your ScriptableObject is drawn by Unity default inspector, some attributes might not work.");
            }

            EditorRefreshTarget();
        }

        private static bool TypeIsSaintsEditor(Type editorType) =>
            editorType.IsSubclassOf(typeof(SaintsEditor)) || editorType == typeof(SaintsEditor);

        private bool NotInProcess => !_inProgress;

        [Ordered, Separator(5), Separator, Separator(5),

         InfoBox("Please wait the checking process to finish", show: nameof(NotInProcess)),

         Required("Pick a target to troubleshoot"),
         OnValueChanged(nameof(TroubleShootTargetChanged)),
         BelowInfoBox("$" + nameof(GetDrawerInfo)),
        ]
        public Object troubleshootTarget;

        // private (EMessageType, string) _troubleshootTargetEditorMessage = default;

        private void TroubleShootTargetChanged()
        {
            troubleshootComponent = null;
            field = -1;
            _targetMessage = default;
            _fieldMessage = default;


        }

        private static (EMessageType, string) GetDrawerInfo(Object target)
        {
            if (target == null)
            {
                return default;
            }

            if(target is GameObject)
            {
                return (EMessageType.Info, "GameObject is selected, pick a component to troubleshoot.");
            }

            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(target);
            Type editorType = editor.GetType();
            DestroyImmediate(editor);
            return TypeIsSaintsEditor(editorType)
                ? (EMessageType.None, "Your target is drawn by Saints Editor. Nice.")
                : (EMessageType.Warning, $"Your target is drawn by {editorType}, some attributes might not work.");
        }

        [Ordered,
         ShowIf(nameof(NeedPickComponent)),
         AdvancedDropdown(nameof(PickComponent)),
         OnValueChanged(nameof(TroubleshootComponentChanged)),
         BelowInfoBox("$" + nameof(GetDrawerInfo)),
        ]
        public Component troubleshootComponent;

        private AdvancedDropdownList<Component> PickComponent()
        {
            AdvancedDropdownList<Component> result = new AdvancedDropdownList<Component>();
            if (troubleshootTarget == null)
            {
                return result;
            }

            if (troubleshootTarget is GameObject go)
            {
                foreach (Component component in go.GetComponents<Component>())
                {
                    // Debug.Log(component);
                    result.Add(component.GetType().Name, component);
                }
            }
            return result;
        }

        private bool NeedPickComponent() => PickComponent().Any();

        private void TroubleshootComponentChanged() => field = -1;

        [Ordered,
         AdvancedDropdown(nameof(PickFieldName)),
         ShowIf(nameof(GetTroubleshootSerTarget)), DisableIf(nameof(_inProgress)),
         OnValueChanged(nameof(RunTargetChecker)),
         BelowInfoBox("$" + nameof(_targetMessage)), BelowInfoBox("$" + nameof(_fieldMessage))]
        public int field;

        private Object GetTroubleshootSerTarget()
        {
            if (troubleshootTarget == null)
            {
                return null;
            }

            Object target = troubleshootComponent == null ? troubleshootTarget : troubleshootComponent;
            return target == null ? null : target;
        }

        private readonly List<MemberInfo> _pickFieldMemberInfos = new List<MemberInfo>();

        private AdvancedDropdownList<int> PickFieldName()
        {
            _pickFieldMemberInfos.Clear();

            AdvancedDropdownList<int> result = new AdvancedDropdownList<int>();
            Object inspectTarget = GetTroubleshootSerTarget();
            if (inspectTarget == null)
            {
                return result;
            }

            Dictionary<string, SerializedProperty> serializedPropertyDict;
            using (SerializedObject serializedObject = new SerializedObject(inspectTarget))
            {
                string[] serializableFields = SaintsEditor.GetSerializedProperties(serializedObject).ToArray();
                // Debug.Log($"serializableFields={string.Join(",", serializableFields)}");
                serializedPropertyDict = serializableFields
                    .ToDictionary(each => each, serializedObject.FindProperty);
            }

            foreach (SaintsFieldWithInfo saintsFieldWithInfo in SaintsEditor.HelperGetSaintsFieldWithInfo(serializedPropertyDict, inspectTarget))
            {
                // Debug.Log(saintsFieldWithInfo.RenderType);
                if (saintsFieldWithInfo.RenderType == SaintsRenderType.SerializedField)
                {
                    // ReSharper disable once InvertIf
                    if(saintsFieldWithInfo.FieldInfo != null)
                    {
                        result.Add(saintsFieldWithInfo.FieldInfo.Name, _pickFieldMemberInfos.Count);
                        _pickFieldMemberInfos.Add(saintsFieldWithInfo.FieldInfo);
                    }
                }
                else
                {
                    if (saintsFieldWithInfo.PlayaAttributes.Count > 0)
                    {
                        switch (saintsFieldWithInfo.RenderType)
                        {
                            case SaintsRenderType.Method:
                                result.Add($"[Method] {saintsFieldWithInfo.MethodInfo.Name}", _pickFieldMemberInfos.Count);
                                _pickFieldMemberInfos.Add(saintsFieldWithInfo.MethodInfo);
                                break;
                            case SaintsRenderType.NativeProperty:
                                result.Add($"[Property] {saintsFieldWithInfo.PropertyInfo.Name}", _pickFieldMemberInfos.Count);
                                _pickFieldMemberInfos.Add(saintsFieldWithInfo.PropertyInfo);
                                break;
                            case SaintsRenderType.NonSerializedField:
                                result.Add($"[NonSerialized] {saintsFieldWithInfo.FieldInfo.Name}", _pickFieldMemberInfos.Count);
                                _pickFieldMemberInfos.Add(saintsFieldWithInfo.FieldInfo);
                                break;
                            case SaintsRenderType.SerializedField:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(saintsFieldWithInfo.RenderType), saintsFieldWithInfo.RenderType, null);
                        }
                    }
                }
            }

            return result;
        }

#pragma warning disable CS0414
        private (EMessageType, string) _targetMessage = (EMessageType.Warning, null);
        private (EMessageType, string) _fieldMessage = (EMessageType.Warning, null);
#pragma warning restore CS0414

        // [Ordered, ]
        private void RunTargetChecker(int index)
        {
            Object inspectTarget = GetTroubleshootSerTarget();
            if (inspectTarget == null)
            {
                return;
            }

            _targetMessage = default;
            _fieldMessage = default;

            if(index < 0 || index >= _pickFieldMemberInfos.Count)
            {
                return;
            }

            MemberInfo memberInfo = _pickFieldMemberInfos[index];

            UnityEditor.Editor inspectEditor = UnityEditor.Editor.CreateEditor(inspectTarget);
            bool isSaintsEditor = TypeIsSaintsEditor(inspectEditor.GetType());
            // _targetMessage = isSaintsEditor
            //     ? (EMessageType.None, $"Your target {inspectTarget} is drawn by Saints Editor. Nice.")
            //     : (EMessageType.Warning, $"Your target {inspectTarget} is drawn by {inspectEditor.GetType()}, some attributes might not work.");
            // Debug.Log(_targetMessage);

            string error = "";
            List<Attribute> playaAttributes = new List<Attribute>();

            Attribute[] allBaseAttributes = ReflectCache.GetCustomAttributes(memberInfo);
            if (!isSaintsEditor)
            {
                playaAttributes.AddRange(allBaseAttributes.Where(each => each is IPlayaAttribute));
            }

            bool hitSaintsAlready = false;
            foreach (PropertyAttribute propertyAttribute in allBaseAttributes.OfType<PropertyAttribute>())
            {
                bool isSaintsProperty = propertyAttribute is ISaintsAttribute;

                // Debug.Log($"{propertyAttribute}: {propertyAttribute is ISaintsAttribute}");
                if (SaintsPropertyDrawer.PropertyGetDecoratorDrawer(propertyAttribute.GetType()) != null)
                {
                    continue;
                }
                if (isSaintsProperty)
                {
                    hitSaintsAlready = true;
                    continue;
                }

                if (!hitSaintsAlready)
                {
                    error = $"Attribute {propertyAttribute} is before any SaintsField attribute, which might block the fallback process.";
                }
            }

            if (playaAttributes.Count > 0)
            {
                _targetMessage =
                    (EMessageType.Warning, $"Attribute(s) might need Saints Editor to work: {string.Join(", ", playaAttributes)}");
            }

            if (!string.IsNullOrEmpty(error))
            {
                _fieldMessage = (EMessageType.Error, error);
            }
            else
            {
                _fieldMessage = (EMessageType.None, $"No issue found for {memberInfo.Name}");
            }

            // EditorRefreshTarget();
        }

        public override void OnEditorEnable()
        {
            _targetMessage = default;
            _fieldMessage = default;
            field = -1;
            SaintsPropertyDrawer.EnsureAndGetTypeToDrawers();
            StartEditorCoroutine(Check());
        }
    }
}

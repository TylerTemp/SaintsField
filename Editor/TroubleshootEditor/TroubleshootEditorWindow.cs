using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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

        [Ordered, RichLabel("Checking..."), ShowIf(nameof(_inProgress)), ReadOnly, ProgressBar(maxCallback: nameof(_maxCount))]
        public int progress;

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

            Dictionary<Type, List<Type>> attrToDecoratorDrawers =
                new Dictionary<Type, List<Type>>();

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
                    foreach (CustomEditor customEditor in eachEditorType.GetCustomAttributes<CustomEditor>(true))
                    {
                        yield return null;
                        Type v = (Type)typeof(CustomEditor)
                            .GetField("m_InspectedType",
                                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy)
                            ?.GetValue(customEditor);
                        Debug.Log($"Found editor: {eachEditorType} -> {v}");
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

            if (!uObjectFound && (!uCompFound || !uMonoFound))
            {
                _uObjectMessage = (EMessageType.Warning, "Your UnityEditor.Object is drawn by Unity default inspector, some attributes might not work.");
            }
            else if(!uCompFound)
            {
                _uComp = (EMessageType.Warning, "Your Component is drawn by Unity default inspector, some attributes might not work.");
            }
            else if(!uMonoFound)
            {
                _uMono = (EMessageType.Warning, "Your MonoBehaviour is drawn by Unity default inspector, some attributes might not work.");
            }

            if(!uObjectFound && !uScriptableObjectFound)
            {
                _uScriptableObject = (EMessageType.Warning, "Your ScriptableObject is drawn by Unity default inspector, some attributes might not work.");
            }

            EditorRefreshTarget();
        }

        private static bool TypeIsSaintsEditor(Type editorType) =>
            editorType.IsSubclassOf(typeof(SaintsEditor)) || editorType == typeof(SaintsEditor);

        [Ordered, Separator(5), Separator, Separator(5), Required("Pick a target to troubleshoot"), OnValueChanged(nameof(TroubleShootTargetChanged))]
        public Object troubleshootTarget;

        private void TroubleShootTargetChanged()
        {
            troubleshootComponent = null;
            fieldName = null;
            _targetMessage = default;
            _fieldMessage = default;
        }

        [Ordered, ShowIf(nameof(NeedPickComponent)), AdvancedDropdown(nameof(PickComponent)), OnValueChanged(nameof(TroubleshootComponentChanged))]
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

        private void TroubleshootComponentChanged() => fieldName = null;

        [Ordered, AdvancedDropdown(nameof(PickFieldName)), ShowIf(nameof(GetTroubleshootSerTarget))]
        public string fieldName;

        private Object GetTroubleshootSerTarget()
        {
            if (troubleshootTarget == null)
            {
                return null;
            }

            Object target = troubleshootComponent == null ? troubleshootTarget : troubleshootComponent;
            return target == null ? null : target;
        }

        private AdvancedDropdownList<string> PickFieldName()
        {
            AdvancedDropdownList<string> result = new AdvancedDropdownList<string>();
            Object inspectTarget = GetTroubleshootSerTarget();
            if (inspectTarget == null)
            {
                return result;
            }

            // ReSharper disable once ConvertToUsingDeclaration
            using (SerializedObject so = new SerializedObject(inspectTarget))
            {
                foreach (SerializedProperty serializedProperty in SerializedUtils.GetAllField(so).Where(each => each != null))
                {
                    result.Add(serializedProperty.displayName, serializedProperty.name);
                }
            }

            return result;
        }

        private (EMessageType, string) _targetMessage = (EMessageType.Warning, null);
        private (EMessageType, string) _fieldMessage = (EMessageType.Warning, null);


        [Ordered, PlayaDisableIf(nameof(_inProgress)), Button("Check"), PlayaInfoBox("$" + nameof(_targetMessage)), PlayaInfoBox("$" + nameof(_fieldMessage))]
        private void RunTargetChecker()
        {
            Object inspectTarget = GetTroubleshootSerTarget();
            if (inspectTarget == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(fieldName))
            {
                return;
            }

            UnityEditor.Editor inspectEditor = UnityEditor.Editor.CreateEditor(inspectTarget);
            bool isSaintsEditor = TypeIsSaintsEditor(inspectEditor.GetType());
            // _targetMessage = isSaintsEditor
            //     ? (EMessageType.None, $"Your target {inspectTarget} is drawn by Saints Editor. Nice.")
            //     : (EMessageType.Warning, $"Your target {inspectTarget} is drawn by {inspectEditor.GetType()}, some attributes might not work.");
            // Debug.Log(_targetMessage);

            using (SerializedObject so = new SerializedObject(inspectTarget))
            {
                SerializedProperty prop = so.FindProperty(fieldName) ?? SerializedUtils.FindPropertyByAutoPropertyName(so, fieldName);
                if (prop == null)
                {
                    _fieldMessage = (EMessageType.Error, $"Field {fieldName} not found.");
                }
                else
                {
                    string error = "";
                    List<Attribute> playaAttributes = new List<Attribute>();

                    (Attribute[] allBaseAttributes, object _) = SerializedUtils.GetAttributesAndDirectParent<Attribute>(prop);
                    if (!isSaintsEditor)
                    {
                        playaAttributes.AddRange(allBaseAttributes.Where(each => each is IPlayaAttribute));
                    }

                    bool hitSaintsAlready = false;
                    foreach (PropertyAttribute propertyAttribute in allBaseAttributes.OfType<PropertyAttribute>())
                    {
                        bool isSaintsProperty = propertyAttribute is ISaintsAttribute;

                        // Debug.Log($"{propertyAttribute}: {propertyAttribute is ISaintsAttribute}");
                        if (SaintsPropertyDrawer.PropertyIsDecoratorDrawer(propertyAttribute))
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
                        _fieldMessage = (EMessageType.None, $"No issue found for field {fieldName}");
                    }

                }
            }

            EditorRefreshTarget();
        }

        public override void OnEditorEnable()
        {
            _targetMessage = default;
            _fieldMessage = default;
            SaintsPropertyDrawer.EnsureAndGetTypeToDrawers();
            StartEditorCoroutine(Check());
        }
    }
}

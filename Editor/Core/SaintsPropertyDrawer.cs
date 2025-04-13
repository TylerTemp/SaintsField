using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Playa;
using SaintsField.Editor.Playa.Renderer.BaseRenderer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Core
{
    // above
    // pre, label, field, post
    // below-
    public partial class SaintsPropertyDrawer: PropertyDrawer, IMakeRenderer, IDisposable
    {
        public bool InHorizontalLayout;

        protected const int LabelLeftSpace = 4;
        protected const int LabelBaseWidth = 120;
        public const int IndentWidth = 15;
        public const float SingleLineHeight = 20f;
        // public const string EmptyRectLabel = "                ";

        // public static bool IsSubDrawer = false;
        private static readonly Dictionary<InsideSaintsFieldScoop.PropertyKey, int> SubDrawCounter = new Dictionary<InsideSaintsFieldScoop.PropertyKey, int>();
        private static readonly Dictionary<InsideSaintsFieldScoop.PropertyKey, int> SubGetHeightCounter = new Dictionary<InsideSaintsFieldScoop.PropertyKey, int>();

        public struct PropertyDrawerInfo
        {
            public bool IsSaints;
            public Type DrawerType;
            public bool UseForChildren;
        }

        protected static readonly Dictionary<Type, IReadOnlyList<PropertyDrawerInfo>> PropertyAttributeToPropertyDrawers =
            new Dictionary<Type, IReadOnlyList<PropertyDrawerInfo>>();
// #if UNITY_2022_1_OR_NEWER && SAINTSFIELD_IMGUI_DUPLICATE_DECORATOR_FIX
        private static IReadOnlyDictionary<Type, IReadOnlyList<PropertyDrawerInfo>> _propertyAttributeToDecoratorDrawers =
            new Dictionary<Type, IReadOnlyList<PropertyDrawerInfo>>();
// #endif

        // [MenuItem("Saints/Debug")]
        // private static void SaintsDebug() => PropertyAttributeToPropertyDrawers.Clear();

        // private class SharedInfo
        // {
        //     public bool Changed;
        //     // public object ParentTarget;
        // }

        // private static readonly Dictionary<string, SharedInfo> PropertyPathToShared = new Dictionary<string, SharedInfo>();

        // private IReadOnlyList<ISaintsAttribute> _allSaintsAttributes;
        // private SaintsPropertyDrawer _labelDrawer;
        // private SaintsPropertyDrawer _fieldDrawer;

        // ReSharper disable once InconsistentNaming
        protected readonly string FieldControlName;

        private static double _sceneViewNotificationTime;
        private static bool _sceneViewNotificationListened;
        private static readonly HashSet<string> SceneViewNotifications = new HashSet<string>();

        private readonly struct SaintsWithIndex : IEquatable<SaintsWithIndex>
        {
            public readonly ISaintsAttribute SaintsAttribute;
            public readonly int Index;

            public SaintsWithIndex(ISaintsAttribute saintsAttribute, int index)
            {
                SaintsAttribute = saintsAttribute;
                Index = index;
            }

            public bool Equals(SaintsWithIndex other)
            {
                return Equals(SaintsAttribute, other.SaintsAttribute) && Index == other.Index;
            }

            public override bool Equals(object obj)
            {
                return obj is SaintsWithIndex other && Equals(other);
            }

            public override int GetHashCode()
            {
                // return HashCode.Combine(SaintsAttribute, Index);
                return Util.CombineHashCode(SaintsAttribute, Index);
            }
        }

        private readonly Dictionary<SaintsWithIndex, SaintsPropertyDrawer> _cachedDrawer = new Dictionary<SaintsWithIndex, SaintsPropertyDrawer>();
        // private readonly Dictionary<Type, PropertyDrawer> _cachedOtherDrawer = new Dictionary<Type, PropertyDrawer>();
        // private readonly HashSet<Type> _usedDrawerTypes = new HashSet<Type>();
        // private readonly Dictionary<ISaintsAttribute, >
        // private struct UsedAttributeInfo
        // {
        //     public Type DrawerType;
        //     public ISaintsAttribute Attribute;
        // }

        // private readonly List<UsedAttributeInfo> _usedAttributes = new List<UsedAttributeInfo>();
        // private readonly Dictionary<SaintsWithIndex, SaintsPropertyDrawer> _usedAttributes = new Dictionary<SaintsWithIndex, SaintsPropertyDrawer>();

        // private static readonly FieldDrawerConfigAttribute DefaultFieldDrawerConfigAttribute =
        //     new FieldDrawerConfigAttribute(FieldDrawerConfigAttribute.FieldDrawType.Inline, 0);

        // private string _cachedPropPath;
#if UNITY_2022_1_OR_NEWER
        private static Assembly _unityEditorAssemble;
#endif

        // ReSharper disable once PublicConstructorInAbstractClass
        public SaintsPropertyDrawer()
        {
            // Selection.selectionChanged += OnSelectionChanged;
            // Debug.Log($"OnSaintsCreate Start: {SepTitleAttributeDrawer.drawCounter}/{fieldInfo}");
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"new SaintsPropertyDrawer {this}");
#endif
            // if (IsSubDrawer)
            // {
            //     return;
            // }

            FieldControlName = Guid.NewGuid().ToString();

            // _usedAttributes.Clear();

            // _propertyAttributeToDrawers.Clear();

            EnsureAndGetTypeToDrawers();
        }

        private static void OnSceneViewNotification(SceneView sv)
        {
            if (SceneViewNotifications.Count == 0)
            {
                _sceneViewNotificationTime = EditorApplication.timeSinceStartup;
                return;
            }

            if(EditorApplication.timeSinceStartup - _sceneViewNotificationTime < 0.5f)
            {
                return;
            }

            _sceneViewNotificationTime = EditorApplication.timeSinceStartup;
            sv.ShowNotification(new GUIContent(string.Join("\n", SceneViewNotifications)));
            SceneViewNotifications.Clear();
            SceneView.RepaintAll();
        }

        public static void EnqueueSceneViewNotification(string message)
        {
            if (!_sceneViewNotificationListened)
            {
                _sceneViewNotificationListened = true;
                _sceneViewNotificationTime = EditorApplication.timeSinceStartup;
                SceneView.duringSceneGui += OnSceneViewNotification;
            }

            SceneViewNotifications.Add(message);
        }

        public static (IReadOnlyDictionary<Type, IReadOnlyList<PropertyDrawerInfo>> attrToPropertyDrawers, IReadOnlyDictionary<Type, IReadOnlyList<PropertyDrawerInfo>> attrToDecoratorDrawers) EnsureAndGetTypeToDrawers()
        {
            if (PropertyAttributeToPropertyDrawers.Count != 0)
            {
                return (PropertyAttributeToPropertyDrawers, _propertyAttributeToDecoratorDrawers);
            }

            Dictionary<Type, HashSet<Type>> attrToDrawers = new Dictionary<Type, HashSet<Type>>();
            Dictionary<Type, HashSet<Type>> attrToDecoratorDrawers =
                new Dictionary<Type, HashSet<Type>>();

            foreach (Assembly asb in AppDomain.CurrentDomain.GetAssemblies())
            {
#if UNITY_2022_1_OR_NEWER
                if (asb.GetName().Name == "UnityEditor")
                {
                    _unityEditorAssemble = asb;
                }
#endif

                Type[] allTypes = asb.GetTypes();

                // foreach (Type editorType in allTypes.Where(type => type.IsSubclassOf(typeof(UnityEditor.Editor))))
                // {
                //     foreach (CustomEditor editorTargetType in editorType.GetCustomAttributes<CustomEditor>(true))
                //     {
                //         Debug.Log($"{editorTargetType} -> {editorType}");
                //     }
                // }

                List<Type> allSubPropertyDrawers = allTypes
                    // .Where(type => type.IsSubclassOf(typeof(SaintsPropertyDrawer)))
                    .Where(type => type.IsSubclassOf(typeof(PropertyDrawer)))
                    .Where(type => !type.IsAbstract)
                    .ToList();

                foreach (Type eachPropertyDrawer in allSubPropertyDrawers)
                {
                    foreach (Type attr in ReflectCache.GetCustomAttributes<CustomPropertyDrawer>(eachPropertyDrawer, true)
                                 .Select(instance => typeof(CustomPropertyDrawer)
                                     .GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance)
                                     ?.GetValue(instance))
                                 .Where(each => each != null))
                    {
                        if (!attrToDrawers.TryGetValue(attr, out HashSet<Type> attrList))
                        {
                            attrToDrawers[attr] = attrList = new HashSet<Type>();
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CORE_DRAWER_INIT
                            Debug.Log($"Found drawer: {attr} -> {eachPropertyDrawer}");
#endif

                        attrList.Add(eachPropertyDrawer);
                    }
                }

                List<Type> allSubDecoratorDrawers = allTypes
                    .Where(type => type.IsSubclassOf(typeof(DecoratorDrawer)))
                    .Where(type => !type.IsAbstract)
                    .ToList();

                foreach (Type eachDecoratorDrawer in allSubDecoratorDrawers)
                {
                    foreach (Type attr in ReflectCache.GetCustomAttributes<CustomPropertyDrawer>(eachDecoratorDrawer, true)
                                 .Select(instance => typeof(CustomPropertyDrawer)
                                     .GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance)
                                     ?.GetValue(instance))
                                 .Where(each => each != null))
                    {
                        if (!attrToDecoratorDrawers.TryGetValue(attr, out HashSet<Type> attrList))
                        {
                            attrToDecoratorDrawers[attr] = attrList = new HashSet<Type>();
                        }

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_CORE_DRAWER_INIT
                            Debug.Log($"Found dec drawer: {attr} -> {eachDecoratorDrawer}");
#endif

                        attrList.Add(eachDecoratorDrawer);
                    }
                }

            }

            foreach (KeyValuePair<Type, HashSet<Type>> kv in attrToDrawers)
            {
                PropertyAttributeToPropertyDrawers[kv.Key] = kv.Value
                    .Select(each => new PropertyDrawerInfo
                        {
                            IsSaints = each.IsSubclassOf(typeof(SaintsPropertyDrawer)),
                            DrawerType = each,
                            UseForChildren = ReflectCache.GetCustomAttributes<CustomPropertyDrawer>(each, true)
                                .Any(instance => typeof(CustomPropertyDrawer)
                                    .GetField("m_UseForChildren", BindingFlags.NonPublic | BindingFlags.Instance)
                                    // ReSharper disable once MergeIntoPattern
                                    ?.GetValue(instance) is bool useForChildren && useForChildren)
                        })
                    .ToArray();
// #if EXT_INSPECTOR_LOG
//                     Debug.Log($"attr {kv.Key} has drawer(s) {string.Join(",", kv.Value)}");
// #endif
            }

// #if UNITY_2022_1_OR_NEWER && SAINTSFIELD_IMGUI_DUPLICATE_DECORATOR_FIX

            Dictionary<Type, IReadOnlyList<PropertyDrawerInfo>> propertyAttributeToDecoratorDrawers =
                new Dictionary<Type, IReadOnlyList<PropertyDrawerInfo>>();
            foreach (KeyValuePair<Type, HashSet<Type>> kv in attrToDecoratorDrawers)
            {
                propertyAttributeToDecoratorDrawers[kv.Key] = kv.Value
                    .Select(each => new PropertyDrawerInfo
                    {
                        IsSaints = false,
                        DrawerType = each,
                        UseForChildren = ReflectCache.GetCustomAttributes<CustomPropertyDrawer>(each, true)
                            .Any(instance => typeof(CustomPropertyDrawer)
                                .GetField("m_UseForChildren", BindingFlags.NonPublic | BindingFlags.Instance)
                                // ReSharper disable once MergeIntoPattern
                                ?.GetValue(instance) is bool useForChildren && useForChildren)
                    })
                    .ToArray();
// #if EXT_INSPECTOR_LOG
//                     Debug.Log($"attr {kv.Key} has drawer(s) {string.Join(",", kv.Value)}");
// #endif
            }

            _propertyAttributeToDecoratorDrawers = propertyAttributeToDecoratorDrawers;
            // _propertyAttributeToDecoratorDrawers = attrToDecoratorDrawers.ToDictionary(each => each.Key, each => (IReadOnlyList<Type>)each.Value);
// #endif

            return (PropertyAttributeToPropertyDrawers, _propertyAttributeToDecoratorDrawers);
        }

        public static Type PropertyGetDecoratorDrawer(Type attributeType)
        {
            EnsureAndGetTypeToDrawers();

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_propertyAttributeToDecoratorDrawers.TryGetValue(attributeType, out IReadOnlyList<PropertyDrawerInfo> info))
            {
                // Debug.Log(propertyAttribute.GetType());
                // foreach (Type key in PropertyAttributeToPropertyDrawers.Keys)
                // {
                //     if ($"{key}".Contains("SepTitle"))
                //     {
                //         Debug.Log(key);
                //     }
                // }
                // not found
                return info[0].DrawerType;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (KeyValuePair<Type, IReadOnlyList<PropertyDrawerInfo>> kv in _propertyAttributeToDecoratorDrawers)
            {
                if (!kv.Key.IsAssignableFrom(attributeType))
                {
                    continue;
                }
                return kv.Value.FirstOrDefault(each => each.UseForChildren).DrawerType;
            }

            return null;
        }

        // TODO: check useForChildren
        // public static bool PropertyIsDecoratorDrawer(PropertyAttribute propertyAttribute)
        // {
        //     // ReSharper disable once ConvertIfStatementToReturnStatement
        //     if (!_propertyAttributeToDecoratorDrawers.ContainsKey(propertyAttribute.GetType()))
        //     {
        //         // Debug.Log(propertyAttribute.GetType());
        //         // foreach (Type key in PropertyAttributeToPropertyDrawers.Keys)
        //         // {
        //         //     if ($"{key}".Contains("SepTitle"))
        //         //     {
        //         //         Debug.Log(key);
        //         //     }
        //         // }
        //         // not found
        //         return false;
        //     }
        //
        //     // return eachDrawer.Any(drawerType => drawerType.DrawerType.IsSubclassOf(typeof(DecoratorDrawer)));
        //     return true;
        // }
        // ~SaintsPropertyDrawer()
        // {
        //     Debug.Log($"[{this}] Stop listening changed");
        //     Selection.selectionChanged -= OnSelectionChanged;
        // }
        //
        // private void OnSelectionChanged()
        // {
        //     Debug.Log($"selection changed: {string.Join(", ", Selection.objects.Select(each => each.ToString()))}");
        // }

        // ~SaintsPropertyDrawer()
        // {
        //     PropertyAttributeToDrawers.Clear();
        // }


        private float _labelFieldBasicHeight = EditorGUIUtility.singleLineHeight;

        protected virtual bool GetThisDecoratorVisibility(ShowIfAttribute targetAttribute, SerializedProperty property, FieldInfo info, object target)
        {
            return true;
        }


        public struct SaintsPropertyInfo
        {
            // ReSharper disable InconsistentNaming
            public SaintsPropertyDrawer Drawer;
            public ISaintsAttribute Attribute;
            public int Index;
            // ReSharper enable InconsistentNaming
        }

        private static (Attribute attributeInstance, Type attributeDrawerType) GetOtherAttributeDrawerType(MemberInfo fieldInfo)
        {
            // ReSharper disable once UseNegatedPatternInIsExpression
            foreach (Attribute fieldAttribute in ReflectCache.GetCustomAttributes(fieldInfo))
            {
                if (fieldAttribute is ISaintsAttribute)
                    continue;

                foreach (KeyValuePair<Type, IReadOnlyList<PropertyDrawerInfo>> kv in PropertyAttributeToPropertyDrawers)
                {
                    Type canDrawType = kv.Key;
                    // Debug.Log($"{fieldAttribute}:{kv.Key}:--");
                    // foreach ((bool isSaints, Type drawerType) in kv.Value.Where(each => !each.isSaints))
                    foreach (PropertyDrawerInfo info in kv.Value)
                    {
                        if (info.IsSaints) continue;
                        // if ($"{drawerType}" == "UnityEditor.RangeDrawer")
                        // {
                        //     Debug.Log($"{canDrawType}:{fieldAttribute}={drawerType} -- {canDrawType.IsInstanceOfType(fieldAttribute)}");
                        // }
                        if (canDrawType.IsInstanceOfType(fieldAttribute))
                        {
                            return (fieldAttribute, info.DrawerType);
                        }
                        // Debug.Log($"--{drawerType}:{drawerType.IsInstanceOfType(fieldAttribute)}");
                    }
                }
            }

            return default;

            // attributes can not be generic, so just check with the dictionary
            // return fieldInfo.GetCustomAttributes()
            //     // ReSharper disable once UseNegatedPatternInIsExpression
            //     .Where(each => !(each is ISaintsAttribute))
            //     .Any(fieldAttribute =>
            //     {
            //         // foreach (KeyValuePair<Type,IReadOnlyList<(bool isSaints, Type drawerType)>> kv in PropertyAttributeToPropertyDrawers)
            //         // {
            //         //     var canDrawType = kv.Key;
            //         //     // Debug.Log($"{fieldAttribute}:{kv.Key}:--");
            //         //     foreach ((bool isSaints, Type drawerType) in kv.Value.Where(each => !each.isSaints))
            //         //     {
            //         //         // if ($"{drawerType}" == "UnityEditor.RangeDrawer")
            //         //         // {
            //         //         //     Debug.Log($"{canDrawType}:{fieldAttribute}={drawerType} -- {canDrawType.IsInstanceOfType(fieldAttribute)}");
            //         //         // }
            //         //         if (canDrawType.IsInstanceOfType(fieldAttribute))
            //         //         {
            //         //             return true;
            //         //         }
            //         //         // Debug.Log($"--{drawerType}:{drawerType.IsInstanceOfType(fieldAttribute)}");
            //         //     }
            //         // }
            //         //
            //         // return false;
            //
            //         bool r = PropertyAttributeToPropertyDrawers.Keys.Any(checkType =>
            //             checkType.IsInstanceOfType(fieldAttribute));
            //         if (r)
            //         {
            //             Debug.Log(r);
            //         }
            //         return r;
            //     });
        }

        private static Type FindTypeDrawer(Type fieldType, bool nonSaints)
        {
            // Type elementType = ReflectUtils.GetElementType(fieldInfo.FieldType);
            // if (elementType != fieldInfo.FieldType)
            // {
            //     lookingForType.Insert(0, elementType);
            // }

            bool isGenericType = fieldType.IsGenericType;
            // Debug.Log($"FindTypeDrawer for {fieldType}, isGenericType={isGenericType}");

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
            Debug.Log($"FindTypeDrawer for {fieldType}, isGenericType={isGenericType}");
#endif

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (KeyValuePair<Type, IReadOnlyList<PropertyDrawerInfo>> propertyAttributeToPropertyDrawer in PropertyAttributeToPropertyDrawers)
            {
                bool matched;
                if (isGenericType)
                {
                    Type genericType = fieldType.GetGenericTypeDefinition();
                    // maybe we only need the first one condition?
                    matched = propertyAttributeToPropertyDrawer.Key.IsAssignableFrom(fieldType) || genericType == propertyAttributeToPropertyDrawer.Key || genericType.IsSubclassOf(propertyAttributeToPropertyDrawer.Key);
                    // Debug.Log(fieldType.GetGenericTypeDefinition().IsSubclassOf(propertyAttributeToPropertyDrawer.Key));
                    // Debug.Log(fieldType.IsAssignableFrom(propertyAttributeToPropertyDrawer.Key));
                    // Debug.Log(propertyAttributeToPropertyDrawer.Key.IsAssignableFrom(fieldType));

                    // ReSharper disable once MergeIntoPattern
                    if (!matched && fieldType.BaseType != null && fieldType.BaseType.IsGenericType)
                    {
                        matched = propertyAttributeToPropertyDrawer.Key.IsAssignableFrom(fieldType.BaseType
                            .GetGenericTypeDefinition());
                    }
                }
                else
                {
                    matched = propertyAttributeToPropertyDrawer.Key.IsAssignableFrom(fieldType);
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                        Debug.Log($"{propertyAttributeToPropertyDrawer.Key}/{matched}/{string.Join(",", propertyAttributeToPropertyDrawer.Value.Select(each => each.DrawerType))}");
#endif
                    if (!matched && propertyAttributeToPropertyDrawer.Key.IsGenericType)
                    {
                        matched = ReflectUtils.IsSubclassOfRawGeneric(propertyAttributeToPropertyDrawer.Key, fieldType);
                    }
                }

// #if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
//                     Debug.Log($"fieldInfo.FieldType={fieldInfo.FieldType}, isGenericType={isGenericType}, GetGenericTypeDefinition={(isGenericType ? fieldInfo.FieldType.GetGenericTypeDefinition().ToString() : "")}, key={propertyAttributeToPropertyDrawer.Key}, {matched}");
// #endif
                // ReSharper disable once InvertIf
                if (matched)
                {
                    PropertyDrawerInfo first = new PropertyDrawerInfo();
                    foreach (PropertyDrawerInfo each in propertyAttributeToPropertyDrawer.Value)
                    {
                        // Debug.Log($"nonSaints={nonSaints}; each={each.DrawerType}, {each.IsSaints}");
                        if (nonSaints && each.IsSaints)
                        {
                            continue;
                        }
                        first = each;
                        break;
                    }
                    Type foundDrawer = first
                        .DrawerType;
#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_DRAW_PROCESS_CORE
                        Debug.Log($"foundDrawer={foundDrawer} for {fieldType}");
#endif
                    if (foundDrawer != null)
                    {
                        return foundDrawer;
                    }
                }
            }
            return null;
        }

        private static Type FindTypeDrawerNonSaints(Type baseType)
        {
            return FindTypeDrawer(baseType, true);
        }

        protected static Type FindTypeDrawerAny(Type baseType)
        {
            return FindTypeDrawer(baseType, false);
        }

        public static PropertyDrawer MakePropertyDrawer(Type foundDrawer, FieldInfo fieldInfo, Attribute attribute, string preferredLabel)
        {
            PropertyDrawer propertyDrawer;
            try
            {
                propertyDrawer = (PropertyDrawer)Activator.CreateInstance(foundDrawer, true);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning(foundDrawer);
                Debug.LogException(e);
#endif
                return null;
            }

            FieldInfo field = foundDrawer.GetField("m_FieldInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                return null;
            }

            field.SetValue(propertyDrawer, fieldInfo);

            FieldInfo mAttributeField = foundDrawer.GetField("m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance);
            if (mAttributeField == null)
            {
                return null;
            }

            mAttributeField.SetValue(propertyDrawer, attribute);

            FieldInfo preferredLabelField = foundDrawer.GetField("m_PreferredLabel", BindingFlags.NonPublic | BindingFlags.Instance);
            if (preferredLabelField != null)
            {
                // Debug.Log($"preferredLabelField={preferredLabelField}");
                preferredLabelField.SetValue(propertyDrawer, preferredLabel);
            }

            return propertyDrawer;
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        public static Type GetFirstSaintsDrawerType(Type attributeType)
        {
            // Debug.Log(attributeType);
            // Debug.Log(string.Join(",", _propertyAttributeToDrawers.Keys));

            EnsureAndGetTypeToDrawers();

            if (!PropertyAttributeToPropertyDrawers.TryGetValue(attributeType,
                    out IReadOnlyList<PropertyDrawerInfo> eachDrawer))
            {
                return null;
            }
            // Debug.Log($"{attributeType}/{eachDrawer.Count}");

            PropertyDrawerInfo drawerInfo = eachDrawer.FirstOrDefault(each => each.IsSaints);

            return drawerInfo.IsSaints ? drawerInfo.DrawerType : null;
        }

        private SaintsPropertyDrawer GetOrCreateSaintsDrawer(SaintsWithIndex saintsAttributeWithIndex)
        {
            if (_cachedDrawer.TryGetValue(saintsAttributeWithIndex, out SaintsPropertyDrawer drawer))
            {
                return drawer;
            }

            // Debug.Log($"create new drawer for {saintsAttributeWithIndex.SaintsAttribute}[{saintsAttributeWithIndex.Index}]");
            // Type drawerType = PropertyAttributeToDrawers[saintsAttributeWithIndex.SaintsAttribute.GetType()].First(each => each.isSaints).drawerType;
            return _cachedDrawer[saintsAttributeWithIndex] = GetOrCreateSaintsDrawerByAttr(saintsAttributeWithIndex.SaintsAttribute);
        }

        public static (Attribute attrOrNull, Type drawerType) GetFallbackDrawerType(MemberInfo info, SerializedProperty property)
        {
            EnsureAndGetTypeToDrawers();

            // check if any property has drawer. If so, just use PropertyField
            // if not, check if it has custom drawer. if it exists, then try use that custom drawer
            (Attribute attr, Type attributeDrawerType) = GetOtherAttributeDrawerType(info);
            if (attributeDrawerType != null)
            {
                return (attr, attributeDrawerType);
            }

            Type fieldType = info.MemberType == MemberTypes.Property
                ? ((PropertyInfo)info).PropertyType
                : ((FieldInfo)info).FieldType;

            Debug.Assert(fieldType != null, info);

            Type fieldElementType = SerializedUtils.IsArrayOrDirectlyInsideArray(property)
                ? ReflectUtils.GetElementType(fieldType)
                : fieldType;

            Type foundDrawer = FindTypeDrawerAny(fieldElementType);

            // ReSharper disable once InvertIf
            if (foundDrawer != null)
            {
                return (null, foundDrawer);
            }

            // if (fieldElementType.IsClass || (fieldElementType.IsValueType && !fieldElementType.IsPrimitive))
            // {
            //     return (null, typeof(SaintsRowAttributeDrawer));
            //     // Console.WriteLine("The type is a class.");
            // }

            return (null, null);
            // else if ()
            // {
            //     Console.WriteLine("The type is a struct.");
            // }
            // else
            // {
            //     Console.WriteLine("The type is neither a class nor a struct.");
            // }

            // Debug.Log($"PropertyFieldFallbackUIToolkit on foundDrawer={foundDrawer}: {property.displayName}");
            // return PropertyFieldFallbackUIToolkit(property);

            // On Hold... Cuz SaintsRow does not support copy/paste yet.
            // check if it's general class/struct. If so, use SaintsRow to draw it.
            // if (info.FieldType.IsClass || info.FieldType.IsValueType)
            // {
            //     PropertyDrawer saintsRowAttributeDrawer = MakePropertyDrawer(typeof(SaintsRowAttributeDrawer),
            //         info, new SaintsRowAttribute(), preferredLabel);
            //     VisualElement saintsRowElement = saintsRowAttributeDrawer.CreatePropertyGUI(property);
            //     return saintsRowElement;
            //     // return PropertyFieldFallbackUIToolkit(property);
            // }

            // return (null, typeof(SaintsRowAttributeDrawer));

            // return PropertyFieldFallbackUIToolkit(property);
        }

        private SaintsPropertyDrawer GetOrCreateSaintsDrawerByAttr(ISaintsAttribute saintsAttribute)
        {
            Type attributeType = saintsAttribute.GetType();
            if (!PropertyAttributeToPropertyDrawers.TryGetValue(attributeType,
                out IReadOnlyList<PropertyDrawerInfo> eachDrawer))
            {
                foreach (KeyValuePair<Type, IReadOnlyList<PropertyDrawerInfo>> kv in PropertyAttributeToPropertyDrawers)
                {
                    if(attributeType.IsSubclassOf(kv.Key))
                    {
                        eachDrawer = kv.Value.Where(each => each.UseForChildren).ToArray();
                        break;
                    }
                }
                if(eachDrawer == null || eachDrawer.Count == 0)
                {
                    throw new Exception($"No drawer found for {saintsAttribute}");
                }
            }

            Type drawerType = eachDrawer.First(each => each.IsSaints).DrawerType;
            SaintsPropertyDrawer saintsPropertyDrawer = (SaintsPropertyDrawer)Activator.CreateInstance(drawerType);
            saintsPropertyDrawer.InHorizontalLayout = InHorizontalLayout;

#if UNITY_2022_2_OR_NEWER  // don't bother with too old Unity
            FieldInfo preferredLabelField = typeof(PropertyDrawer).GetField("m_PreferredLabel", BindingFlags.NonPublic | BindingFlags.Instance);
            if (preferredLabelField != null)
            {
                // Debug.Log($"preferredLabelField={preferredLabelField}");
                preferredLabelField.SetValue(saintsPropertyDrawer, preferredLabelField.GetValue(this));
            }
#endif

            return saintsPropertyDrawer;
        }

        protected string GetPreferredLabel(SerializedProperty sp)
        {
#if UNITY_2022_3_OR_NEWER
            return preferredLabel;
#elif UNITY_2022_2_OR_NEWER  // Unity such a mess...
            FieldInfo preferredLabelField = GetType().GetField("m_PreferredLabel", BindingFlags.NonPublic | BindingFlags.Instance);
            if (preferredLabelField != null)
            {
                // Debug.Log($"preferredLabelField={preferredLabelField}");
                return (string)preferredLabelField.GetValue(this);
            }

            return sp.displayName;
#else
            return sp.displayName;
#endif
        }

        public AbsRenderer MakeRenderer(SerializedObject serializedObject, SaintsFieldWithInfo fieldWithInfo)
        {
            return SaintsEditor.HelperMakeRenderer(serializedObject, fieldWithInfo);
        }

        public void Dispose()
        {
            OnDisposeIMGUI();
#if UNITY_2021_3_OR_NEWER
            OnDisposeUIToolkit();
#endif
        }
    }
}

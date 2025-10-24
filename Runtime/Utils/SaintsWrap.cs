using System.Collections;
using System.Collections.Generic;
using SaintsField.SaintsSerialization;
using UnityEngine;
// ReSharper disable once CheckNamespace
using System;

namespace SaintsField.Utils
{
    [Serializable]
    public class SaintsWrap<T> : BaseWrap<T>, ISerializationCallbackReceiver
    {
        [SerializeField] public T value;
        [SerializeField] public SaintsSerializedProperty valueField;
        [SerializeField] public SaintsSerializedProperty[] valueArray = Array.Empty<SaintsSerializedProperty>();
        [SerializeField] public List<SaintsSerializedProperty> valueList = new List<SaintsSerializedProperty>();
        [SerializeField] public WrapType wrapType;

        private T _runtimeResult;

        public override T Value
        {
            get
            {
                EnsureOnAfterDeserializeOnce();
                return _runtimeResult;
            }
            set => _runtimeResult = value;
        }

#if UNITY_EDITOR
        // ReSharper disable once StaticMemberInGenericType
        public static readonly string EditorPropertyName = nameof(value);
#endif

        public SaintsWrap(T v)
        {
            _runtimeResult = v;
        }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            EnsureInit();
            switch (wrapType)
            {
                case WrapType.Undefined:
                case WrapType.T:
                {
                    value = _runtimeResult;
                }
                    break;
                case WrapType.Array:
                {
                    if (_runtimeResult == null)
                    {
                        _runtimeResult = (T) (object)Array.CreateInstance(_subType, 0);
                        valueArray = Array.Empty<SaintsSerializedProperty>();
                        // Debug.Log("init valueArray to empty");
                    }
                    else
                    {
                        List<object> lis = new List<object>();
                        // ReSharper disable once LoopCanBeConvertedToQuery
                        foreach (object o in (IEnumerable)_runtimeResult)
                        {
                            lis.Add(o);
                        }

                        List<SaintsSerializedProperty> oldArray = new List<SaintsSerializedProperty>(valueArray);
                        if (valueArray.Length != lis.Count)
                        {
                            valueArray = new SaintsSerializedProperty[lis.Count];
                        }
                        // valueArray = new SaintsSerializedProperty[lis.Count];
                        int index = 0;

                        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                        foreach (object o in lis)
                        {
                            bool isVRef = index < oldArray.Count && oldArray[index].IsVRef;
                            // Debug.Log($"before ser {index}={thisSer.V}/{thisSer.VRef}");

                            if (!SaintsSerializedPropertyEqual(o, valueArray[index], isVRef))
                            {
                                SaintsSerializedProperty thisSer = GetSaintsSerializedProperty(o, isVRef);
                                // Debug.Log($"on before ser not equal {index} {valueArray[index].VRef}->{thisSer.VRef}: {o}");
                                valueArray[index] = thisSer;
                            }

                            index++;
                        }

                    }
                }
                    break;
                case WrapType.List:
                {
                    if (_runtimeResult == null)
                    {
                        // Debug.Log(_listType);
                        _runtimeResult = (T)Activator.CreateInstance(_listType);
                        valueList.Clear();
                    }
                    else
                    {
                        List<SaintsSerializedProperty> oldArray = new List<SaintsSerializedProperty>(valueList);

                        // valueList.Clear();
                        int index = 0;
                        foreach (object o in (IEnumerable)_runtimeResult)
                        {
                            bool isVRef = index < oldArray.Count && oldArray[index].IsVRef;
                            SaintsSerializedProperty thisSer = GetSaintsSerializedProperty(o, isVRef);
                            if(index < valueList.Count)
                            {
                                if (!SaintsSerializedPropertyEqual(o, valueList[index], isVRef))
                                {
                                    // Debug.Log($"on before ser not equal {index} {valueList[index].VRef}->{thisSer.VRef}: {o}");
                                    valueList[index] = thisSer;
                                }
                            }
                            else
                            {
                                // Debug.Log($"on before ser add {index} {thisSer.VRef}: {o}");
                                valueList.Add(thisSer);
                            }

                            index++;
                        }

                        int shouldBeTotal = index;
                        if (valueList.Count > shouldBeTotal)
                        {
                            valueList.RemoveRange(shouldBeTotal, valueList.Count - shouldBeTotal);
                        }

                    }
                }
                    break;
                case WrapType.Field:
                {
                    valueField = GetSaintsSerializedProperty(_runtimeResult, valueField.IsVRef);
                }
                    break;
            }
#endif
        }

        private bool SaintsSerializedPropertyEqual(object o, SaintsSerializedProperty thisSer, bool isVRef)
        {
            if (isVRef)
            {
                return thisSer.VRef == o;
            }

            if(o is UnityEngine.Object uo)
            {
                return thisSer.V == uo;
            }

            if (RuntimeUtil.IsNull(thisSer.V))
            {
                return o == null;
            }

#pragma warning disable CS0252, CS0253
            return thisSer.V == o;
#pragma warning restore CS0252, CS0253
        }

        private SaintsSerializedProperty GetSaintsSerializedProperty(object o, bool isVRef)
        {
            bool isVRefValue = isVRef;

            Type elementType;
            if (_isArray || _isList)
            {
                elementType = _subType;
            }
            else
            {
                elementType = typeof(T);
            }

            if(elementType.IsClass && !typeof(UnityEngine.Object).IsAssignableFrom(elementType))
            {
                isVRefValue = true;
            }
            else if(elementType.IsValueType)
            {
                isVRefValue = true;
            }

            if (RuntimeUtil.IsNull(o))
            {
                return new SaintsSerializedProperty
                {
                    propertyType = SaintsPropertyType.Interface,
                    IsVRef = isVRefValue,
                };
            }

            if (o is UnityEngine.Object uo)
            {
                return new SaintsSerializedProperty
                {
                    propertyType = SaintsPropertyType.Interface,
                    V = uo,
                };
            }

            return new SaintsSerializedProperty
            {
                propertyType = SaintsPropertyType.Interface,
                VRef = o,
                IsVRef = isVRefValue,
            };
        }

        // public bool OnAfterDeserializeReady { get; private set; }
        private bool _onAfterDeserializeOnce;

        private void EnsureOnAfterDeserializeOnce()
        {
            if (!_onAfterDeserializeOnce)
            {
                OnAfterDeserialize();
            }
        }

        public void OnAfterDeserialize()
        {
            _onAfterDeserializeOnce = true;
            // return;
            EnsureInit();
            switch (wrapType)
            {
                case WrapType.T:
                {
                    _runtimeResult = value;
                    // Debug.Log($"set runtime to {_runtimeResult}");
                }
                    break;
                case WrapType.Array:
                {
                    if (_runtimeResult == null)
                    {
                        _runtimeResult = (T) (object)Array.CreateInstance(_subType, valueArray.Length);
                    }
                    Array runtimeArray = (Array)(object)_runtimeResult;

                    if (runtimeArray.Length != valueArray.Length)
                    {
                        runtimeArray = Array.CreateInstance(_subType, valueArray.Length);
                        _runtimeResult = (T) (object)runtimeArray;
                    }

                    // Array arr = Array.CreateInstance(_subType, valueArray.Length);
                    for (int index = 0; index < valueArray.Length; index++)
                    {
                        SaintsSerializedProperty serObj = valueArray[index];
                        object runtimeValue = runtimeArray.GetValue(index);
                        if(!SaintsSerializedPropertyEqual(runtimeValue, serObj, serObj.IsVRef))
                        {
                            object getActualValue = GetObjectFromSaintsSerializedProperty(serObj, _subType);
                            // Debug.Log($"after ser {index}={serObj.V?.GetType()}/{serObj.VRef?.GetType()}->{getActualValue}");
                            runtimeArray.SetValue(getActualValue, index);
                        }
                        // Debug.Log($"after ser {index} done");
                    }

                    // runtimeResult = (T) (object)arr;
                }
                    break;
                case WrapType.List:
                {
                    IList lis = (IList)Activator.CreateInstance(_listType, valueList.Count);
                    foreach (SaintsSerializedProperty serObj in valueList)
                    {
                        lis.Add(GetObjectFromSaintsSerializedProperty(serObj, _subType));
                    }

                    _runtimeResult = (T) lis;
                }
                    break;

                case WrapType.Field:
                {
                    _runtimeResult = GetFromSaintsSerializedProperty(valueField);
                }
                    break;
                case WrapType.Undefined:  // Never inspected, ignore
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wrapType), wrapType, null);
            }

            // Debug.Log("OnAfterDeserialize done");
#if UNITY_EDITOR
            // do nothing
#else
            value = default;
            valueField = default;
            valueArray = Array.Empty<SaintsSerializedProperty>();
            valueList.Clear();
#endif
        }

        private static object GetObjectFromSaintsSerializedProperty(SaintsSerializedProperty serObj, Type targetType)
        {
            if (serObj.IsVRef)
            {
                if (serObj.VRef == null)
                {
                    return targetType.IsValueType? Activator.CreateInstance(targetType) : null;
                }

                // Debug.Log($"{serObj.VRef}: {serObj.VRef.GetType()} -> {targetType}");
                // return Convert.ChangeType(serObj.VRef, targetType);
                return serObj.VRef;
            }

            if (RuntimeUtil.IsNull(serObj.V))
            {
                return targetType.IsValueType? Activator.CreateInstance(targetType) : null;
            }

            // return Convert.ChangeType(serObj.V, targetType);
            return serObj.V;
        }

        private static T GetFromSaintsSerializedProperty(SaintsSerializedProperty serObj)
        {
            if (serObj.IsVRef)
            {
                if (serObj.VRef == null)
                {
                    return default;
                }

                // Debug.Log($"{serObj.VRef}: {serObj.VRef.GetType()} -> {typeof(T)}");
                return (T)serObj.VRef;
            }

            if (RuntimeUtil.IsNull(serObj.V))
            {
                return default;
            }

            return (T) (object)serObj.V;
        }

        // private bool _init;
        private Type _subType;
        private Type _listType;
        private bool _isArray;
        private bool _isList;

        private void EnsureInit()
        {
            // if (_init)
            // {
            //     return;
            // }
            //
            // _init = true;

            Type t = typeof(T);
            // Debug.Log($"{t}/{t.IsGenericType}/{(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))}");
            WrapType wrapT;
            if (t.IsArray)
            {
                _isArray = true;

                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if(_subType == null)
                {
                    _subType = t.GetElementType();
                }

                wrapT = RuntimeUtil.IsSubFieldUnitySerializable(_subType)? WrapType.T: WrapType.Array;
            }
            else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
            {
                _isList = true;

                // wrapT = WrapType.List;
                if (_subType == null)
                {
                    _subType = t.GetGenericArguments()[0];
                }
                if(_listType == null)
                {
                    _listType = typeof(List<>).MakeGenericType(_subType);
                }

                wrapT = RuntimeUtil.IsSubFieldUnitySerializable(_subType)? WrapType.T: WrapType.List;
                // Debug.Log($"_listType={_listType}");
            }
            else
            {
                wrapT = RuntimeUtil.IsSubFieldUnitySerializable(t)? WrapType.T: WrapType.Field;
            }

            // ReSharper disable once RedundantCheckBeforeAssignment
            if (wrapType == WrapType.Undefined)
            {
#if SAINTSFIELD_DEBUG
                Debug.Log($"SaintsWrap reset wrapType {wrapType} -> {wrapT} for {t}");
#endif
                wrapType = wrapT;
            }

            // Debug.Log($"{t}: {wrapType}");

        }
    }
}

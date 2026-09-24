using System;
using System.Collections;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.BaseRenderer.EditUnityEvent
{
    public class UnityEventRuntimeElement: VisualElement
    {
        private VisualElement _container;

        public UnityEventRuntimeElement(string label)
        {
            Add(new Label(label));
            Add(_container = new VisualElement());
        }


        public void SetValueWithoutNotify(UnityEventBase unityEvent)
        {
            _container.Clear();

            object calls = GetFieldValue(unityEvent, "m_Calls");
            IList runtimeCalls = calls == null
                ? null
                : GetFieldValue(calls, "m_RuntimeCalls") as IList;
            _container.Add(new Label(runtimeCalls?.ToString()));
            if (runtimeCalls != null)
            {
                foreach (object runtimeCall in runtimeCalls)
                {
                    if (runtimeCall == null)
                    {
                        _container.Add(new Label("null"));
                    }
                    else
                    {
                        _container.Add(new Label(runtimeCall.ToString()));
                    }
                }
            }
        }

        private static object GetFieldValue(object instance, string fieldName)
        {
            if (instance == null)
            {
                return null;
            }

            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public |
                                                           BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field.GetValue(instance);
                }
            }

            return null;
        }
    }
}

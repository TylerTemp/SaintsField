using System;
using System.Reflection;
using SaintsField.ComponentHeader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaintsField.Editor.HeaderGUI.Drawer
{
    public static class HeaderDrawDrawer
    {
        public static bool ValidateMethodInfo(MethodInfo methodInfo, Type reflectedType)
        {
            ParameterInfo[] methodParams = methodInfo.GetParameters();

            if (methodParams.Length == 0)
            {
                Debug.LogWarning($"method {methodInfo.Name} in {reflectedType} does not accept any argument, skip");
                return false;
            }

            Type methodParamType = methodParams[0].ParameterType;
            if (methodParamType != typeof(HeaderArea))
            {
                Debug.LogWarning($"method {methodInfo.Name} in {reflectedType} does not have 1st argument as HeaderArea, skip");
                return false;
            }

            for (int index = 1; index < methodParams.Length; index++)
            {
                ParameterInfo otherParmas = methodParams[index];
                // ReSharper disable once InvertIf
                if (!otherParmas.IsOptional)
                {
                    Debug.LogWarning($"method {methodInfo.Name}.{otherParmas.Name}({index}) in {reflectedType} is not optional, skip");
                    return false;
                }
            }

            Type methodReturn = methodInfo.ReturnType;
            // ReSharper disable once InvertIf
            if (methodReturn != typeof(HeaderUsed))
            {
                Debug.LogWarning($"method {methodInfo.Name} in {reflectedType} does not return HeaderUsed, skip");
                return false;
            }

            return true;
        }

        public static (bool used, HeaderUsed headerUsed) Draw(Object target, HeaderArea headerArea, DrawHeaderGUI.RenderTargetInfo renderTargetInfo)
        {
            MethodInfo method = (MethodInfo)renderTargetInfo.MemberInfo;

            ParameterInfo[] methodParams = method.GetParameters();
            object[] methodPass = new object[methodParams.Length];
            methodPass[0] = headerArea;
            for (int index = 1; index < methodParams.Length; index++)
            {
                ParameterInfo param = methodParams[index];
                object defaultValue = param.DefaultValue;
                methodPass[index] = defaultValue;
            }

            HeaderUsed methodReturn;
            try
            {
                methodReturn = (HeaderUsed)method.Invoke(target, methodPass);
            }
            catch (Exception e)
            {
                Debug.LogException(e.InnerException ?? e);
                return (false, default);
            }

            return (true, methodReturn);
        }
    }
}

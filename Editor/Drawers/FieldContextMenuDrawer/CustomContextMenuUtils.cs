using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Drawers.ButtonDrawers.DecButtonDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.FieldContextMenuDrawer
{
    public static class CustomContextMenuUtils
    {
        public interface IManipulatorHandler
        {
            void SetHelpBox(string error);
            void SetIEnumerators(IReadOnlyCollection<IEnumerator> enumerators);
            object GetParent();
        }

        public static void AddManipulator(VisualElement container, string funcName, string menuName, bool menuNameIsCallback, bool isFirst, SerializedProperty property, FieldInfo info, IManipulatorHandler manipulatorHandler)
        {
            container.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                string useMenuName = menuName;
                DropdownMenuAction.Status status = DropdownMenuAction.Status.Normal;
                if (menuNameIsCallback)
                {
                    (string error, object result) = Util.GetOf<object>(menuName, null, property, info, manipulatorHandler.GetParent(), null);
                    // UIToolkitUtils.SetHelpBox(helpBox, error);
                    if (error != "")
                    {
                        manipulatorHandler.SetHelpBox(error);
                    }
                    else
                    {
                        // ReSharper disable once ConvertIfStatementToSwitchStatement
                        if (result is null or "") //  don't do menu this time
                        {
                            return;
                        }

                        if (result is ValueTuple<string, EContextMenuStatus> sb)
                        {
                            useMenuName = sb.Item1;

                            if (useMenuName is null or "") //  don't do menu this time
                            {
                                return;
                            }

                            status = sb.Item2 switch
                            {
                                EContextMenuStatus.Normal => DropdownMenuAction.Status.Normal,
                                EContextMenuStatus.Checked => DropdownMenuAction.Status.Checked,
                                EContextMenuStatus.Disabled => DropdownMenuAction.Status.Disabled,
                                _ => throw new ArgumentOutOfRangeException(nameof(sb.Item2), sb.Item2, null),
                            };
                            // isChecked = sb.Item2;
                            // isDisabled = sb.Item3;
                        }
                        else
                        {
                            useMenuName = result.ToString();
                        }
                    }
                }

                if (funcName == null)
                {
                    evt.menu.AppendSeparator(useMenuName);
                    return;
                }

                if (isFirst)
                {
                    // Debug.Log("AppendSeparator for first");
                    evt.menu.AppendSeparator();
                }

                evt.menu.AppendAction(useMenuName, _ =>
                {
                    string buttonError = "";
                    List<IEnumerator> enumerators = new List<IEnumerator>();
                    // ReSharper disable once PossibleNullReferenceException
                    // ReSharper disable once AccessToModifiedClosure
                    // HashSet<IEnumerator> enumerators = (HashSet<IEnumerator>)buttonElement.userData;
                    foreach ((string eachError, object buttonResult) in DecButtonAttributeDrawer.CallButtonFunc(property, funcName, info, manipulatorHandler.GetParent()))
                    {
                        // Debug.Log($"{eachError}/{buttonResult}");
                        if (eachError == "")
                        {
                            // Debug.Log(buttonResult is IEnumerator);
                            if (buttonResult is IEnumerator enumerator)
                            {
                                enumerators.Add(enumerator);
                            }
                        }
                        else
                        {
                            buttonError += eachError;
                        }
                    }

                    manipulatorHandler.SetIEnumerators(enumerators);

                    manipulatorHandler.SetHelpBox(buttonError);
                }, status);
            }));
        }
    }
}

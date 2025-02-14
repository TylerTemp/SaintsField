using System;
using System.IO;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Addressable.AddressableResourceDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(AddressableResourceAttribute), true)]
    public partial class AddressableResourceAttributeDrawer: SaintsPropertyDrawer
    {

        public enum NameType
        {
            FilePath,
            FileNameBase,
            FileName,
            // ReSharper disable once InconsistentNaming
            GUID,
        }

        private static string GetObjectName(NameType nameType, UnityEngine.Object curObj)
        {
            // ReSharper disable once ConvertSwitchStatementToSwitchExpression
            switch (nameType)
            {
                case NameType.FilePath:
                    return AssetDatabase.GetAssetPath(curObj);
                case NameType.FileNameBase:
                    return Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(curObj));
                case NameType.FileName:
                    return Path.GetFileName(AssetDatabase.GetAssetPath(curObj));
                case NameType.GUID:
                    return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(curObj));
                default:
                    throw new ArgumentOutOfRangeException(nameof(nameType), nameType, null);
            }
        }
    }

    public static class NameTypeExt
    {
        public static string ToFriendlyString(this AddressableResourceAttributeDrawer.NameType nameType)
        {
            switch(nameType)
            {
                case AddressableResourceAttributeDrawer.NameType.FilePath:
                    return "File Path";
                case AddressableResourceAttributeDrawer.NameType.FileNameBase:
                    return "File Name Base";
                case AddressableResourceAttributeDrawer.NameType.FileName:
                    return "File Name";
                case AddressableResourceAttributeDrawer.NameType.GUID:
                    return "GUID";
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(nameType), nameType, null);
            }
        }
    }
}

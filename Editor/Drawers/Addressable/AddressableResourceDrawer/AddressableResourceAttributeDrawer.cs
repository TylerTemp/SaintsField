#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
using System;
using System.IO;
using SaintsField.Addressable;
using SaintsField.Editor.Core;
using UnityEditor;

namespace SaintsField.Editor.Drawers.Addressable.AddressableResourceDrawer
{
    [CustomPropertyDrawer(typeof(AddressableResourceAttribute))]
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
            return nameType switch
            {
                AddressableResourceAttributeDrawer.NameType.FilePath => "File Path",
                AddressableResourceAttributeDrawer.NameType.FileNameBase => "File Name Base",
                AddressableResourceAttributeDrawer.NameType.FileName => "File Name",
                AddressableResourceAttributeDrawer.NameType.GUID => "GUID",
                _ => throw new System.ArgumentOutOfRangeException(nameof(nameType), nameType, null),
            };
        }
    }
}
#endif

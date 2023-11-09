// namespace SaintsField.DropdownBase
// {
//     public class DropdownItem<T>
//     {
//         public bool IsSeparator;
//         public string SeparatorPath;
//
//         public T Value;
//         public bool Disabled;
//
//         public DropdownItem() { }
//
//         public DropdownItem(T value, bool disabled=false)
//         {
//             Value = value;
//             Disabled = disabled;
//         }
//
//         public static DropdownItem<T> Separator(string separatorPath="")
//         {
//             return new DropdownItem<T>
//             {
//                 IsSeparator = true,
//                 SeparatorPath = separatorPath,
//                 Value = default,
//                 Disabled = default,
//             };
//         }
//     }
// }

// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
//
// namespace SaintsField.DropdownBase
// {
//     public class DropdownItems<DropdownItem<T>> : IDropdownList
//     {
//         private readonly List<KeyValuePair<string, object>> _values;
//
//         public DropdownItems() => _values = new List<KeyValuePair<string, object>>();
//         public DropdownItems(IEnumerable<KeyValuePair<string, T>> value) => _values = value.Select(each => new KeyValuePair<string, object>(each.Key, each.Value)).ToList();
//
//         public void Add(string displayName, T value) => _values.Add(new KeyValuePair<string, object>(displayName, value));
//         public void AddRange(KeyValuePair<string, T> pair) => _values.Add(new KeyValuePair<string, object>(pair.Key, pair.Value));
//
//         public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _values.GetEnumerator();
//
//         IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
//
//         public static explicit operator DropdownList<object>(DropdownList<T> target)
//         {
//             DropdownList<object> result = new DropdownList<object>();
//             foreach (KeyValuePair<string, object> kvp in target)
//             {
//                 result.Add(kvp.Key, kvp.Value);
//             }
//
//             return result;
//         }
//     }
// }

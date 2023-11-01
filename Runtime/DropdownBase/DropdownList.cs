using System.Collections;
using System.Collections.Generic;

namespace SaintsField.DropdownBase
{
    public class DropdownList<T> : IDropdownList
    {
        private readonly List<KeyValuePair<string, object>> _values;

        public DropdownList()
        {
            _values = new List<KeyValuePair<string, object>>();
        }

        public void Add(string displayName, T value)
        {
            _values.Add(new KeyValuePair<string, object>(displayName, value));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static explicit operator DropdownList<object>(DropdownList<T> target)
        {
            DropdownList<object> result = new DropdownList<object>();
            foreach (KeyValuePair<string, object> kvp in target)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            return result;
        }
    }
}

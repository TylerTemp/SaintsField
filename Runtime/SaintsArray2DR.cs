using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Utils;
using UnityEngine;

namespace SaintsField
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Array two-dimensional rectangular
    /// </summary>
    [Serializable]
    public class SaintsArray2DR<T>: IWrapProp
        , IReadOnlyCollection<T>
        , ICollection
        , ICloneable
        , IStructuralComparable
        , IStructuralEquatable
        , ISerializationCallbackReceiver
    {
        // ReSharper disable once InconsistentNaming
        [SerializeField] public List<SaintsList<T>> _saintsList = new List<SaintsList<T>>();
        [SerializeField] private WrapType _wrapType;
        [SerializeField] private int _columnCount;

#pragma warning disable CS0414 // Field is assigned but its value is never used
        [SerializeField] private int _saintsSerializedVersion;
#pragma warning restore CS0414 // Field is assigned but its value is never used
        private const int SaintsSerializedVersionRuntime = 1;

        // actual value
        private T[,] _array = new T[0, 0];

#if UNITY_EDITOR
        public static readonly string EditorPropertyName = nameof(_saintsList);
#endif

        public static implicit operator T[,](SaintsArray2DR<T> saintsArray) => saintsArray._array;

        public static implicit operator SaintsArray2DR<T>(T[,] array) => new SaintsArray2DR<T>(array);

        public override string ToString() => _array.ToString();

        public SaintsArray2DR()
        {
            _saintsSerializedVersion = SaintsSerializedVersionRuntime;
            _wrapType = SaintsWrap<T>.GuessWrapType();
        }

        public SaintsArray2DR(T[,] array): this()
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            _array = (T[,])array.Clone();
            _columnCount = array.GetLength(1);
#if UNITY_EDITOR
            CopyToSerializedRows();
#endif
        }

        public SaintsArray2DR(int length0, int length1): this()
        {
            _array = new T[length0, length1];
            _columnCount = length1;
#if UNITY_EDITOR
            CopyToSerializedRows();
#endif
        }

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T value in _array)
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();

        #endregion

        #region ICollection

        public void CopyTo(Array array, int index) => _array.CopyTo(array, index);
        public int Count => _array.Length;
        public bool IsSynchronized => _array.IsSynchronized;
        public object SyncRoot => _array.SyncRoot;

        #endregion

        #region ICloneable

        public object Clone() => _array.Clone();

        #endregion

        #region IStructuralComparable

        public int CompareTo(object other, IComparer comparer) =>
            ((IStructuralComparable)_array).CompareTo(other, comparer);

        #endregion

        #region IStructuralEquatable

        public bool Equals(object other, IEqualityComparer comparer) =>
            ((IStructuralEquatable)_array).Equals(other, comparer);

        public int GetHashCode(IEqualityComparer comparer) =>
            ((IStructuralEquatable)_array).GetHashCode(comparer);

        #endregion

        public T this[int index0, int index1]
        {
            get => _array[index0, index1];
            set
            {
                _array[index0, index1] = value;
#if UNITY_EDITOR
                if (_saintsList.Count == _array.GetLength(0) &&
                    index0 < _saintsList.Count &&
                    _saintsList[index0] != null &&
                    _saintsList[index0].Count == _array.GetLength(1))
                {
                    _saintsList[index0][index1] = value;
                }
                else
                {
                    CopyToSerializedRows();
                }
#endif
            }
        }

        public int Length => _array.Length;
        public long LongLength => _array.LongLength;
        public int Rank => _array.Rank;

        public int GetLength(int dimension) => _array.GetLength(dimension);
        public long GetLongLength(int dimension) => _array.GetLongLength(dimension);
        public int GetLowerBound(int dimension) => _array.GetLowerBound(dimension);
        public int GetUpperBound(int dimension) => _array.GetUpperBound(dimension);

        public void OnBeforeSerialize()
        {
#if !UNITY_EDITOR
            CopyToSerializedRows();
#endif
            _saintsSerializedVersion = SaintsSerializedVersionRuntime;
        }

#if UNITY_EDITOR
        private HashSet<SaintsList<T>> _editorWatchedRows = new HashSet<SaintsList<T>>();
#endif

        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            IEnumerable<SaintsList<T>> extraRows = _saintsList
                .Where(row => row != null)
                .Except(_editorWatchedRows);
            foreach (SaintsList<T> row in extraRows)
            {
                row.EditorOnAfterDeserializeChanged.AddListener(OnAfterDeserializeProcess);
                _editorWatchedRows.Add(row);
            }
#endif
            OnAfterDeserializeProcess();
        }

        private void OnAfterDeserializeProcess()
        {
            int rows = _saintsList.Count;
            if (rows == 0)
            {
                _columnCount = Math.Max(0, _columnCount);
                _array = new T[0, _columnCount];
                return;
            }

            SaintsList<T> firstRow = _saintsList[0];
            if (firstRow == null)
            {
                throw new InvalidOperationException("A rectangular array cannot contain a null row.");
            }

            int columns = firstRow.Count;
            _columnCount = columns;
            T[,] array = new T[rows, columns];
            for (int row = 0; row < rows; row++)
            {
                SaintsList<T> serializedRow = _saintsList[row];
                if (serializedRow == null || serializedRow.Count != columns)
                {
                    throw new InvalidOperationException("All rows in a rectangular array must have the same length.");
                }

                for (int column = 0; column < columns; column++)
                {
                    array[row, column] = serializedRow[column];
                }
            }

            _array = array;
        }

        private void CopyToSerializedRows()
        {
            int rows = _array.GetLength(0);  // 行，外部
            int columns = _array.GetLength(1);  // 列，内部
            _columnCount = columns;
            _saintsList.Clear();
            for (int row = 0; row < rows; row++)
            {
                T[] serializedRow = new T[columns];
                for (int column = 0; column < columns; column++)
                {
                    serializedRow[column] = _array[row, column];
                }

                _saintsList.Add(new SaintsList<T>(serializedRow));
            }
        }

    }
}

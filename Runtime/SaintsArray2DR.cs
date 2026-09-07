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

#pragma warning disable CS0414 // Field is assigned but its value is never used
        [SerializeField] private int _saintsSerializedVersion;
#pragma warning restore CS0414 // Field is assigned but its value is never used
        private const int SaintsSerializedVersionRuntime = 1;

        // actual value
        private T[,] _array = new T[0, 0];
        private bool _dirty;

        private void MarkDirty() => _dirty = true;

        private T[,] EnsureArray()
        {
            // ReSharper disable once InvertIf
            if (_dirty)
            {
                OnAfterDeserializeProcess();
                _dirty = false;
            }

            return _array;
        }

#if UNITY_EDITOR
        public static readonly string EditorPropertyName = nameof(_saintsList);
#endif

        public static implicit operator T[,](SaintsArray2DR<T> saintsArray) => saintsArray.EnsureArray();

        public static implicit operator SaintsArray2DR<T>(T[,] array) => new SaintsArray2DR<T>(array);

        public override string ToString() => EnsureArray().ToString();

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
#if UNITY_EDITOR
            CopyToSerializedRows();
#endif
        }

        public SaintsArray2DR(int length0, int length1): this()
        {
            _array = new T[length0, length1];
#if UNITY_EDITOR
            CopyToSerializedRows();
#endif
        }

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T value in EnsureArray())
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => EnsureArray().GetEnumerator();

        #endregion

        #region ICollection

        public void CopyTo(Array array, int index) => EnsureArray().CopyTo(array, index);
        public int Count => EnsureArray().Length;
        public bool IsSynchronized => EnsureArray().IsSynchronized;
        public object SyncRoot => EnsureArray().SyncRoot;

        #endregion

        #region ICloneable

        public object Clone() => EnsureArray().Clone();

        #endregion

        #region IStructuralComparable

        public int CompareTo(object other, IComparer comparer) =>
            ((IStructuralComparable)EnsureArray()).CompareTo(other, comparer);

        #endregion

        #region IStructuralEquatable

        public bool Equals(object other, IEqualityComparer comparer) =>
            ((IStructuralEquatable)EnsureArray()).Equals(other, comparer);

        public int GetHashCode(IEqualityComparer comparer) =>
            ((IStructuralEquatable)EnsureArray()).GetHashCode(comparer);

        #endregion

        public T this[int index0, int index1]
        {
            get => EnsureArray()[index0, index1];
            set
            {
                EnsureArray()[index0, index1] = value;
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

        public int Length => EnsureArray().Length;
        public long LongLength => EnsureArray().LongLength;
        public int Rank => EnsureArray().Rank;

        public int GetLength(int dimension) => EnsureArray().GetLength(dimension);
        public long GetLongLength(int dimension) => EnsureArray().GetLongLength(dimension);
        public int GetLowerBound(int dimension) => EnsureArray().GetLowerBound(dimension);
        public int GetUpperBound(int dimension) => EnsureArray().GetUpperBound(dimension);

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
                row.EditorOnAfterDeserializeChanged.AddListener(MarkDirty);
                _editorWatchedRows.Add(row);
            }
#endif

            MarkDirty();
        }

        private void OnAfterDeserializeProcess()
        {
            int rows = _saintsList.Count;
            if (rows == 0)
            {
                _array = new T[0, 0];
                return;
            }

            SaintsList<T> firstRow = _saintsList[0];
            if (firstRow == null)
            {
                throw new InvalidOperationException("A rectangular array cannot contain a null row.");
            }

            int columns = firstRow.Count;
            T[,] array = new T[rows, columns];
            for (int row = 0; row < rows; row++)
            {
                SaintsList<T> serializedRow = _saintsList[row];
                if (serializedRow == null || serializedRow.Count != columns)
                {
                    throw new InvalidOperationException($"All rows in a rectangular array must have the same length; expect={columns}, get[{row}]={serializedRow?.Count}");
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
            EnsureArray();
            int rows = _array.GetLength(0);  // 行，外部
            int columns = _array.GetLength(1);  // 列，内部
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

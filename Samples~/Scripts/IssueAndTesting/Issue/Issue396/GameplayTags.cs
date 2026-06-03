using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue396
{
    public abstract class GameplayTags<T> : GameplayTagsBase where T : struct, Enum
    {
        [SerializeField] [SaintsHashSet(numberOfItemsPerPage: 10)] private SaintsHashSet<T> _tags;

        public void Add(T tag)
        {
            _tags.Add(tag);
        }

        public void Add(ReadOnlySpan<T> tags)
        {
            for(int i = 0; i < tags.Length; i++)
                Add(tags[i]);
        }

        public bool Contains(T tag) => _tags.Contains(tag);

        public bool Contains(in ReadOnlySpan<T> tags)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (!Contains(tags[i]))
                    return false;
            }

            return true;
        }

        public bool ContainsAny(in ReadOnlySpan<T> tags)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (Contains(tags[i]))
                    return true;
            }

            return false;
        }

        public void Remove(T tag) => _tags.Remove(tag);

        public void Remove(in ReadOnlySpan<T> tags)
        {
            for(int i = 0; i < tags.Length; i++)
                Remove(tags[i]);
        }
    }
}

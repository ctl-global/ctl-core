using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    public struct StringSegment : IList<char>, IReadOnlyList<char>, IEquatable<StringSegment>, IComparable<StringSegment>
    {
        readonly string val;

        public string String => val ?? string.Empty;
        public int Offset { get; }
        public int Count { get; }

        public bool IsReadOnly => true;

        char IList<char>.this[int index]
        {
            get
            {
                return this[index];
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public char this[int index]
        {
            get
            {
                if (index >= Count) throw new IndexOutOfRangeException();
                return String[Offset + index];
            }
        }

        public StringSegment(string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            this.val = s;
            this.Offset = 0;
            this.Count = s.Length;
        }

        public StringSegment(string s, int offset, int count)
        {
            if (offset < 0 || offset >= s.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || count > (s.Length - offset)) throw new ArgumentOutOfRangeException(nameof(count));

            this.val = s;
            this.Offset = offset;
            this.Count = count;
        }

        public IEnumerator<char> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return String[Offset + i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(char item) => String.IndexOf(item, Offset, Count);

        public void Insert(int index, char item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(char item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(char item) => IndexOf(item) != -1;

        public void CopyTo(char[] array, int arrayIndex) => String.CopyTo(Offset, array, arrayIndex, Count);

        public bool Remove(char item)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => String.Substring(Offset, Count);

        public override int GetHashCode() => ToString().GetHashCode();

        public override bool Equals(object obj) => (obj as StringSegment?)?.Equals(this) == true;

        public bool Equals(StringSegment other) => CompareTo(other) == 0;

        public int CompareTo(StringSegment other)
        {
            int res = string.Compare(String, Offset, other.String, other.Offset, Math.Min(Count, other.Count));
            return res != 0 ? res : Count.CompareTo(other.Count);
        }
    }
}

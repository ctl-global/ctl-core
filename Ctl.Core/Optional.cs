using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Represents an empty optional value.
    /// </summary>
    /// <remarks>
    /// This is a sentinel type used for assigning a non-generic Optional.Empty to any Optional type.
    /// </remarks>
    public struct EmptyOptional
    {
    }

    /// <summary>
    /// Utility methods for use with Optional types.
    /// </summary>
    public static class Optional
    {
        /// <summary>
        /// Gets an empty value for assignment to optionals.
        /// </summary>
        public static EmptyOptional Empty { get { return new EmptyOptional(); } }

        /// <summary>
        /// Compares two optional values.
        /// </summary>
        /// <typeparam name="T">The types to compare.</typeparam>
        /// <param name="x">The first value to compare.</param>
        /// <param name="y">The second value to compare.</param>
        /// <returns>
        /// If x is less than y, a negative number.
        /// If y is less than x, a positive number.
        /// if x and y are equal, zero.
        /// 
        /// Optionals without values are sorted before optionals with values.
        /// When both values are present, the type's default comparer is used.
        /// </returns>
        public static int Compare<T>(Optional<T> x, Optional<T> y)
        {
            if (x.HasValue)
            {
                if (y.HasValue) return Comparer<T>.Default.Compare(x.Value, y.Value);
                return 1;
            }

            if (y.HasValue) return -1;

            return 0;
        }

        /// <summary>
        /// Compares two optional values for quality.
        /// </summary>
        /// <typeparam name="T">The types to compare.</typeparam>
        /// <param name="x">The first value to compare.</param>
        /// <param name="y">The second value to compare.</param>
        /// <returns>
        /// If both values are equal, returns true. When both values are present, the type's default comparer is used.
        /// </returns>
        public static bool Equals<T>(Optional<T> x, Optional<T> y)
        {
            if (x.HasValue)
            {
                return y.HasValue && EqualityComparer<T>.Default.Equals(x.Value, y.Value);
            }

            return !y.HasValue;
        }
    }

    /// <summary>
    /// An type with an optional value.
    /// </summary>
    /// <typeparam name="T">The type to make optional.</typeparam>
    /// <remarks>
    /// Similar usage to Nullable, but Optional works with reference types.
    /// </remarks>
    public struct Optional<T>
    {
        /// <summary>
        /// An empty optional.
        /// </summary>
        public static Optional<T> Empty { get { return new Optional<T>(); } }

        T val;
        bool hasVal;

        /// <summary>
        /// True, if the optional has a value.
        /// </summary>
        public bool HasValue { get { return hasVal; } }

        /// <summary>
        /// The value of the optional, or an InvalidOperationException if empty.
        /// </summary>
        public T Value
        {
            get
            {
                if (!hasVal)
                {
                    throw new InvalidOperationException("Optional has no value to get.");
                }

                return val;
            }
            set
            {
                val = value;
                hasVal = true;
            }
        }
        
        /// <summary>
        /// Creates an optional with a value set.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public Optional(T value)
        {
            val = value;
            hasVal = true;
        }

        /// <summary>
        /// Gets a string representing the optional.
        /// </summary>
        /// <returns>
        /// If the optional has a value and is non-null, calls the value's ToString().
        /// Otherwise, an empty string.
        /// </returns>
        public override string ToString()
        {
            return hasVal && val != null ? val.ToString() : string.Empty;
        }

        /// <summary>
        /// Gets a hash code for the optional.
        /// </summary>
        /// <returns>
        /// If the optional has a value and is non-null, calls the value's GetHashCode().
        /// Otherwise, zero.
        /// </returns>
        public override int GetHashCode()
        {
            return hasVal && val != null ? val.GetHashCode() : 0;
        }

        /// <summary>
        /// Determines if the optional is equal to the object.
        /// </summary>
        /// <param name="obj">The value to test.</param>
        /// <returns>
        /// If <paramref name="obj"/> is another Optional, will test that both are empty or have equal values.
        /// Otherwise, will test if Value equals the object.
        /// </returns>
        public override bool Equals(object obj)
        {
            var x = obj as Optional<T>?;

            if (!hasVal)
            {
                return x != null && !x.HasValue;
            }

            return val.Equals(x != null ? x.Value : obj);
        }

        /// <summary>
        /// Creates an optional with a value set.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }

        /// <summary>
        /// Creates an empty optional.
        /// </summary>
        /// <param name="value">A sentinel value for an empty optional.</param>
        public static implicit operator Optional<T>(EmptyOptional value)
        {
            return new Optional<T>();
        }

        /// <summary>
        /// Retrieves the value of the optional, or an InvalidOperationException if empty.
        /// </summary>
        public static explicit operator T(Optional<T> value)
        {
            return value.Value;
        }

        /// <summary>
        /// Retrieves the value of the optional, or the type's default value.
        /// </summary>
        public T GetValueOrDefault()
        {
            return val;
        }

        /// <summary>
        /// Retrieves the value of the optional, or the specified default value.
        /// </summary>
        public T GetValueOrDefault(T defaultValue)
        {
            return hasVal ? val : defaultValue;
        }
    }
}

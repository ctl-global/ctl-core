/*
    Copyright (c) 2014, CTL Global, Inc.
    Copyright (c) 2012, iD Commerce + Logistics
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification, are permitted
    provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions
    and the following disclaimer. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the documentation and/or other
    materials provided with the distribution.
 
    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
    IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
    FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
    CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
    CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
    THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
    OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
    POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ctl.Extensions
{
    /// <summary>
    /// Extension methods for the IDictionary type.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets or adds a value to a dictionary.
        /// </summary>
        /// <typeparam name="TKey">The dictionary's key type.</typeparam>
        /// <typeparam name="TValue">The dictionary's value type.</typeparam>
        /// <param name="dict">The dictionary to use.</param>
        /// <param name="key">The key to find.</param>
        /// <returns>An existing or newly inserted value pertaining to the specified key.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TValue : new()
        {
            if (dict == null) throw new ArgumentNullException("dict");
            if (key == null) throw new ArgumentNullException("key");

            TValue ret;

            if (!dict.TryGetValue(key, out ret))
            {
                dict[key] = ret = new TValue();
            }

            return ret;
        }

        /// <summary>
        /// Gets or adds a value to a dictionary, constructing the new object as needed.
        /// </summary>
        /// <typeparam name="TKey">The dictionary's key type.</typeparam>
        /// <typeparam name="TValue">The dictionary's value type.</typeparam>
        /// <param name="dict">The dictionary to use.</param>
        /// <param name="key">The key to find.</param>
        /// <param name="value">A value to insert for the key if one does not already exist.</param>
        /// <returns>An existing or newly inserted value pertaining to the specified key.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict == null) throw new ArgumentNullException("dict");
            if (key == null) throw new ArgumentNullException("key");

            TValue ret;

            if (!dict.TryGetValue(key, out ret))
            {
                dict[key] = ret = value;
            }

            return ret;
        }

        /// <summary>
        /// Gets or adds a value to a dictionary, constructing the new object as needed.
        /// </summary>
        /// <typeparam name="TKey">The dictionary's key type.</typeparam>
        /// <typeparam name="TValue">The dictionary's value type.</typeparam>
        /// <typeparam name="TOther">A type containing data used to create a <typeparamref name="TValue"/>.</typeparam>
        /// <param name="dict">The dictionary to use.</param>
        /// <param name="key">The key to find.</param>
        /// <param name="other">Data passed to <paramref name="createValue"/> to create a <typeparamref name="TValue"/>.</param>
        /// <param name="createValue">A function to create a value for the key if one does not already exist.</param>
        /// <returns>An existing or newly inserted value pertaining to the specified key.</returns>
        public static TValue GetOrAdd<TKey, TValue, TOther>(this IDictionary<TKey, TValue> dict, TKey key, TOther other, Func<TKey, TOther, TValue> createValue)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (createValue == null) throw new ArgumentNullException(nameof(createValue));

            TValue ret;

            if (!dict.TryGetValue(key, out ret))
            {
                dict[key] = ret = createValue(key, other);
            }

            return ret;
        }

        /// <summary>
        /// Gets or adds a value to a dictionary, constructing the new object as needed.
        /// </summary>
        /// <typeparam name="TKey">The dictionary's key type.</typeparam>
        /// <typeparam name="TValue">The dictionary's value type.</typeparam>
        /// <param name="dict">The dictionary to use.</param>
        /// <param name="key">The key to find.</param>
        /// <param name="createValue">A function to create a value for the key if one does not already exist.</param>
        /// <returns>An existing or newly inserted value pertaining to the specified key.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> createValue)
        {
            if (dict == null) throw new ArgumentNullException("dict");
            if (key == null) throw new ArgumentNullException("key");
            if (createValue == null) throw new ArgumentNullException("createValue");

            TValue ret;

            if (!dict.TryGetValue(key, out ret))
            {
                dict[key] = ret = createValue();
            }

            return ret;
        }

        /// <summary>
        /// Retrieves a key's value from a dictionary, returning the type's default value if it's not found.
        /// </summary>
        /// <typeparam name="TKey">The dictionary's key type.</typeparam>
        /// <typeparam name="TValue">The dictionary's value type.</typeparam>
        /// <param name="dict">The dictionary to use.</param>
        /// <param name="key">The key to find.</param>
        /// <returns>The value associated with the specified key, or default(TValue) if the key is not found.</returns>
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null) throw new ArgumentNullException("dict");
            if (key == null) throw new ArgumentNullException("key");

            TValue v;
            return dict.TryGetValue(key, out v) ? v : default(TValue);
        }

        /// <summary>
        /// Retrieves a key's value from a dictionary, returning the type's default value if it's not found.
        /// </summary>
        /// <typeparam name="TKey">The dictionary's key type.</typeparam>
        /// <typeparam name="TValue">The dictionary's value type.</typeparam>
        /// <param name="dict">The dictionary to use.</param>
        /// <param name="key">The key to find.</param>
        /// <param name="defaultValue">The default value to return in case the dictionary does not contain the key.</param>
        /// <returns>The value associated with the specified key, or <paramref name="defaultValue"/> if the key is not found.</returns>
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            if (dict == null) throw new ArgumentNullException("dict");
            if (key == null) throw new ArgumentNullException("key");

            TValue v;
            return dict.TryGetValue(key, out v) ? v : defaultValue;
        }

        /// <summary>
        /// Removes a range of items from a dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type of the dictionary.</typeparam>
        /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
        /// <param name="dict">The collection to remove from.</param>
        /// <param name="items">The items to remove.</param>
        /// <returns>An array of booleans indicating if the item at that index was found and removed.</returns>
        public static bool[] RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> items)
        {
            if (dict == null) throw new ArgumentNullException("dict");
            if (items == null) throw new ArgumentNullException("items");

            return items.Select(x => dict.Remove(x)).ToArray();
        }

        /// <summary>
        /// Removes a range of items from a dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type of the dictionary.</typeparam>
        /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
        /// <param name="dict">The collection to remove from.</param>
        /// <param name="items">The items to remove.</param>
        /// <returns>An array of booleans indicating if the item at that index was found and removed.</returns>
        public static bool[] RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, params TKey[] items)
        {
            if (dict == null) throw new ArgumentNullException("dict");
            if (items == null) throw new ArgumentNullException("items");

            return RemoveRange(dict, items.AsEnumerable());
        }
    }
}

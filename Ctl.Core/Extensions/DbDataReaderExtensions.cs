/*
    Copyright (c) 2014, CTL Global, Inc.
    Copyright (c) 2013, iD Commerce + Logistics
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
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for DbDataReader
    /// </summary>
    public static class DbDataReaderExtensions
    {
        /// <summary>
        /// Gets the value of the specified column, throwing an exception if the column is null.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="reader">The reader to get a value from.</param>
        /// <param name="name">The name of the reader.</param>
        /// <returns>The value of the column.</returns>
        public static T GetValue<T>(this DbDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);

            if (reader.IsDBNull(ordinal))
            {
                throw new InvalidCastException(string.Format("Unable to read value for '{0}', database returned null.", name));
            }

            return GetTypedValue<T>(reader, name, ordinal);
        }

        /// <summary>
        /// Gets the value of the specified column, or default(<typeparamref name="T"/>) if the column is null.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="reader">The reader to get a value from.</param>
        /// <param name="name">The name of the reader.</param>
        /// <returns>If the column is null, default(<typeparamref name="T"/>). Otherwise, the value of the column.</returns>
        public static T GetValueOrDefault<T>(this DbDataReader reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? default(T) : GetTypedValue<T>(reader, name, ordinal);
        }

        /// <summary>
        /// Gets the value of the specified column, or a default value if the column is null.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="reader">The reader to get a value from.</param>
        /// <param name="name">The name of the reader.</param>
        /// <param name="defaultValue">The default value to return if the column is null.</param>
        /// <returns>If the column is null, <paramref name="defaultValue"/>. Otherwise, the value of the column.</returns>
        public static T GetValueOrDefault<T>(this DbDataReader reader, string name, T defaultValue)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? defaultValue : GetTypedValue<T>(reader, name, ordinal);
        }

        /// <summary>
        /// Extracts a strongly-typed value from the reader, throwing an InvalidOperationException if the type is incorrect.
        /// </summary>
        static T GetTypedValue<T>(DbDataReader reader, string name, int ordinal)
        {
            try
            {
                return (T)reader.GetValue(ordinal);
            }
            catch (InvalidCastException ex)
            {
                Type expectedType = reader.GetFieldType(ordinal);
                throw new InvalidOperationException(string.Format("Unable to read value of type '{0}' from column '{1}'. Expected type '{2}'.", typeof(T), name, expectedType), ex);
            }
        }

        /// <summary>
        /// Enumerates POCO models from a database reader.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="reader">A database reader to read from.</param>
        /// <returns>A collection of POCO models.</returns>
        public static IEnumerable<T> AsPocoEnumerable<T>(this DbDataReader reader)
        {
            DbModelReader<T> r = new DbModelReader<T>();

            while (reader.Read())
            {
                yield return r.Read(reader);
            }
        }

        /// <summary>
        /// Asynchronously enumerates POCO models from a database reader.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="reader">A database reader to read from.</param>
        /// <returns>A collection of POCO models.</returns>
        public static IAsyncEnumerable<T> AsPocoEnumerableAsync<T>(this DbDataReader reader)
        {
            return AsyncEx.Create(
                () => new DbModelReader<T>(),
                (s, ct) => reader.ReadAsync(ct),
                s => s.Read(reader));
        }

        /// <summary>
        /// Reads POCO models from a database reader into a List.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="reader">A database reader to read from.</param>
        /// <returns>A collection of POCO models.</returns>
        public static List<T> ToPocoList<T>(this DbDataReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            List<T> list = new List<T>();
            DbModelReader<T> r = new DbModelReader<T>();

            while (reader.Read())
            {
                list.Add(r.Read(reader));
            }

            return list;
        }

        /// <summary>
        /// Asynchronously reads POCO models from a database reader into a List.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="reader">A database reader to read from.</param>
        /// <param name="token"></param>
        /// <returns>A collection of POCO models.</returns>
        public static async Task<List<T>> ToPocoList<T>(this DbDataReader reader, CancellationToken token)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            List<T> list = new List<T>();
            DbModelReader<T> r = new DbModelReader<T>();

            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                list.Add(r.Read(reader));
            }

            return list;
        }
    }
}

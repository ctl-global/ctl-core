/*
    Copyright (c) 2014, CTL Global, Inc.
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Reads POCO models from a database.
    /// </summary>
    /// <typeparam name="T">The data type to read.</typeparam>
    public sealed class DbModelReader<T>
    {
        static readonly string[] columnNames;
        static readonly ReadDbModelFunc<T> readFunc;

        /// <summary>
        /// Maps columnName[n] to DbDataReader[n].
        /// </summary>
        int[] indexes;

        static DbModelReader()
        {
            var initData = DbModelReader.CreateReadDelegate(typeof(T));
            readFunc = (ReadDbModelFunc<T>)initData.Item1.Compile();
            columnNames = initData.Item2;
        }

        /// <summary>
        /// Reads a POCO model from a database reader.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>A POCO model.</returns>
        public T Read(DbDataReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            if (indexes == null)
            {
                Init(reader);
            }

            return readFunc(reader, indexes);
        }

        void Init(DbDataReader reader)
        {
            Dictionary<string, int> nameIndexMap = new Dictionary<string, int>();

            for (int i = 0; i < reader.FieldCount; ++i)
            {
                string name = reader.GetName(i);

                if (name != null)
                {
                    nameIndexMap[name] = i;
                }
            }

            indexes = new int[columnNames.Length];

            for (int i = 0; i < columnNames.Length; ++i)
            {
                int idx;
                indexes[i] = nameIndexMap.TryGetValue(columnNames[i], out idx) ? idx : -1;
            }
        }
    }

    /// <summary>
    /// Utility methods for reading POCO models from a database.
    /// </summary>
    public static class DbModelReader
    {
        /// <summary>
        /// Executes a command and reads its first result set into a list.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="cmd">The command to execute and read from</param>
        /// <returns>A list of models.</returns>
        public static List<T> ReadAll<T>(DbCommand cmd)
        {
            List<T> list = new List<T>();
            var model = new DbModelReader<T>();

            if (cmd == null) throw new ArgumentNullException("cmd");

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(model.Read(reader));
                }
            }

            return list;
        }

        /// <summary>
        /// Reads all records available from the reader.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="reader">The reader to read from</param>
        /// <returns>A list of models.</returns>
        public static List<T> ReadAll<T>(DbDataReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            List<T> list = new List<T>();
            var model = new DbModelReader<T>();

            while (reader.Read())
            {
                list.Add(model.Read(reader));
            }

            return list;
        }

        /// <summary>
        /// Executes a command and reads its first result set into a list.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="cmd">The command to execute and read from</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A list of models.</returns>
        public static async Task<List<T>> ReadAllAsync<T>(DbCommand cmd, CancellationToken token)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            List<T> list = new List<T>();
            var model = new DbModelReader<T>();

            using(var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false))
            {
                while (await reader.ReadAsync(token).ConfigureAwait(false))
                {
                    list.Add(model.Read(reader));
                }
            }

            return list;
        }

        /// <summary>
        /// Reads all records available from the reader.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="reader">The reader to read from</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A list of models.</returns>
        public static async Task<List<T>> ReadAllAsync<T>(DbDataReader reader, CancellationToken token)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            List<T> list = new List<T>();
            var model = new DbModelReader<T>();

            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                list.Add(model.Read(reader));
            }

            return list;
        }

        /// <summary>
        /// Enumerates records available from the reader.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="reader">The reader to read from</param>
        /// <returns>A sequence of models.</returns>
        public static IEnumerable<T> Enumerate<T>(DbDataReader reader)
        {
            return new DbModelEnumerable<T>(reader);
        }

        /// <summary>
        /// Enumerates records available from the reader.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="reader">The reader to read from</param>
        /// <returns>A sequence of models.</returns>
        public static IAsyncEnumerable<T> EnumerateAsync<T>(DbDataReader reader)
        {
            return new DbModelEnumerable<T>(reader);
        }

        sealed class DbModelEnumerable<T> : IEnumerable<T>, IAsyncEnumerable<T>, IEnumerator<T>, IAsyncEnumerator<T>
        {
            readonly DbDataReader reader;
            readonly DbModelReader<T> model = new DbModelReader<T>();

            public DbModelEnumerable(DbDataReader reader)
            {
                if (reader == null) throw new ArgumentNullException("reader");
                this.reader = reader;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this;
            }

            IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
            {
                return this;
            }

            public T Current { get; private set; }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (reader.Read())
                {
                    Current = model.Read(reader);
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    Current = model.Read(reader);
                    return true;
                }

                return false;
            }
        }

        internal static Tuple<LambdaExpression, string[]> CreateReadDelegate(Type t)
        {
            List<Expression> readBody = new List<Expression>();

            ParameterExpression readerParam = Expression.Parameter(typeof(DbDataReader), "reader");
            ParameterExpression indexesParam = Expression.Parameter(typeof(int[]), "indexes");

            ParameterExpression valueVar = Expression.Variable(t, "value");
            ParameterExpression indexVar = Expression.Variable(typeof(int), "index");
            ConstantExpression negOne = Expression.Constant(-1);

            readBody.Add(Expression.Assign(valueVar, Expression.New(t)));

            var info = (from m in t.GetMembers()
                        where m.GetCustomAttribute<NotMappedAttribute>() == null
                        let p = m as PropertyInfo
                        let f = m as FieldInfo
                        where ((p != null && p.GetSetMethod() != null) || f != null)
                        let column = m.GetCustomAttribute<ColumnAttribute>()
                        select new
                        {
                            Member = m,
                            Name = column != null && column.Name != null ? column.Name : m.Name,
                            MemberType = p != null ? p.PropertyType : f.FieldType
                        }).ToArray();

            string[] columnNames = new string[info.Length];

            for (int i = 0; i < info.Length; ++i)
            {
                columnNames[i] = info[i].Name;

                Type memberBaseType = Nullable.GetUnderlyingType(info[i].MemberType) ?? info[i].MemberType;


                MethodInfo getMethod;

                if (!readerMethods.TryGetValue(memberBaseType, out getMethod))
                {
                    getMethod = typeof(DbDataReader).GetMethod("GetFieldValue").MakeGenericMethod(memberBaseType);
                }

                Expression getValue = Expression.Call(readerParam, getMethod, indexVar);

                if (memberBaseType != info[i].MemberType)
                {
                    getValue = Expression.New(info[i].MemberType.GetConstructor(new[] { memberBaseType }), getValue);
                }

                getValue = Expression.Assign(Expression.MakeMemberAccess(valueVar, info[i].Member), getValue);

                readBody.Add(Expression.Assign(indexVar, Expression.ArrayIndex(indexesParam, Expression.Constant(i))));

                if (info[i].Member.GetCustomAttribute<RequiredAttribute>() != null || (info[i].MemberType.IsValueType && (info[i].MemberType == memberBaseType)))
                {
                    // required field: throw if it's null or not returned.

                    Expression isMissing = Expression.Equal(indexVar, negOne);
                    Expression isNull = Expression.Call(readerParam, typeof(DbDataReader).GetMethod("IsDBNull"), indexVar);
                    Expression shouldThrow = Expression.OrElse(isMissing, isNull);
                    Expression exception = Expression.New(typeof(InvalidCastException).GetConstructor(new[] { typeof(string) }), Expression.Constant(string.Format("Required member '{0}' is missing or null from database.", info[i].Member.Name)));

                    readBody.Add(Expression.IfThen(shouldThrow, Expression.Throw(exception)));
                    readBody.Add(getValue);
                }
                else
                {
                    // type is nullable or a reference type, and not required. only read when available.

                    Expression isPresent = Expression.NotEqual(indexVar, negOne);
                    Expression isNotNull = Expression.Equal(Expression.Call(readerParam, typeof(DbDataReader).GetMethod("IsDBNull"), indexVar), Expression.Constant(false));
                    Expression shouldGetValue = Expression.AndAlso(isPresent, isNotNull);

                    readBody.Add(Expression.IfThen(shouldGetValue, getValue));
                }
            }

            readBody.Add(valueVar);

            BlockExpression body = Expression.Block(new[] { valueVar, indexVar }, readBody);

            Type delegateType = typeof(ReadDbModelFunc<>).MakeGenericType(t);
            LambdaExpression funcExp = Expression.Lambda(delegateType, body, readerParam, indexesParam);

            return Tuple.Create(funcExp, columnNames);
        }

        static readonly Dictionary<Type, MethodInfo> readerMethods = new Dictionary<Type, MethodInfo>
        {
            { typeof(bool), typeof(DbDataReader).GetMethod("GetBoolean") },
            { typeof(byte), typeof(DbDataReader).GetMethod("GetByte") },
            { typeof(char), typeof(DbDataReader).GetMethod("GetChar") },
            { typeof(DateTime), typeof(DbDataReader).GetMethod("GetDateTime") },
            { typeof(decimal), typeof(DbDataReader).GetMethod("GetDecimal") },
            { typeof(double), typeof(DbDataReader).GetMethod("GetDouble") },
            { typeof(float), typeof(DbDataReader).GetMethod("GetFloat") },
            { typeof(Guid), typeof(DbDataReader).GetMethod("GetGuid") },
            { typeof(short), typeof(DbDataReader).GetMethod("GetInt16") },
            { typeof(int), typeof(DbDataReader).GetMethod("GetInt32") },
            { typeof(long), typeof(DbDataReader).GetMethod("GetInt64") },
            { typeof(string), typeof(DbDataReader).GetMethod("GetString") }
        };
    }

    delegate T ReadDbModelFunc<T>(DbDataReader reader, int[] indexes);
}

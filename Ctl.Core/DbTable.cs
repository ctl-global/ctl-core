﻿/*
    Copyright (c) 2015, CTL Global, Inc.
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

using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Represents a user-defined table, used to create TVPs for SqlClient.
    /// </summary>
    public class DbTable
    {
        /// <summary>
        /// The SQL user-defined table type for the table.
        /// </summary>
        public string TypeName
        {
            get;
        }

        /// <summary>
        /// Column specifications for the table type.
        /// </summary>
        public SqlMetaData[] MetaData
        {
            get;
        }

        /// <summary>
        /// Initializes a new DbTable.
        /// </summary>
        /// <param name="typeName">The SQL type name for the user-defined table.</param>
        /// <param name="metaData">Column specifications for the table type.</param>
        public DbTable(string typeName, params SqlMetaData[] metaData)
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));
            if (metaData == null) throw new ArgumentNullException(nameof(metaData));

            this.TypeName = typeName;
            this.MetaData = metaData;
        }

        /// <summary>
        /// Defines a table-valued parameter.
        /// </summary>
        /// <param name="transform">An action that transforms a value into a SqlDataRecord.</param>
        /// <param name="records">Records to create a TVP for. May be null or empty if no records are available.</param>
        /// <remarks>
        /// This is deprecated because
        ///     (a) intellisense works a lot better when these two params are reversed, and
        ///     (b) it is now consistently overloaded with the typed version of DbTable.
        /// </remarks>
        [Obsolete, EditorBrowsable(EditorBrowsableState.Never)]
        public TableValuedParameter Create<T>(Action<SqlDataRecord, T> transform, IEnumerable<T> records)
        {
            return Create(records, transform);
        }

        /// <summary>
        /// Defines a table-valued parameter.
        /// </summary>
        /// <param name="transform">An action that transforms a value into a SqlDataRecord.</param>
        /// <param name="records">Records to create a TVP for. May be null or empty if no records are available.</param>
        public TableValuedParameter Create<T>(IEnumerable<T> records, Action<SqlDataRecord, T> transform)
        {
            if (transform == null) throw new ArgumentNullException(nameof(transform));

            return new TableValuedParameter(TypeName, records?.Any() == true ? Db.BuildTvp(MetaData, transform, records) : null);
        }
    }

    /// <summary>
    /// Represents a user-defined table, used to create TVPs for SqlClient.
    /// </summary>
    /// <typeparam name="T">The type th create TVPs for.</typeparam>
    public sealed class DbTable<T> : DbTable
    {
        readonly Action<SqlDataRecord, T> transform;

        /// <summary>
        /// Initializes a new DbTable.
        /// </summary>
        /// <param name="transform">A function to transform between a .NET object and a TVP row.</param>
        /// <param name="typeName">The SQL type name for the user-defined table.</param>
        /// <param name="metaData">Column specifications for the table type.</param>
        public DbTable(Action<SqlDataRecord, T> transform, string typeName, params SqlMetaData[] metaData) : base(typeName, metaData)
        {
            this.transform = transform;
        }

        /// <summary>
        /// Defines a table-valued parameter.
        /// </summary>
        /// <param name="records">Records to create a TVP for. May be null or empty if no records are available.</param>
        public TableValuedParameter Create(IEnumerable<T> records)
        {
            return base.Create(records, transform);
        }
    }
}

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

using Microsoft.SqlServer.Server;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Ctl.Extensions
{
    /// <summary>
    /// Extension methods for DbCommand.
    /// </summary>
    public static class DbCommandExtensions
    {
        /// <summary>
        /// Adds a parameter with a specific name and type to the command's parameter collection.
        /// </summary>
        /// <param name="cmd">The database command to add a parameter to.</param>
        /// <param name="name">The name of the parameter to create.</param>
        /// <param name="type">The type of the parameter's value.</param>
        /// <returns>The parameter which has been added to the command.</returns>
        public static DbParameter AddParameter(this DbCommand cmd, string name, DbType type)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (name == null) throw new ArgumentNullException("name");

            DbParameter p = cmd.CreateParameter();

            p.ParameterName = name;
            p.DbType = type;

            cmd.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds a parameter with a specific name and type to the command's parameter collection.
        /// </summary>
        /// <param name="cmd">The database command to add a parameter to.</param>
        /// <param name="name">The name of the parameter to create.</param>
        /// <param name="type">The type of the parameter's value.</param>
        /// <param name="size">The size of the parameter's value.</param>
        /// <returns>The parameter which has been added to the command.</returns>
        public static DbParameter AddParameter(this DbCommand cmd, string name, DbType type, int size)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (name == null) throw new ArgumentNullException("name");

            DbParameter p = cmd.CreateParameter();

            p.ParameterName = name;
            p.DbType = type;
            p.Size = size;

            cmd.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds a parameter with a specific name and type to the command's parameter collection.
        /// </summary>
        /// <param name="cmd">The database command to add a parameter to.</param>
        /// <param name="name">The name of the parameter to create.</param>
        /// <param name="type">The type of the parameter's value.</param>
        /// <param name="size">The size of the parameter's value.</param>
        /// <param name="sourceColumn">The source column name for use with DataSets.</param>
        /// <returns>The parameter which has been added to the command.</returns>
        public static DbParameter AddParameter(this DbCommand cmd, string name, DbType type, int size, string sourceColumn)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (name == null) throw new ArgumentNullException("name");

            DbParameter p = cmd.CreateParameter();

            p.ParameterName = name;
            p.DbType = type;
            p.Size = size;
            p.SourceColumn = sourceColumn;

            cmd.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds an output parameter with a specific name and type to the command's parameter collection.
        /// </summary>
        /// <param name="cmd">The database command to add a parameter to.</param>
        /// <param name="name">The name of the parameter to create.</param>
        /// <param name="type">Type type of the parameter's value.</param>
        /// <returns>The output parameter whcih has been added to the command.</returns>
        public static DbParameter AddOutputParameter(this DbCommand cmd, string name, DbType type)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (name == null) throw new ArgumentNullException("name");

            DbParameter p = cmd.CreateParameter();

            p.ParameterName = name;
            p.DbType = type;
            p.Direction = ParameterDirection.Output;

            cmd.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds an output parameter with a specific name and type to the command's parameter collection.
        /// </summary>
        /// <param name="cmd">The database command to add a parameter to.</param>
        /// <param name="name">The name of the parameter to create.</param>
        /// <param name="type">Type type of the parameter's value.</param>
        /// <param name="size">The size of the parameter's value.</param>
        /// <returns>The output parameter whcih has been added to the command.</returns>
        public static DbParameter AddOutputParameter(this DbCommand cmd, string name, DbType type, int size)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (name == null) throw new ArgumentNullException("name");

            DbParameter p = cmd.CreateParameter();

            p.ParameterName = name;
            p.DbType = type;
            p.Size = size;
            p.Direction = ParameterDirection.Output;

            cmd.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds a parameter with a specific name and value to the command's parameter collection.
        /// </summary>
        /// <param name="cmd">The database command to add a parameter to.</param>
        /// <param name="name">The name of the parameter to create.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The parameter which has been added to the command.</returns>
        public static DbParameter AddParameterWithValue(this DbCommand cmd, string name, object value)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (name == null) throw new ArgumentNullException("name");

            DbParameter p = cmd.CreateParameter();

            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;

            cmd.Parameters.Add(p);

            return p;
        }

        /// <summary>
        /// Adds a table-valued parameter to the command's parameter collection.
        /// </summary>
        /// <param name="cmd">The database command to add a parameter to.</param>
        /// <param name="name">The name of the parameter to create.</param>
        /// <param name="typeName">The name of table type to use.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The parameter which has been added to the command.</returns>
        public static SqlParameter AddParameterWithValue(this SqlCommand cmd, string name, string typeName, IEnumerable<SqlDataRecord> value)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (name == null) throw new ArgumentNullException("name");
            if (typeName == null) throw new ArgumentNullException("typeName");

            if (value?.Any() != true)
            {
                // do not add a parameter if there are no values.
                // TVPs default to an empty table if the parameter is not specified.
                return null;
            }

            SqlParameter p = cmd.CreateParameter();

            p.ParameterName = name;
            p.SqlDbType = SqlDbType.Structured;
            p.TypeName = typeName;
            p.Value = value;

            cmd.Parameters.Add(p);

            return p;
        }

        /// <summary>
        /// Adds fields and properties from a POCO type as command parameters.
        /// </summary>
        /// <typeparam name="T">The type containing parameter values to add.</typeparam>
        /// <param name="cmd">The database command to add a parameter to.</param>
        /// <param name="value">The object to create parameters from.</param>
        public static void AddParameters<T>(this DbCommand cmd, T value)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (value == null) throw new ArgumentNullException("value");

            DbDynamicMethods<T>.AddParameters(cmd, value);
        }
    }
}

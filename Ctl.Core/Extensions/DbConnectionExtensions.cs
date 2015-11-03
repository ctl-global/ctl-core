/*
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    /// <summary>
    /// Extension methods for DbConnection.
    /// </summary>
    public static class DbConnectionExtensions
    {
        /// <summary>
        /// Creates a new command.
        /// </summary>
        public static DbCommand CreateCommand(this DbConnection con, string commandText, CommandType commandType)
        {
            if (con == null) throw new ArgumentNullException(nameof(con));
            if (commandText == null) throw new ArgumentNullException(nameof(commandText));

            DbCommand cmd = con.CreateCommand();

            cmd.CommandText = commandText;
            cmd.CommandType = commandType;

            return cmd;
        }

        /// <summary>
        /// Creates a new command and initializes its parameters.
        /// </summary>
        public static DbCommand CreateCommand(this DbConnection con, string commandText, CommandType commandType, object parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            DbCommand cmd = con.CreateCommand(commandText, commandType);
            cmd.AddParameters(parameters);

            return cmd;
        }

        /// <summary>
        /// Executes a command.
        /// </summary>
        public static int Execute(this DbConnection con, string commandText, CommandType commandType)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a command.
        /// </summary>
        public static async Task<int> ExecuteAsync(this DbConnection con, string commandText, CommandType commandType, CancellationToken token)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType))
            {
                return await cmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a parameterized command.
        /// </summary>
        public static int Execute(this DbConnection con, string commandText, CommandType commandType, object parameters)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a parameterized command.
        /// </summary>
        public static async Task<int> ExecuteAsync(this DbConnection con, string commandText, CommandType commandType, CancellationToken token, object parameters)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType, parameters))
            {
                return await cmd.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a command.
        /// </summary>
        public static List<TRet> Execute<TRet>(this DbConnection con, string commandText, CommandType commandType)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType))
            {
                return Db.ReadAll<TRet>(cmd);
            }
        }

        /// <summary>
        /// Executes a command.
        /// </summary>
        public static async Task<List<TRet>> ExecuteAsync<TRet>(this DbConnection con, string commandText, CommandType commandType, CancellationToken token)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType))
            {
                return await Db.ReadAllAsync<TRet>(cmd, token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a parameterized command.
        /// </summary>
        public static List<TRet> Fetch<TRet>(this DbConnection con, string commandText, CommandType commandType, object parameters)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType, parameters))
            {
                return Db.ReadAll<TRet>(cmd);
            }
        }

        /// <summary>
        /// Executes a parameterized command.
        /// </summary>
        public static async Task<List<TRet>> ExecuteAsync<TRet>(this DbConnection con, string commandText, CommandType commandType, CancellationToken token, object parameters)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType, parameters))
            {
                return await Db.ReadAllAsync<TRet>(cmd, token).ConfigureAwait(false);
            }
        }
    }
}

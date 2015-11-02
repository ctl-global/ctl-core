using System;
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
        public static DbCommand CreateCommand<TParameters>(this DbConnection con, string commandText, CommandType commandType, TParameters parameters)
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
        public static int Execute<TParameters>(this DbConnection con, string commandText, CommandType commandType, TParameters parameters)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a parameterized command.
        /// </summary>
        public static async Task<int> ExecuteAsync<TParameters>(this DbConnection con, string commandText, CommandType commandType, CancellationToken token, TParameters parameters)
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
        public static List<TRet> Execute<TRet, TParameters>(this DbConnection con, string commandText, CommandType commandType, TParameters parameters)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType, parameters))
            {
                return Db.ReadAll<TRet>(cmd);
            }
        }

        /// <summary>
        /// Executes a parameterized command.
        /// </summary>
        public static async Task<List<TRet>> ExecuteAsync<TRet, TParameters>(this DbConnection con, string commandText, CommandType commandType, CancellationToken token, TParameters parameters)
        {
            using (DbCommand cmd = con.CreateCommand(commandText, commandType, parameters))
            {
                return await Db.ReadAllAsync<TRet>(cmd, token).ConfigureAwait(false);
            }
        }
    }
}

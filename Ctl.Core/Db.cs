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
using System.Data.Common;

namespace Ctl
{
    /// <summary>
    /// Factory methods to help with database connections.
    /// </summary>
    public static class Db
    {
        /// <summary>
        /// Creates a new connection using a connection string from the application's configuration.
        /// </summary>
        /// <param name="connectionStringName">The name of a connection string present in the application's configuration.</param>
        /// <returns>An unopened database connection.</returns>
        public static DbConnection NewConnection(string connectionStringName)
        {
            if (connectionStringName == null) throw new ArgumentNullException("connectionStringName");

            var cs = Config.ConnectionStrings[connectionStringName];
            var fact = DbProviderFactories.GetFactory(cs.ProviderName);

            DbConnection con = null;

            try
            {
                con = fact.CreateConnection();
                con.ConnectionString = cs.ConnectionString;
                return con;
            }
            catch
            {
                if (con != null)
                {
                    con.Dispose();
                }
                throw;
            }
        }

        /// <summary>
        /// Creates a new connection using a connection string from the application's configuration, and verifys that it supports a specific DbConnection class.
        /// </summary>
        /// <typeparam name="TConnection">The type of DbConnection class to use.</typeparam>
        /// <param name="connectionStringName">The name of a connection string present in the application's configuration.</param>
        /// <returns>An unopened database connection.</returns>
        public static TConnection NewConnection<TConnection>(string connectionStringName) where TConnection : DbConnection
        {
            if (connectionStringName == null) throw new ArgumentNullException("connectionStringName");

            DbConnection con = null;

            try
            {
                con = NewConnection(connectionStringName);
                
                TConnection ret = con as TConnection;

                if (ret == null)
                {
                    con.Dispose();
                    con = null;

                    throw new ArgumentException("Connection string with name \"" + connectionStringName + "\" was found, but did not match TConnection type.", "connectionStringName");
                }

                return ret;
            }
            catch
            {
                if (con != null)
                {
                    con.Dispose();
                }
                throw;
            }

        }
    }
}

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
using System.Configuration;
using System.Linq;

namespace Ctl
{
    /// <summary>
    /// Provides functionality similar to ConfigurationManager, but throws exceptions instead of returning null if values are not found.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Provides safe access to ConfigurationManager.AppSettings.
        /// </summary>
        public static AppSettingsCollection AppSettings { get { return AppSettingsCollection.Instance; } }

        /// <summary>
        /// Provides safe access to ConfigurationManager.ConnectionStrings.
        /// </summary>
        public static ConnectionStringsCollection ConnectionStrings { get { return ConnectionStringsCollection.Instance; } }

        public sealed class ConnectionStringsCollection : IReadOnlyDictionary<string, ConnectionStringSettings>
        {
            internal static readonly ConnectionStringsCollection Instance = new ConnectionStringsCollection();

            ConnectionStringsCollection()
            {
            }

            public bool ContainsKey(string key)
            {
                if (key == null) throw new ArgumentNullException("key");

                return ConfigurationManager.ConnectionStrings[key] != null;
            }

            public IEnumerable<string> Keys
            {
                get { return ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>().Select(x => x.Name); }
            }

            public bool TryGetValue(string key, out ConnectionStringSettings value)
            {
                if (key == null) throw new ArgumentNullException("key");

                value = ConfigurationManager.ConnectionStrings[key];
                return value != null;
            }

            public IEnumerable<ConnectionStringSettings> Values
            {
                get { return ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>(); }
            }

            public ConnectionStringSettings this[string key]
            {
                get
                {
                    if (key == null) throw new ArgumentNullException("key");

                    ConnectionStringSettings value;

                    if (TryGetValue(key, out value))
                    {
                        return value;
                    }

                    throw new KeyNotFoundException("Unable to find connection string \"" + key + "\".");
                }
            }

            public int Count
            {
                get { return ConfigurationManager.ConnectionStrings.Count; }
            }

            public IEnumerator<KeyValuePair<string, ConnectionStringSettings>> GetEnumerator()
            {
                foreach (var s in Values)
                {
                    yield return new KeyValuePair<string, ConnectionStringSettings>(s.Name, s);
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public sealed class AppSettingsCollection : IReadOnlyDictionary<string, string>
        {
            internal static readonly AppSettingsCollection Instance = new AppSettingsCollection();

            AppSettingsCollection()
            {
            }

            public bool ContainsKey(string key)
            {
                if (key == null) throw new ArgumentNullException("key");

                return ConfigurationManager.AppSettings[key] != null;
            }

            public IEnumerable<string> Keys
            {
                get { return ConfigurationManager.AppSettings.Keys.OfType<string>(); }
            }

            public bool TryGetValue(string key, out string value)
            {
                if (key == null) throw new ArgumentNullException("key");

                value = ConfigurationManager.AppSettings[key];
                return value != null;
            }

            public bool TryGetValues(string key, out string[] values)
            {
                if (key == null) throw new ArgumentNullException("key");

                values = ConfigurationManager.AppSettings.GetValues(key);
                return values != null;
            }

            public string[] GetValues(string key)
            {
                if (key == null) throw new ArgumentNullException("key");

                string[] values;

                if (TryGetValues(key, out values))
                {
                    return values;
                }

                throw new KeyNotFoundException("Unable to find application setting \"" + key + "\".");
            }

            public IEnumerable<string> Values
            {
                get
                {
                    foreach (var k in Keys)
                    {
                        yield return this[k];
                    }
                }
            }

            public string this[string key]
            {
                get
                {
                    if (key == null) throw new ArgumentNullException("key");

                    string value;

                    if (TryGetValue(key, out value))
                    {
                        return value;
                    }

                    throw new KeyNotFoundException("Unable to find application setting \"" + key + "\".");
                }
            }

            public int Count
            {
                get { return ConfigurationManager.AppSettings.Count; }
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                foreach (var k in Keys)
                {
                    yield return new KeyValuePair<string, string>(k, this[k]);
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}

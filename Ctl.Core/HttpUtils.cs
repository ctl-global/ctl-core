using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Utility methods for working with HTTP.
    /// </summary>
    public static class HttpUtils
    {
        /// <summary>
        /// Crates a basic authentication header value
        /// </summary>
        /// <param name="username">The user name to encode. Must not include the ':' character.</param>
        /// <param name="password">The password to encode.</param>
        /// <returns>An encoded string to use in basic auth.</returns>
        public static string CreateBasicAuthValue(string username, string password)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (username.IndexOf(':') != -1) throw new ArgumentException($"{nameof(username)} must not contain the ':' character.");

            return Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
        }

        /// <summary>
        /// Crates a basic authentication header value
        /// </summary>
        /// <param name="credential">The credential to encode.</param>
        /// <returns>An encoded string to use in basic auth.</returns>
        public static string CreateBasicAuthValue(NetworkCredential credential)
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));
            if (credential.Domain != null) throw new ArgumentException($"{nameof(credential)} must not specify a domain.");

            return CreateBasicAuthValue(credential.UserName, credential.Password);
        }
    }
}

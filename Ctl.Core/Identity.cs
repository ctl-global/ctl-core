/*
    Copyright (c) 2015, CTL Global, Inc.
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

using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;

namespace Ctl
{
    /// <summary>
    /// Provides utility methods for working with Windows identities.
    /// </summary>
    public static class Identity
    {
        /// <summary>
        /// Creates a WindowsIdentity instance for a user.
        /// </summary>
        /// <param name="domain">The domain to login to.</param>
        /// <param name="userName">The username to login to.</param>
        /// <param name="password">The password for the user.</param>
        /// <param name="type">The type of logon to use.</param>
        /// <returns>A WindowsIdentity pertaining to the user.</returns>
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static WindowsIdentity LogonUser(string domain, string userName, string password, LogonType type)
        {
            using(SafeFileHandle token = LogonUserImpl(domain, userName, password, type))
            {
                return new WindowsIdentity(token.DangerousGetHandle());
            }
        }

        /// <summary>
        /// Creates an impersonation context for a user.
        /// </summary>
        /// <param name="domain">The domain to login to. If null, <paramref name="userName"/> must have a full UPN-format (User@DNSDomainName) username.</param>
        /// <param name="userName">The username to login to.</param>
        /// <param name="password">The password for the user.</param>
        /// <param name="type">The type of logon to use.</param>
        /// <returns>An impersonation context pertaining to the user.</returns>
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static WindowsImpersonationContext Impersonate(string domain, string userName, string password, LogonType type)
        {
            using (SafeFileHandle token = LogonUserImpl(domain, userName, password, type))
            {
                return WindowsIdentity.Impersonate(token.DangerousGetHandle());
            }
        }

        static SafeFileHandle LogonUserImpl(string domain, string userName, string password, LogonType type)
        {
            if (userName == null) throw new ArgumentNullException("userName");
            if (password == null) throw new ArgumentNullException("password");

            try
            {
                SafeFileHandle token;

                if (!LogonUser(userName, domain, password, (int)type, LOGON32_PROVIDER_DEFAULT, out token))
                {
                    throw new Win32Exception();
                }

                return token;
            }
            catch (Win32Exception ex) when ((uint)ex.HResult == 0x80004005)
            {
                throw new UnauthorizedAccessException("Unable to logon with supplied credentials. See InnerException for details.", ex);
            }
        }

        const int LOGON32_PROVIDER_DEFAULT = 0;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool LogonUser([In] String lpszUsername, [In, Optional] String lpszDomain, [In, Optional] String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeFileHandle phToken);
    }

    /// <summary>
    /// Specifies properties for the logon process.
    /// </summary>
    public enum LogonType
    {
        /// <summary>
        /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on by a terminal server, remote shell, or similar process. This logon type has the additional expense of caching logon information for disconnected operations; therefore, it is inappropriate for some client/server applications, such as a mail server.
        /// </summary>
        Interactive = 2,

        /// <summary>
        /// This logon type is intended for high performance servers to authenticate plaintext passwords. Credentials will not be cached with this logon type.
        /// </summary>
        Network = 3,

        /// <summary>
        /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without their direct intervention. This type is also for higher performance servers that process many plaintext authentication attempts at a time, such as mail or web servers.
        /// </summary>
        Batch = 4,

        /// <summary>
        /// Indicates a service-type logon. The account provided must have the service privilege enabled.
        /// </summary>
        Service = 5,

        /// <summary>
        /// This logon type preserves the name and password in the authentication package, which allows the server to make connections to other network servers while impersonating the client. A server can accept plaintext credentials from a client, logon with NetworkCleartext to verify that the user can access the system across the network, and still communicate with other servers.
        /// </summary>
        NetworkCleartext = 8,

        /// <summary>
        /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections. The new logon session has the same local identifier but uses different credentials for other network connections.
        /// </summary>
        NewCredentials = 9
    }
}

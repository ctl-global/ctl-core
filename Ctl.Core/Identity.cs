/*
    Copyright (c) 2014, CTL Global, Inc.
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
        /// <returns>A WindowsIdentity pertaining to the user.</returns>
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static WindowsIdentity LogonUser(string domain, string userName, string password)
        {
            using(SafeFileHandle token = LogonUserImpl(domain, userName, password))
            {
                return new System.Security.Principal.WindowsIdentity(token.DangerousGetHandle());
            }
        }

        /// <summary>
        /// Creates an impersonation context for a user.
        /// </summary>
        /// <param name="domain">The domain to login to.</param>
        /// <param name="userName">The username to login to.</param>
        /// <param name="password">The password for the user.</param>
        /// <returns>An impersonation context pertaining to the user.</returns>
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static WindowsImpersonationContext Impersonate(string domain, string userName, string password)
        {
            using (SafeFileHandle token = LogonUserImpl(domain, userName, password))
            {
                return WindowsIdentity.Impersonate(token.DangerousGetHandle());
            }
        }

        static SafeFileHandle LogonUserImpl(string domain, string userName, string password)
        {
            if (userName == null) throw new ArgumentNullException("userName");
            if (password == null) throw new ArgumentNullException("password");

            SafeFileHandle token;
            
            if (!LogonUser(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out token))
            {
                throw new System.ComponentModel.Win32Exception();
            }

            return token;
        }

        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_INTERACTIVE = 2;

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool LogonUser([In] String lpszUsername, [In] String lpszDomain, [In] String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeFileHandle phToken);
    }
}

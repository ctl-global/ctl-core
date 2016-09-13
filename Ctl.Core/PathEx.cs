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

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Ctl
{
    /// <summary>
    /// Additional path helper methods.
    /// </summary>
    public static class PathEx
    {
        /// <summary>
        /// Finds the UNC path of a path.
        /// </summary>
        /// <param name="p">The path to convert.</param>
        /// <returns>A network-accessible (with proper permissions) UNC path.</returns>
        public static string GetUncPath(string p)
        {
            if (p == null) throw new ArgumentNullException("p");

            p = Path.GetFullPath(p);

            if (PathIsUNC(p))
            {
                return p;
            }

            int idx = p.IndexOf(':');

            if (idx == -1)
            {
                throw new Exception("Unable to find drive name.");
            }

            string unc = GetDriveUncPath(p.Substring(0, idx));

            return unc + p.Substring(idx + 1);
        }

        /// <summary>
        /// Finds the UNC path of a path.
        /// </summary>
        /// <param name="p">The path to convert.</param>
        /// <returns>A network-accessible (with proper permissions) UNC path.</returns>
        [Obsolete("Use PathEx.GetUncPath instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string ToUncPath(string p) => GetUncPath(p);

        [DllImport("shlwapi.dll", EntryPoint="PathIsUNC", CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PathIsUNCImpl([MarshalAs(UnmanagedType.LPWStr), In] string path);

        /// <summary>
        /// Determines if a path is a UNC path.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns>If the path is a UNC path, returns true. Otherwise, false.</returns>
        public static bool PathIsUNC(string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            return PathIsUNCImpl(path);
        }

        /// <summary>
        /// Gets the UNC path of a drive (ie. "C").
        /// </summary>
        /// <param name="drive">The drive to find the UNC path for.</param>
        /// <returns>The UNC path for the given drive letter.</returns>
        static string GetDriveUncPath(string drive)
        {
            if (drive.Any(x => !char.IsLetterOrDigit(x)))
            {
                throw new Exception("Drive names must be made up of letters and digits.");
            }

            WqlObjectQuery query = new WqlObjectQuery("SELECT ProviderName FROM Win32_LogicalDisk WHERE DeviceID='" + drive + ":'");

            ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2");
            scope.Options.Impersonation = ImpersonationLevel.Impersonate;
            scope.Options.EnablePrivileges = true;

            string providerName;

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                ManagementObject obj = searcher.Get().Cast<ManagementObject>().SingleOrDefault();

                if (obj == null)
                {
                    throw new Exception("Unable to find UNC path for drive.");
                }

                providerName = obj["ProviderName"] as string;
            }

            if (!string.IsNullOrEmpty(providerName))
            {
                return providerName;
            }

            return "\\\\" + Environment.MachineName + "\\" + drive + "$";
        }

        /// <summary>
        /// Converts a path to its long-path representation.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>A long path.</returns>
        /// <remarks>
        /// Many APIs will not accept long paths, YMMV.
        /// </remarks>
        public static string GetLongPath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (path.StartsWith(@"\\?\"))
            {
                return path;
            }

            if (PathIsUNC(path))
            {
                return @"\\?\UNC" + path.Substring(1);
            }

            path = Path.GetFullPath(path);

            return @"\\?\" + path;
        }
    }
}

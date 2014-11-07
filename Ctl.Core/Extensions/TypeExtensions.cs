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
using System.IO;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for Type.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Retrieves a stream to an embedded resource.
        /// </summary>
        /// <param name="type">The type who's assembly and namespace will be searched.</param>
        /// <param name="fileName">The file name of the embedded resource.</param>
        /// <returns>A stream to an embedded resource.</returns>
        public static Stream GetManifestResourceStream(this Type type, string fileName)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (fileName == null) throw new ArgumentNullException("fileName");

            return type.Assembly.GetManifestResourceStream(type, fileName);
        }

        /// <summary>
        /// Retrieves an embedded resource as a string.
        /// </summary>
        /// <param name="type">The type who's assembly and namespace will be searched.</param>
        /// <param name="fileName">The file name of the embedded resource.</param>
        /// <returns>A string value of the embedded resource.</returns>
        public static string GetManifestResourceString(this Type type, string fileName)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (fileName == null) throw new ArgumentNullException("fileName");

            Stream s = type.GetManifestResourceStream(fileName);

            if (s == null)
            {
                return null;
            }

            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}

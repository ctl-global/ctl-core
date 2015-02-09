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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Ctl.Security
{
    /// <summary>
    /// Creates HMAC-Based One-Time Passwords, as per RFC 4226.
    /// </summary>
    class Hotp : IDisposable
    {
        protected const string DefaultHmac = "HMACSHA1";

        readonly HMAC hmac;
        readonly long mod;

        /// <summary>
        /// Creates an instance of the Hotp class using the HMAC-SHA1 algorithm.
        /// </summary>
        /// <param name="digits">The number of digits in the password.</param>
        /// <param name="key">The secret shared key used to mix the password.</param>
        public Hotp(int digits, string key)
            : this(digits, Encoding.UTF8.GetBytes(key), HMAC.Create(DefaultHmac))
        {
        }

        /// <summary>
        /// Creates an instance of the Hotp class.
        /// </summary>
        /// <param name="digits">The number of digits in the password.</param>
        /// <param name="key">The secret shared key used to mix the password.</param>
        /// <param name="hmac">The base HMAC algorithm to use. Must produce at least 160 bits.</param>
        public Hotp(int digits, string key, HMAC hmac)
            : this(digits, Encoding.UTF8.GetBytes(key), hmac)
        {
        }

        /// <summary>
        /// Creates an instance of the Hotp class using the HMAC-SHA1 algorithm.
        /// </summary>
        /// <param name="digits">The number of digits in the password.</param>
        /// <param name="key">The secret shared key used to mix the password.</param>
        public Hotp(int digits, byte[] key)
            : this(digits, key, HMAC.Create(DefaultHmac))
        {
        }

        /// <summary>
        /// Creates an instance of the Hotp class.
        /// </summary>
        /// <param name="digits">The number of digits in the password.</param>
        /// <param name="key">The secret shared key used to mix the password.</param>
        /// <param name="hmac">The base HMAC algorithm to use. Must produce at least 160 bits.</param>
        public Hotp(int digits, byte[] key, HMAC hmac)
        {
            if (digits < 6) throw new ArgumentOutOfRangeException("digits", "One-time passwords must consist of at least 6 digits.");
            if (key == null) throw new ArgumentNullException("key");
            if (hmac.HashSize < 160) throw new ArgumentException("HMAC must produce at least 160 bits.", "hmac");

            this.hmac = hmac;
            this.hmac.Key = key;

            //this.mod = (long)BigInteger.Pow(10, Math.Min(digits, 10));
            this.mod = Convert.ToInt64(Math.Pow(10, Math.Min(digits, 10))); // accuracy should be fine for well above 10 digits...
        }

        /// <summary>
        /// Creates an OTP given a counter, returning the full-widh HMAC data.
        /// </summary>
        /// <param name="counter">The counter to return an OTP for.</param>
        /// <returns>A full-width OTP.</returns>
        public byte[] CreateFullOtp(long counter)
        {
            return CreateFullOtp(GetBigEndian(counter));
        }

        /// <summary>
        /// Creates an OTP given a counter, returning the full-widh HMAC data.
        /// </summary>
        /// <param name="counter">The counter to return an OTP for.</param>
        /// <returns>A full-width OTP.</returns>
        public byte[] CreateFullOtp(byte[] counter)
        {
            return hmac.ComputeHash(counter);
        }

        /// <summary>
        /// Creates an OTP given a counter.
        /// </summary>
        /// <param name="counter">The counter to return an OTP for.</param>
        /// <returns>An OTP.</returns>
        public int CreateOtp(long counter)
        {
            return CreateOtp(GetBigEndian(counter));
        }

        /// <summary>
        /// Creates an OTP given a counter.
        /// </summary>
        /// <param name="counter">The counter to return an OTP for.</param>
        /// <returns>An OTP.</returns>
        public int CreateOtp(byte[] counter)
        {
            byte[] hash = hmac.ComputeHash(counter);

            int offset = hash[hash.Length - 1] & 0xF;

            int code =
                ((hash[offset] & 0x7F) << 24) |
                ((int)hash[offset + 1] << 16) |
                ((int)hash[offset + 2] << 8) |
                (int)hash[offset + 3];

            return (int)(code % mod);
        }

        /// <summary>
        /// Gets a big-endian byte representation of a long.
        /// </summary>
        protected static byte[] GetBigEndian(long x)
        {
            ulong ux = (ulong)x;

            return new byte[]
            {
                (byte)(ux >> 56),
                (byte)(ux >> 48),
                (byte)(ux >> 40),
                (byte)(ux >> 32),
                (byte)(ux >> 24),
                (byte)(ux >> 16),
                (byte)(ux >> 8),
                (byte)x
            };
        }

        /// <summary>
        /// Disposes of the type.
        /// </summary>
        public void Dispose()
        {
            hmac.Dispose();
        }
    }
}

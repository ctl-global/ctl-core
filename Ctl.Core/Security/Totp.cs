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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Security
{
    /// <summary>
    /// Creates Time-Based One-Time Passwords, as per RFC 6238.
    /// </summary>
    class Totp : Hotp
    {
        /// <summary>
        /// DateTime.Ticks for Unix epoch.
        /// </summary>
        const long UnixEpoch = 621355968000000000;

        readonly long div;

        /// <summary>
        /// Creates an instance of the Totp class using the HMAC-SHA1 algorithm.
        /// </summary>
        /// <param name="digits">The number of digits in the password.</param>
        /// <param name="granularity">The time granularity to use.</param>
        /// <param name="key">The secret shared key used to mix the password.</param>
        public Totp(int digits, TimeSpan granularity, string key)
            : this(digits, granularity, key, HMAC.Create(DefaultHmac))
        {
        }

        /// <summary>
        /// Creates an instance of the Totp class using the HMAC-SHA1 algorithm.
        /// </summary>
        /// <param name="digits">The number of digits in the password.</param>
        /// <param name="granularity">The time granularity to use.</param>
        /// <param name="key">The secret shared key used to mix the password.</param>
        public Totp(int digits, TimeSpan granularity, byte[] key)
            : this(digits, granularity, key, HMAC.Create(DefaultHmac))
        {
        }

        /// <summary>
        /// Creates an instance of the Totp class.
        /// </summary>
        /// <param name="digits">The number of digits in the password.</param>
        /// <param name="granularity">The time granularity to use.</param>
        /// <param name="key">The secret shared key used to mix the password.</param>
        /// <param name="hmac">The base HMAC algorithm to use. Must produce at least 160 bits.</param>
        public Totp(int digits, TimeSpan granularity, string key, HMAC hmac)
            : base(digits, key, hmac)
        {
            this.div = granularity.Ticks;
        }

        /// <summary>
        /// Creates an instance of the Totp class.
        /// </summary>
        /// <param name="digits">The number of digits in the password.</param>
        /// <param name="granularity">The time granularity to use.</param>
        /// <param name="key">The secret shared key used to mix the password.</param>
        /// <param name="hmac">The base HMAC algorithm to use. Must produce at least 160 bits.</param>
        public Totp(int digits, TimeSpan granularity, byte[] key, HMAC hmac)
            : base(digits, key, hmac)
        {
            this.div = granularity.Ticks;
        }

        /// <summary>
        /// Creates an OTP given a date, returning the full-widh HMAC data.
        /// </summary>
        /// <param name="date">The date to return an OTP for.</param>
        /// <returns>A full-width OTP.</returns>
        public byte[] CreateFullOtp(DateTimeOffset date)
        {
            return CreateFullOtp(GetBigEndian(GetCounter(date)));
        }

        /// <summary>
        /// Creates an OTP given a date.
        /// </summary>
        /// <param name="date">The date to return an OTP for.</param>
        /// <returns>An OTP.</returns>
        public int CreateOtp(DateTimeOffset date)
        {
            return CreateOtp(GetBigEndian(GetCounter(date)));
        }

        /// <summary>
        /// Gets the counter value for a specific date.
        /// </summary>
        /// <param name="date">The date to retreive a counter for.</param>
        /// <returns>A counter for use in OTPs.</returns>
        public long GetCounter(DateTimeOffset date)
        {
            return (date.UtcTicks - UnixEpoch) / div;
        }
    }
}

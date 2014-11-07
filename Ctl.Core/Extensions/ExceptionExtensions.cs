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

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for Exception.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Flattens an AggregateException into a single exception, if possible.
        /// </summary>
        /// <param name="ex">The exception to flatten.</param>
        /// <returns>
        /// If the exception is an AggregateException with only a single inner exception, it will return that single exception.
        /// With more than one exception, it will return a flattened AggregateException.
        /// Otherwise, it returns the given exception unchanged.
        /// </returns>
        public static Exception ToSingle(this Exception ex)
        {
            if (ex == null) throw new ArgumentNullException("ex");

            AggregateException aex = ex as AggregateException;

            if (aex == null)
            {
                return ex;
            }

            aex = aex.Flatten();
            return aex.InnerExceptions.Count > 1 ? aex : aex.InnerExceptions[0];
        }

        /// <summary>
        /// Handles an exception of a specific type, returning what exceptions are left.
        /// </summary>
        /// <typeparam name="T">The type of exception to handle.</typeparam>
        /// <param name="ex">An exception to handle.</param>
        /// <param name="handler">A handler to call for each exception.</param>
        /// <returns>An AggregateException containing any remaining exceptions.</returns>
        public static AggregateException HandleEx<T>(this AggregateException ex, Func<T, bool> handler)
            where T : Exception
        {
            if (ex == null) throw new ArgumentNullException("ex");
            if (handler == null) throw new ArgumentNullException("handler");

            ex = ex.Flatten();

            try
            {
                ex.Handle(ex2 =>
                {
                    T ex3 = ex2 as T;

                    if (ex3 != null)
                    {
                        return handler(ex3);
                    }

                    return false;
                });
            }
            catch (AggregateException ex2)
            {
                return ex2;
            }

            return new AggregateException();
        }

        /// <summary>
        /// Handles any exceptions which weren't handled previously with HandleEx.
        /// </summary>
        /// <param name="ex">An exception to handle.</param>
        /// <param name="handler">A handler to call for each exception.</param>
        public static void HandleFallthrough(this AggregateException ex, Action<AggregateException> handler)
        {
            if (ex == null) throw new ArgumentNullException("ex");
            if (handler == null) throw new ArgumentNullException("handler");

            ex = ex.Flatten();

            if (ex.InnerExceptions.Count > 0)
            {
                handler(ex);
            }
        }

        /// <summary>
        /// Throws any exceptions which weren't handled previously with HandleEx.
        /// </summary>
        /// <param name="ex">The exception to throw.</param>
        public static void ThrowFallthrough(this AggregateException ex)
        {
            if (ex == null) throw new ArgumentNullException("ex");

            ex = ex.Flatten();

            if (ex.InnerExceptions.Count > 0)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Retrieves the win32 error code from an exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>A win32 error code</returns>
        /// <remarks>
        /// There is no guarantee an exception has an error code associated with it,
        /// so the return value of this method is undefinied for most exception types.
        /// </remarks>
        public static int Win32ErrorCode(this Exception ex)
        {
            if (ex == null) throw new ArgumentNullException("ex");

            Win32Exception wex = ex as Win32Exception;
            return wex != null ? wex.NativeErrorCode : ex.HResult & 0xFFFF;
        }
    }
}

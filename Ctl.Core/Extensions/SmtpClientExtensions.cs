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
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    public static class SmtpClientExtensions
    {
        /// <summary>
        /// Sends the specified message to a SMTP server for delivery as a cancellable asyncornous operation.
        /// </summary>
        /// <param name="client">The SmtpClient to send on.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="timeout">A timeout for the asynchronous operation.</param>
        /// <param name="token">A token for cancellation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SendMailAsync(this SmtpClient client, MailMessage message, TimeSpan timeout)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (message == null) throw new ArgumentNullException("message");

            using (CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout))
            {
                try
                {
                    await SendMailAsyncImpl(client, message, timeoutCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException ex)
                {
                    if (timeoutCts.IsCancellationRequested)
                    {
                        throw new SmtpException("A timeout expired during the SMTP send operation.", ex);
                    }

                    throw;
                }
            }
        }
        

        /// <summary>
        /// Sends the specified message to a SMTP server for delivery as a cancellable asyncornous operation.
        /// </summary>
        /// <param name="client">The SmtpClient to send on.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="timeout">A timeout for the asynchronous operation.</param>
        /// <param name="token">A token for cancellation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task SendMailAsync(this SmtpClient client, MailMessage message, TimeSpan timeout, CancellationToken token)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (message == null) throw new ArgumentNullException("message");

            return token.CanBeCanceled ? SendMailAsyncImpl(client, message, timeout, token) : SendMailAsync(client, message, timeout);
        }

        static async Task SendMailAsyncImpl(this SmtpClient client, MailMessage message, TimeSpan timeout, CancellationToken token)
        {
            Debug.Assert(client != null);
            Debug.Assert(message != null);

            using (CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout))
            using (CancellationTokenSource merged = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, token))
            {
                try
                {
                    await SendMailAsyncImpl(client, message, merged.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException ex)
                {
                    if (!token.IsCancellationRequested && timeoutCts.IsCancellationRequested)
                    {
                        throw new SmtpException("A timeout expired during the SMTP send operation.", ex);
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Sends the specified message to a SMTP server for delivery as a cancellable asyncornous operation.
        /// </summary>
        /// <param name="client">The SmtpClient to send on.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="token">A token for cancellation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task SendMailAsync(this SmtpClient client, MailMessage message, CancellationToken token)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (message == null) throw new ArgumentNullException("message");

            return token.CanBeCanceled ? SendMailAsyncImpl(client, message, token) : client.SendMailAsync(message);
        }

        static async Task SendMailAsyncImpl(SmtpClient client, MailMessage message, CancellationToken token)
        {
            Debug.Assert(client != null);
            Debug.Assert(message != null);

            token.ThrowIfCancellationRequested();

            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            SendCompletedEventHandler eh = null;

            eh = (s, e) =>
            {
                try
                {
                    client.SendCompleted -= eh;
                }
                finally
                {
                    eh = null;

                    TaskCompletionSource<int> tcs2 = (TaskCompletionSource<int>)e.UserState;

                    if (e.Error != null) tcs2.TrySetException(e.Error);
                    else if (e.Cancelled) tcs2.TrySetCanceled();
                    else tcs2.TrySetResult(0);
                }
            };

            client.SendCompleted += eh;

            try
            {
                client.SendAsync(message, tcs);
            }
            catch
            {
                if (eh != null)
                {
                    client.SendCompleted -= eh;
                }
                throw;
            }

            using (token.Register(o => ((SmtpClient)o).SendAsyncCancel(), client, false))
            {
                await tcs.Task.ConfigureAwait(false);
            }
        }
    }
}

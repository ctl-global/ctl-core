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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Provides utility methods for file access.
    /// </summary>
    public static class FileEx
    {
        /// <summary>
        /// Waits for a file to be unlocked, then opens it for asynchronous use.
        /// </summary>
        /// <param name="filePath">The path to monitor.</param>
        /// <param name="mode">The open mode that should be monitored for availability.</param>
        /// <param name="access">Access requirements for the file.</param>
        /// <param name="share">Share requirements for the file.</param>
        /// <param name="token">A cancellation token.</param>
        /// <param name="buffer">The buffer size.</param>
        /// <returns>A FileStream ready for asynchronous use.</returns>
        /// <remarks>Best used when it's possible a file will be locked by another app.</remarks>
        public static async Task<FileStream> OpenLocked(string filePath, FileMode mode, FileAccess access, FileShare share, CancellationToken token, int buffer = 4096, FileOptions options = FileOptions.None)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            Func<FileStream> func = () => new FileStream(filePath, mode, access, share, buffer, options | FileOptions.Asynchronous);

            int delay = 500;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    return await Task.Run(func).ConfigureAwait(false);
                }
                catch (IOException ex)
                {
                    if ((ex.HResult & 0xFFFF) != 32)
                    {
                        throw ex;
                    }
                }

                await Task.Delay(delay, token).ConfigureAwait(false);
                delay = delay + delay / 2; // exponential backoff.
            }
        }

        /// <summary>
        /// Opens a StreamReader for asynchronous use.
        /// </summary>
        /// <param name="filePath">The path of the file to open.</param>
        /// <returns>A StreamReader ready for asynchronous use.</returns>
        public static async Task<StreamReader> OpenLockedText(string filePath, CancellationToken token)
        {
            return new StreamReader(await OpenLocked(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, token, options: FileOptions.SequentialScan));
        }

        /// <summary>
        /// Copies a file asynchronously.
        /// </summary>
        /// <param name="srcFilePath">A path to the source file.</param>
        /// <param name="dstFilePath">A path to the destination file.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task Copy(string srcFilePath, string dstFilePath)
        {
            if (srcFilePath == null) throw new ArgumentNullException("srcFilePath");
            if (dstFilePath == null) throw new ArgumentNullException("dstFilePath");

            return Copy(srcFilePath, dstFilePath, CancellationToken.None);
        }

        /// <summary>
        /// Copies a file asynchronously.
        /// </summary>
        /// <param name="srcFilePath">A path to the source file.</param>
        /// <param name="dstFilePath">A path to the destination file.</param>
        /// <param name="token">A token which cancels the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task Copy(string srcFilePath, string dstFilePath, CancellationToken token)
        {
            if (srcFilePath == null) throw new ArgumentNullException("srcFilePath");
            if (dstFilePath == null) throw new ArgumentNullException("dstFilePath");

            using (Stream src = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous))
            using (Stream dst = new FileStream(dstFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous))
            {
                await src.CopyToAsync(dst, 4096, token).ConfigureAwait(false);
                await dst.FlushAsync(token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously loads a stream into a MemoryStream.
        /// </summary>
        /// <param name="src">The stream to load from.</param>
        /// <returns>A MemoryStream containing a copy of the source.</returns>
        /// <remarks>This is useful for hacking async support into non-async code.</remarks>
        public static Task<MemoryStream> LoadStream(Stream src)
        {
            return LoadStream(src, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously loads a stream into a MemoryStream.
        /// </summary>
        /// <param name="src">The stream to load from.</param>
        /// <param name="token">A token which cancels the operation.</param>
        /// <returns>A MemoryStream containing a copy of the source.</returns>
        /// <remarks>This is useful for hacking async support into non-async code.</remarks>
        public static async Task<MemoryStream> LoadStream(Stream src, CancellationToken token)
        {
            if (src == null) throw new ArgumentNullException("src");

            MemoryStream ms = new MemoryStream();

            await src.CopyToAsync(ms, 4096, token).ConfigureAwait(false);

            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Asynchronously loads a file into a MemoryStream.
        /// </summary>
        /// <param name="filePath">The file to load from.</param>
        /// <returns>A MemoryStream containing a copy of the file.</returns>
        /// <remarks>This is useful for hacking async support into non-async code that takes streams.</remarks>
        public static Task<MemoryStream> LoadStream(string filePath)
        {
            return LoadStream(filePath, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously loads a file into a MemoryStream.
        /// </summary>
        /// <param name="filePath">The file to load from.</param>
        /// <param name="token">A token which cancels the operation.</param>
        /// <returns>A MemoryStream containing a copy of the file.</returns>
        /// <remarks>This is useful for hacking async support into non-async code that takes streams.</remarks>
        public static async Task<MemoryStream> LoadStream(string filePath, CancellationToken token)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            using (Stream s = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous))
            {
                return await LoadStream(s, token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously reads all text from a file.
        /// </summary>
        /// <param name="filePath">The file to read from.</param>
        /// <returns>A string of all the text in the file.</returns>
        public static async Task<string> ReadAllText(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            using (Stream s = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous))
            using (StreamReader sr = new StreamReader(s))
            {
                return await sr.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Waits for a file to be available for open under certain parameters.
        /// </summary>
        /// <param name="path">The path to monitor.</param>
        /// <param name="mode">The open mode that should be monitored for availability.</param>
        /// <param name="access">Access requirements for the file.</param>
        /// <param name="share">Share requirements for the file.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>There are no guarantees that the file will remain unlocked at the time the task has completed.</remarks>
        public static Task WaitForUnlock(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return WaitForUnlock(path, mode, access, share, CancellationToken.None);
        }

        /// <summary>
        /// Waits for a file to be unlocked.
        /// </summary>
        /// <param name="filePath">The path to monitor.</param>
        /// <param name="mode">The open mode that should be monitored for availability.</param>
        /// <param name="access">Access requirements for the file.</param>
        /// <param name="share">Share requirements for the file.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>There are no guarantees that the file will remain unlocked at the time the task has completed.</remarks>
        public static async Task WaitForUnlock(string filePath, FileMode mode, FileAccess access, FileShare share, CancellationToken token)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            (await OpenLocked(filePath, mode, access, share, token).ConfigureAwait(false)).Dispose();
        }

        /// <summary>
        /// Waits for a file's size to stop changing.
        /// </summary>
        /// <param name="filePath">The file's path.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        public static async Task WaitForStabilization(string filePath, CancellationToken token)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            FileInfo fi = new FileInfo(filePath);

            fi.Refresh();
            long prevSize = fi.Length;
            int sleepTime = 5000;

            Stopwatch sw = new Stopwatch();

            while (true)
            {
                await Task.Delay(sleepTime, token).ConfigureAwait(false);

                if (sleepTime < 60000)
                {
                    sleepTime = sleepTime + sleepTime / 2;
                }

                fi.Refresh();

                if (prevSize == fi.Length)
                {
                    return;
                }

                prevSize = fi.Length;
            }
        }
    }
}

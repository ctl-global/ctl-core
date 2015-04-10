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
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Security
{
    /// <summary>
    /// Implements an Elliptic Curve Integrated Encryption Scheme.
    /// This is a one-way encryption method that provides optional message authentication.
    /// </summary>
    /// <remarks>
    /// This has a fixed overhead of 84 bytes, and a small padding for odd-length messages.
    /// Fixed overhead could be reduced to 52 bytes if .NET ever gets the ability to reconstruct ECDH keys.
    /// </remarks>
    public static class ECIES
    {
        /// <summary>
        /// Encrypts data with authentication against the source's key.
        /// </summary>
        /// <param name="sourcePrivateKey">The source's private key used to authenticate the message.</param>
        /// <param name="targetPublicKey">The target's public key used to encrypt the message.</param>
        /// <param name="data">The data tn encrypt.</param>
        /// <param name="offset">The offset of the first byte in the data to encrypt.</param>
        /// <param name="length">The length of the data to encrypt.</param>
        /// <returns>The encrypted message.</returns>
        public static byte[] AuthenticatedEncrypt(ECDiffieHellmanCng sourcePrivateKey, ECDiffieHellmanPublicKey targetPublicKey, byte[] data, int offset, int length)
        {
            if (sourcePrivateKey == null) throw new ArgumentNullException("sourcePrivateKey");
            if (targetPublicKey == null) throw new ArgumentNullException("targetPublicKey");

            byte[] sharedAuthKey = sourcePrivateKey.DeriveKeyMaterial(targetPublicKey);
            return EncryptImpl(targetPublicKey, sharedAuthKey, data, offset, length);
        }

        /// <summary>
        /// Encrypts data.
        /// </summary>
        /// <param name="targetPublicKey">The public key of the target of the message.</param>
        /// <param name="data">The data tn encrypt.</param>
        /// <param name="offset">The offset of the first byte in the data to encrypt.</param>
        /// <param name="length">The length of the data to encrypt.</param>
        /// <returns>The encrypted message.</returns>
        public static byte[] Encrypt(ECDiffieHellmanPublicKey targetPublicKey, byte[] data, int offset, int length)
        {
            return EncryptImpl(targetPublicKey, null, data, offset, length);
        }

        static byte[] EncryptImpl(ECDiffieHellmanPublicKey targetPublicKey, byte[] sharedAuthKey, byte[] data, int offset, int length)
        {
            if (targetPublicKey == null) throw new ArgumentNullException("targetPublicKey");
            if (data == null) throw new ArgumentNullException("data");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset");
            if (length < 0 || checked(offset + length) > data.Length) throw new ArgumentOutOfRangeException("length");

            // generate an ephemeral key and shared secret.
            // note: the first 8 bytes of ephemeralPublicKey are constant to indicate blob format;
            // we don't include these in the message since we know the format on the other end already.

            byte[] ephemeralPublicKey, sharedSecret;

            using (var ephemeralKey = new ECDiffieHellmanCng(256))
            {
                ephemeralKey.HashAlgorithm = CngAlgorithm.Sha512;

                ephemeralPublicKey = ephemeralKey.PublicKey.ToByteArray();
                sharedSecret = ephemeralKey.DeriveKeyMaterial(targetPublicKey);
            }

            Debug.Assert(ephemeralPublicKey.Length == 72);
            Debug.Assert(sharedSecret.Length == 64);

            // encrypt the message.

            byte[] encrypted;

            byte[] key = new byte[16];
            Array.Copy(sharedSecret, 0, key, 0, 16);

            byte[] iv = new byte[16];
            Array.Copy(sharedSecret, 16, iv, 0, 16);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (ICryptoTransform enc = aes.CreateEncryptor())
                {
                    encrypted = enc.TransformFinalBlock(data, offset, length);
                }
            }

            // calculate HMAC.

            byte[] mac;

            byte[] hmacKey = new byte[32 + (sharedAuthKey != null ? sharedAuthKey.Length : 0)];
            Array.Copy(sharedSecret, 32, hmacKey, 0, 32);

            if (sharedAuthKey != null)
            {
                Array.Copy(sharedAuthKey, 0, hmacKey, 32, sharedAuthKey.Length);
            }

            using (HMAC hmac = HMAC.Create())
            {
                hmac.Key = hmacKey;
                mac = hmac.ComputeHash(encrypted, 0, encrypted.Length);
            }

            Debug.Assert(mac.Length == 20);

            // output public key + hmac + encrypted data.

            byte[] msg = new byte[84 + encrypted.Length];

            Array.Copy(ephemeralPublicKey, 8, msg, 0, 64);
            Array.Copy(mac, 0, msg, 64, 20);
            Array.Copy(encrypted, 0, msg, 84, encrypted.Length);

            return msg;
        }

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="targetPrivateKey">The target's private key used to decrypt the message.</param>
        /// <param name="sourcePublicKey">The source's public key used to authenticate the message.</param>
        /// <param name="data">The data tn decrypt.</param>
        /// <param name="offset">The offset of the first byte in the data to decrypt.</param>
        /// <param name="length">The length of the data to decrypt.</param>
        /// <returns>The decrypted message.</returns>
        public static byte[] AuthenticatedDecrypt(ECDiffieHellmanCng targetPrivateKey, ECDiffieHellmanPublicKey sourcePublicKey, byte[] data, int offset, int length)
        {
            if (targetPrivateKey == null) throw new ArgumentNullException("targetPrivateKey");
            if (sourcePublicKey == null) throw new ArgumentNullException("sourcePublicKey");

            byte[] sharedAuthKey = targetPrivateKey.DeriveKeyMaterial(sourcePublicKey);
            return DecryptImpl(targetPrivateKey, sharedAuthKey, data, offset, length);
        }

        /// <summary>
        /// Decrypts data.
        /// </summary>
        /// <param name="targetPrivateKey">The private key used to decrypt the message.</param>
        /// <param name="data">The data tn decrypt.</param>
        /// <param name="offset">The offset of the first byte in the data to decrypt.</param>
        /// <param name="length">The length of the data to decrypt.</param>
        /// <returns>The decrypted message.</returns>
        public static byte[] Decrypt(ECDiffieHellmanCng targetPrivateKey, byte[] data, int offset, int length)
        {
            return DecryptImpl(targetPrivateKey, null, data, offset, length);
        }

        static byte[] DecryptImpl(ECDiffieHellmanCng targetPrivateKey, byte[] sharedAuthKey, byte[] data, int offset, int length)
        {
            if (targetPrivateKey == null) throw new ArgumentNullException("targetPrivateKey");
            if (data == null) throw new ArgumentNullException("data");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset");
            if (length < 0 || checked(offset + length) > data.Length) throw new ArgumentOutOfRangeException("length");
            if (length < 84) throw new ArgumentOutOfRangeException("length", length, "Length is not large enough to fit an encrypted message.");

            // calculate the shared secret.
            byte[] sharedSecret;

            byte[] ephemeralPublicKeyBlob = new byte[72];

            // this is the blob format identifier.

            ephemeralPublicKeyBlob[0] = 69;
            ephemeralPublicKeyBlob[1] = 67;
            ephemeralPublicKeyBlob[2] = 75;
            ephemeralPublicKeyBlob[3] = 49;
            ephemeralPublicKeyBlob[4] = 32;
            ephemeralPublicKeyBlob[5] = 0;
            ephemeralPublicKeyBlob[6] = 0;
            ephemeralPublicKeyBlob[7] = 0;

            Array.Copy(data, offset, ephemeralPublicKeyBlob, 8, 64);

            using (ECDiffieHellmanPublicKey ephemeralPublicKey = ECDiffieHellmanCngPublicKey.FromByteArray(ephemeralPublicKeyBlob, CngKeyBlobFormat.EccPublicBlob))
            {
                // I really don't like mutating this.
                targetPrivateKey.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                targetPrivateKey.HashAlgorithm = CngAlgorithm.Sha512;

                sharedSecret = targetPrivateKey.DeriveKeyMaterial(ephemeralPublicKey);
            }

            // validate HMAC.

            byte[] mac;

            byte[] hmacKey = new byte[32 + (sharedAuthKey != null ? sharedAuthKey.Length : 0)];
            Array.Copy(sharedSecret, 32, hmacKey, 0, 32);

            if (sharedAuthKey != null)
            {
                Array.Copy(sharedAuthKey, 0, hmacKey, 32, sharedAuthKey.Length);
            }

            using (HMAC hmac = HMAC.Create())
            {
                hmac.Key = hmacKey;
                mac = hmac.ComputeHash(data, offset + 84, data.Length - 84);
            }

            if (!mac.SequenceEqual(data.Skip(offset + 64).Take(20)))
            {
                throw new Exception(sharedAuthKey == null ? "Encrypted message is corrupt." : "Encrypted message is corrupt or failed authentication.");
            }

            // decrypt the message.

            byte[] key = new byte[16];
            Array.Copy(sharedSecret, 0, key, 0, 16);

            byte[] iv = new byte[16];
            Array.Copy(sharedSecret, 16, iv, 0, 16);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (ICryptoTransform dec = aes.CreateDecryptor())
                {
                    return dec.TransformFinalBlock(data, 84, length - 84);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ctl.Extensions;
using Ctl.Validation;
using System.Security.Cryptography;

namespace Ctl.Core.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = new ECDiffieHellmanCng(256);
            var target = new ECDiffieHellmanCng(256);

            byte[] orig = Encoding.UTF8.GetBytes("Hello, world!");
            byte[] enc = Security.ECIES.AuthenticatedEncrypt(source, target.PublicKey, orig, 0, orig.Length);
            byte[] dec = Security.ECIES.AuthenticatedDecrypt(target, source.PublicKey, enc, 0, enc.Length);
            string decstr = Encoding.UTF8.GetString(dec);

            Console.WriteLine("Original:       {0}", Convert.ToBase64String(orig));
            Console.WriteLine("Encoded:        {0}", Convert.ToBase64String(enc));
            Console.WriteLine("Decoded:        {0}", Convert.ToBase64String(dec));
            Console.WriteLine("Decoded String: {0}", decstr);
        }
    }
}

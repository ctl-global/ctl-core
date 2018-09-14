using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ctl.Core.Tests
{
    [TestClass]
    public class FarmHashTests
    {
        [TestMethod]
        public void TestHash0() => TestHash("");
        [TestMethod]
        public void TestHash2() => TestHash("ab");
        [TestMethod]
        public void TestHash6() => TestHash("abcdef");
        [TestMethod]
        public void TestHash14() => TestHash("abcdefghijklmn");
        [TestMethod]
        public void TestHash30() => TestHash("Duis sed nisi ex orci aliquam.");
        [TestMethod]
        public void TestHash62() => TestHash("Curabitur et metus sodales, aliquet eros vel, suscipit nullam.");
        [TestMethod]
        public void TestHash100() => TestHash("In eu dolor quam. In vel sapien consequat, cursus purus vitae, aliquet ipsum. Nullam congue posuere.");

        static void TestHash(string data)
        {
            long hash1 = HashUtils.FarmHash64(data.AsSpan());
            long hash2 = BitConverter.ToInt64(System.Data.HashFunction.FarmHash.FarmHashFingerprint64Factory.Instance.Create().ComputeHash(Encoding.UTF8.GetBytes(data)).Hash, 0);
            Assert.AreEqual(hash1, hash2);
        }

    }
}

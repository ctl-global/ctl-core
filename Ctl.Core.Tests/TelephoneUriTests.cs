using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ctl.Core.Tests
{
    [TestClass]
    public class TelephoneUriTests
    {
        [TestMethod]
        public void TestGlobal()
        {
            var uri = new TelephoneUri("tel:+1-123-456-7890");

            Assert.IsTrue(uri.IsGlobalNumber == true);
            Assert.AreEqual("+1-123-456-7890", uri.Number);
        }

        [TestMethod]
        public void TestLocal()
        {
            var uri = new TelephoneUri("tel:7890;phone-context=+1-123-456-");

            Assert.IsTrue(uri.IsGlobalNumber == false);
            Assert.AreEqual("7890", uri.Number);
            Assert.AreEqual("+1-123-456-", uri.LocalContext);
        }

        [TestMethod]
        public void TestLocalContext()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new TelephoneUri("tel:+7890;phone-context=+1234");
            });

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new TelephoneUri("tel:7890");
            });
        }

        public void TestExtension()
        {
            var uri = new TelephoneUri("tel:+1;ext=123");

            Assert.AreEqual(uri.Extension, "123");
        }

        public void TestIsdnSubaddress()
        {
            var uri = new TelephoneUri("tel:+1;isub=123");

            Assert.AreEqual(uri.IsdnSubaddress, "123");
        }

        public void TestExtensionAndIsdnSubaddress()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new TelephoneUri("tel:+1;ext=123;isub=123");
            });
        }
    }
}

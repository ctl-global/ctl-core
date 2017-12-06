using Ctl.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Ctl.Core.Tests
{
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        public void TestSplitToLines()
        {
            string lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In velit mi, congue sed orci et, tempus accumsan neque. Aliquam luctus risus vel congue condimentum. Integer congue felis eget egestas imperdiet. Praesent mi ex, ornare a lorem tristique, euismod consectetur mauris. Donec mollis eget nulla sagittis auctor. Donec orci dui, consequat.";
            string[] lines = lipsum.SplitToLines(50).ToArray();

            Assert.AreEqual(lines.Length, 8);
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipiscing", lines[0]);
            Assert.AreEqual("elit. In velit mi, congue sed orci et, tempus", lines[1]);
            Assert.AreEqual("accumsan neque. Aliquam luctus risus vel congue", lines[2]);
            Assert.AreEqual("condimentum. Integer congue felis eget egestas", lines[3]);
            Assert.AreEqual("imperdiet. Praesent mi ex, ornare a lorem", lines[4]);
            Assert.AreEqual("tristique, euismod consectetur mauris. Donec", lines[5]);
            Assert.AreEqual("mollis eget nulla sagittis auctor. Donec orci dui,", lines[6]);
            Assert.AreEqual("consequat.", lines[7]);
        }

        [TestMethod]
        public void TestSplitToLinesEllipsis()
        {
            string lipsum = "foobarbaz";
            string[] lines = lipsum.SplitToLines(5, true).ToArray();

            Assert.AreEqual(3, lines.Length);
            Assert.AreEqual("foo…", lines[0]);
            Assert.AreEqual("…bar…", lines[1]);
            Assert.AreEqual("…baz", lines[2]);
        }

        [TestMethod]
        public void TestSplitToLinesNoEllipsis()
        {
            Assert.ThrowsException<Exception>(() =>
            {
                string lipsum = "foobarbaz";
                string[] lines = lipsum.SplitToLines(5, false).ToArray();
            });
        }
    }
}

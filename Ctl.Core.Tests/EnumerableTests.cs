using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Ctl.Extensions;
using System.Diagnostics;

namespace Ctl.Core.Tests
{
    [TestClass]
    public class EnumerableTests
    {
        [TestMethod]
        public void TestFullJoin()
        {
            var aCollection = new[]
            {
                new { Id = 1, Name = "A" },
                new { Id = 2, Name = "B" },
                new { Id = 3, Name = "C" },
                new { Id = 3, Name = "D" },
                new { Id = 4, Name = "E" }
            };

            var bCollection = new[]
            {
                new { Id = 2, Name = "F" },
                new { Id = 3, Name = "G" },
                new { Id = 4, Name = "H" },
                new { Id = 4, Name = "I" },
                new { Id = 5, Name = "J" }
            };

            var joined = aCollection
                .FullOuterJoin(bCollection, a => a.Id, b => b.Id, (a, b) => new { a, b })
                .OrderBy(x => x.a?.Id ?? -1)
                .ThenBy(x => x.b?.Id ?? -1)
                .ToArray();

            Assert.AreEqual(7, joined.Length);

            Assert.IsNull(joined[0].a);
            Assert.AreEqual("J", joined[0].b.Name);

            Assert.AreEqual("A", joined[1].a.Name);
            Assert.IsNull(joined[1].b);

            Assert.AreEqual("B", joined[2].a.Name);
            Assert.AreEqual("F", joined[2].b.Name);

            Assert.AreEqual("C", joined[3].a.Name);
            Assert.AreEqual("G", joined[3].b.Name);

            Assert.AreEqual("D", joined[4].a.Name);
            Assert.AreEqual("G", joined[4].b.Name);

            Assert.AreEqual("E", joined[5].a.Name);
            Assert.AreEqual("H", joined[5].b.Name);

            Assert.AreEqual("E", joined[6].a.Name);
            Assert.AreEqual("I", joined[6].b.Name);
        }
    }
}

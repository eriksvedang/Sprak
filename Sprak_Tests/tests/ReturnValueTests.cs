using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgrammingLanguageNr1;
using NUnit.Framework;
namespace Sprak_Tests
{
    [TestFixture]
    public class ReturnValueTests
    {
        [Test]
        public void PackUnpackFloat()
        {
            ReturnValue rv = new ReturnValue(ReturnValueType.NUMBER, 32.4f);
            Assert.AreEqual(32.4f, (float)rv.Unpack(), 0.0001f);
            Assert.AreEqual(32, Convert.ToInt32(rv.Unpack()));
        }
        [Test]
        public void PackUnpackInt()
        {
            ReturnValue rv = new ReturnValue(ReturnValueType.NUMBER, 32);
            Assert.AreEqual(32.0f, (float)rv.Unpack(), 0.0001f);
            Assert.AreEqual(32, Convert.ToInt32(rv.Unpack()));
        }

    
    }
}

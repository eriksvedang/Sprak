using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgrammingLanguageNr1;
using NUnit.Framework;

namespace ProgrammingLanguageNr1.tests
{
    [TestFixture]
    public class ReturnValue_TEST
    {
        [Test]
        public void PackUnpackFloat()
        {
            ReturnValue returnValue = new ReturnValue(ReturnValueType.NUMBER, 32.4f);
            Assert.AreEqual(32.4f, (float)returnValue.Unpack(), 0.0001f);
            Assert.AreEqual(32, Convert.ToInt32(returnValue.Unpack()));
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

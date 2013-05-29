using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using ProgrammingLanguageNr1;
using System.Reflection;

namespace ProgrammingLanguageNr1.tests
{
	public class DemoClass
	{
		[SprakAPI("returns two values", "floatA", "second float")]
		public int API_GetValues(int pFloatA, int pFloatB) { return (int)(pFloatA * pFloatB); }

		[SprakAPI("werw", "sdf")]
		public bool API_UseBool(bool pFloatA) { return pFloatA; }

		//no api
		public string API_NumberToString(float pFloat) { return pFloat.ToString(); }
	}

    public class DemoClassTwo
    {
        [SprakAPI("returns two values", "floatA", "second float")]
        public int[] API_GetValues(int pFloatA, int pFloatB) { return null; }

        [SprakAPI("werw", "sdf")]
        public bool[] API_UseBool(bool pFloatA) { return null; }

        //no api
        public string[] API_NumberToString(float pFloat) { return null; }

    }
    
    [TestFixture]
    public class FunctionDefinitionCreator_TEST
    {
        [Test]
        public void BasicUsage()
        {
            DemoClass dc = new DemoClass();
            FunctionDefinition[] defs = FunctionDefinitionCreator.CreateDefinitions(dc, typeof(DemoClass));
            foreach (FunctionDefinition fd in defs)
            {
                if (fd.functionName == "NumberToString")
                {
                    ReturnValue rv = fd.callback(new ReturnValue[]{ new ReturnValue(ReturnValueType.NUMBER, 1.5f)});
                    Assert.AreEqual((1.5f).ToString(), rv.StringValue);
                }
                if (fd.functionName == "GetValues")
                {
                    Console.WriteLine("retval " + fd.functionDocumentation.GetFunctionDescription() + ", " + 
                        fd.functionDocumentation.GetArgumentDescription(0) + 
                        ", " + fd.functionDocumentation.GetArgumentDescription(1));
                    ReturnValue rv = fd.callback(new ReturnValue[] { new ReturnValue(3.0f), new ReturnValue(4.0f)});
                    Assert.AreEqual(12f, rv.NumberValue, 0.001f);
                }
                if (fd.functionName == "UseBool")
                {
                    ReturnValue rv = fd.callback(new ReturnValue[] { new ReturnValue(true)});
                    Assert.AreEqual(true, rv.BoolValue);
                }
            }
        }

        [Test]
        public void ThrowsException()
        {
            DemoClassTwo dc = new DemoClassTwo();
            Assert.Throws<Exception>(() => FunctionDefinitionCreator.CreateDefinitions(dc, typeof(DemoClassTwo)));
        }

		[Test]
		public void CallingFunctionDefinitionCreatorFunctionWithWrongArgumentType()
		{
			TextReader programString = File.OpenText("code72.txt");
			DefaultSprakRunner program = new DefaultSprakRunner(programString);



			program.run();

			Assert.AreEqual (0, program.getCompileTimeErrorHandler().getErrors().Count);
		}
    }
}

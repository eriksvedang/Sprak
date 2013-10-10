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

		static FunctionDefinition GetPrintFunction() {
			return new FunctionDefinition (
				"void", "print",
				new string[] { "string" }, new string[] { "text" },
				o => {
				Console.WriteLine (o[0].StringValue);
				return new ReturnValue (); }, 
				FunctionDocumentation.Default ());
		}

		[Test]
		public void CallingFunctionWithWrongArgumentType_MANUAL_FUNCTION_DEFINITION()
		{
			TextReader programString = File.OpenText("code72.txt");

			FunctionDefinition[] functionDefinitions = new FunctionDefinition[] {
				new FunctionDefinition(
					"number", "ThisFunctionTakesANumber",
					new string[] { "number" }, new string[] { "x" },
				ThisFunctionTakesANumber, FunctionDocumentation.Default()),

				GetPrintFunction()
			};

			SprakRunner program = new SprakRunner(programString, functionDefinitions);
			program.run();

			Assert.AreEqual (0, program.getCompileTimeErrorHandler().getErrors().Count);
		}

		ReturnValue ThisFunctionTakesANumber(ReturnValue[] pArguments) {
			return new ReturnValue(pArguments[0].NumberValue * 2.0f);
		}

		public class ClassWithFunction
		{
			[SprakAPI("Returns the number times two", "The number")]
			public float API_ThisFunctionTakesANumber(float x) { 
				return x * 2.0f;
			}
		}

		[Test]
		public void CallingFunctionWithWrongArgumentType_USING_FUNCTION_DEFINITION_CREATOR()
		{
			TextReader programString = File.OpenText("code73.txt");

			ClassWithFunction c = new ClassWithFunction ();
			FunctionDefinition[] funcDefs = FunctionDefinitionCreator.CreateDefinitions (c, typeof(ClassWithFunction));

			List<FunctionDefinition> moreFunctionDefinitions = new List<FunctionDefinition> {
				GetPrintFunction ()
			};

			moreFunctionDefinitions.AddRange (funcDefs);

			SprakRunner program = new SprakRunner(programString, moreFunctionDefinitions.ToArray());
			program.run();

			Assert.AreEqual (0, program.getCompileTimeErrorHandler().getErrors().Count);
		}
		
		public class DemoClassThree
	    {
	        [SprakAPI("adds all the numbers", "numbers")]
	        public float API_AddListOfNumbers(object[] nums) 
			{ 
				float sum = 0;
				foreach(float f in nums) {
					sum += f;
				}
				return sum;					
			}
	    }
		
		[Test]
		public void ArrayAsArgumentListTest()
		{
			DemoClassThree dc3 = new DemoClassThree();
			FunctionDefinition[] defs = FunctionDefinitionCreator.CreateDefinitions(dc3, typeof(DemoClassThree));
			Assert.AreEqual(1, defs.Length);
			
			List<FunctionDefinition> moreFunctionDefinitions = new List<FunctionDefinition> {
				GetPrintFunction ()
			};

			moreFunctionDefinitions.AddRange (defs);
			
			TextReader programString = File.OpenText("code75.txt");
			SprakRunner program = new SprakRunner(programString, moreFunctionDefinitions.ToArray());
			
			program.run();
			
			Assert.AreEqual (0, program.getCompileTimeErrorHandler().getErrors().Count);
		}

		public class DemoClassFour
		{
			[SprakAPI("this function crashes")]
			public void API_crash() 
			{ 
				throw new Error("Crashed!");
			}
		}

		[Test]
		public void CallFunctionThatThrowsException()
		{
			DemoClassFour dc3 = new DemoClassFour();
			FunctionDefinition[] defs = FunctionDefinitionCreator.CreateDefinitions(dc3, typeof(DemoClassFour));
			Assert.AreEqual(1, defs.Length);

			List<FunctionDefinition> moreFunctionDefinitions = new List<FunctionDefinition> {
				GetPrintFunction ()
			};

			moreFunctionDefinitions.AddRange (defs);

			TextReader programString = File.OpenText("code77.txt");
			SprakRunner program = new SprakRunner(programString, moreFunctionDefinitions.ToArray());

			program.run();

			Assert.AreEqual (0, program.getCompileTimeErrorHandler().getErrors().Count);
			Assert.AreEqual (1, program.getRuntimeErrorHandler().getErrors().Count);

			program.getRuntimeErrorHandler().printErrorsToConsole();
		}


    }
}

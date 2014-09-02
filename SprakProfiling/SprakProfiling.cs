using System;
using System.IO;
using ProgrammingLanguageNr1;
using System.Diagnostics;
using System.Collections.Generic;

namespace SprakProfiling
{
    class SprakProfiling
    {
        static void Main(string[] args)
        {
			string filename = ""; //"../Program1";
			
			if (args.Length > 0) {
				filename = args [0];
			} else {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine ("No program file given");
				return;
			}
			
            TextReader tr = File.OpenText(filename);
			//StringReader programString = new StringReader("g()\nfloat g() {\n	print(42)\n }");
			
			FunctionDefinition[] functionDefinitions = new FunctionDefinition[] {
                new FunctionDefinition("void", "print", new string[] { "string" }, new string[] { "text" }, print, FunctionDocumentation.Default())
            };

            SprakRunner runner = new SprakRunner(tr, functionDefinitions);
			bool success = runner.Start();
			if (success) {
				InterpreterTwo.Status status = InterpreterTwo.Status.OK;
				while (true) {
					status = runner.Step ();
					if (status == InterpreterTwo.Status.ERROR) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine ("Runtime error:");
						Console.ForegroundColor = ConsoleColor.White;
						runner.getCompileTimeErrorHandler ().printErrorsToConsole ();
					}
					else if (status == InterpreterTwo.Status.FINISHED) {
						return;
					}
				}
			} else {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine ("Compile errors:");
				Console.ForegroundColor = ConsoleColor.White;
				runner.getCompileTimeErrorHandler ().printErrorsToConsole ();
				return;
			}

        }
		
		private static ReturnValue print(ReturnValue[] parameters)
        {
            ReturnValue parameter0 = parameters[0];
			Console.WriteLine (parameter0);
            return new ReturnValue(); // void
        }
    }
}

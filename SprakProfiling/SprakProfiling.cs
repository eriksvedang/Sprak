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
			string filename = "../Program1";
			
			if(args.Length > 0) {
				filename = args[0];
			}
			
            TextReader tr = File.OpenText(filename);
			//StringReader programString = new StringReader("g()\nfloat g() {\n	print(42)\n }");
			
			FunctionDefinition[] functionDefinitions = new FunctionDefinition[] {
                new FunctionDefinition("void", "print", new string[] { "string" }, new string[] { "text" }, print, FunctionDocumentation.Default())
            };

            SprakRunner runner = new SprakRunner(tr, functionDefinitions);
			bool success = runner.Start();
			if(success) {
				while(runner.Step() == InterpreterTwo.Status.OK) {
				}
			}
			
			Console.WriteLine("OUTPUT: ");
			foreach(string s in m_output) {
				Console.WriteLine(s);
			}
			
			//runner.printTree(true);

            //Console.In.ReadLine();
        }
		
		private static ReturnValue print(ReturnValue[] parameters)
        {
            ReturnValue parameter0 = parameters[0];
            m_output.Add(parameter0.ToString());
            return new ReturnValue(); // void
        }
		
		static List<string> m_output = new List<string>();
    }
}

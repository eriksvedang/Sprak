using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ProgrammingLanguageNr1
{
    public class DefaultSprakRunner
    {
    	public DefaultSprakRunner(TextReader stream)
		{
            FunctionDefinition[] functionDefinitions = new FunctionDefinition[] {
                new FunctionDefinition("void", "print", new string[] { "var" }, new string[] { "the thing to print" }, new ExternalFunctionCreator.OnFunctionCall(print), FunctionDocumentation.Default()),
                new FunctionDefinition("number", "sqrt", new string[] { "number" }, new string[] { "f" }, new ExternalFunctionCreator.OnFunctionCall(sqrt), FunctionDocumentation.Default())
            };

            m_sprakRunner = new SprakRunner(stream, functionDefinitions);
		}
		
		public void printOutputToConsole()
        {
			Console.WriteLine("PROGRAM OUTPUT:");
			foreach(string s in m_output)
            {
				Console.WriteLine(s);
			}
		}

        private object print(object[] parameters)
        {
            object parameter0 = parameters[0];
			if (parameter0 == null) {
				throw new Exception ("Parameter0 is null!");
			}
			m_output.Add(ReturnValueConversions.PrettyStringRepresenation(parameter0));
			return VoidType.voidType;
        }

        private object sqrt(object[] parameters)
        {
            object parameter0 = parameters[0];
            if (parameter0.GetType() == typeof(float))
            {
                return (float)(Math.Sqrt((float)parameter0));
            }
            else
            {
				throw new Error("Can't use sqrt on something that's not a number", Error.ErrorType.SYNTAX, 0, 0);
            }
        }

        private object f(object[] parameters)
        {
            return new object();
        }
		
		public ErrorHandler getCompileTimeErrorHandler() { return m_sprakRunner.getCompileTimeErrorHandler(); }
		public ErrorHandler getRuntimeErrorHandler() { return m_sprakRunner.getRuntimeErrorHandler(); }

        public void run()
        {
            m_sprakRunner.run();
        }

		public List<string> Output
        {
            get { return m_output; }
        }
		
		public void printTree(bool printExecutionCounters)
		{
			m_sprakRunner.printTree(printExecutionCounters);
		}

		public object RunFunction(string functionName, object[] args) {
			return m_sprakRunner.RunFunction (functionName, args);
		}

		public SprakRunner sprakRunner {
			get {
				return m_sprakRunner;
			}
		}

        List<string> m_output = new List<string>();
		SprakRunner m_sprakRunner;        
	}
}

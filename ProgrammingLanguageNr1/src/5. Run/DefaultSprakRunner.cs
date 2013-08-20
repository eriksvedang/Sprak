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
                new FunctionDefinition("void", "print", new string[] { "string" }, new string[] { "text" }, new ExternalFunctionCreator.OnFunctionCall(print), FunctionDocumentation.Default()),
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

        private ReturnValue print(ReturnValue[] parameters)
        {
            ReturnValue parameter0 = parameters[0];
            m_output.Add(parameter0.ToString());
            return new ReturnValue(); // void
        }

        private ReturnValue sqrt(ReturnValue[] parameters)
        {
            ReturnValue parameter0 = parameters[0];
            if (parameter0.getReturnValueType() == ReturnValueType.NUMBER)
            {
                return new ReturnValue((float)(Math.Sqrt(parameter0.NumberValue)));
            }
            else
            {
                m_sprakRunner.getRuntimeErrorHandler().errorOccured(new Error("Can't use sqrt on something that's not a number", Error.ErrorType.SYNTAX, 0, 0));
                return new ReturnValue(0.0f);
            }
        }

        private ReturnValue f(ReturnValue[] parameters)
        {
            return new ReturnValue();
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

		public ReturnValue RunFunction(string functionName, ReturnValue[] args) {
			return m_sprakRunner.RunFunction (functionName, args);
		}

        List<string> m_output = new List<string>();
        SprakRunner m_sprakRunner;        
	}
}

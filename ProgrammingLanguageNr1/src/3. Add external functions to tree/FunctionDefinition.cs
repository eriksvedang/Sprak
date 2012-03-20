using System;
using System.Text;
using System.Collections.Generic;

namespace ProgrammingLanguageNr1
{
    public struct FunctionDefinition
    {
        public FunctionDefinition(string pReturnType, string pFunctionName, string[] pParameterTypes, string[] pParameterNames, ExternalFunctionCreator.OnFunctionCall pCallback, FunctionDocumentation pFunctionDocumentation)
        {
            returnType = pReturnType;
            functionName = pFunctionName;
            parameterTypes = pParameterTypes;
            parameterNames = pParameterNames;
            callback = pCallback;
			functionDocumentation = pFunctionDocumentation;
        }

        public string returnType;
        public string functionName;
        public string[] parameterTypes;
        public string[] parameterNames;
        public ExternalFunctionCreator.OnFunctionCall callback;
		public FunctionDocumentation functionDocumentation;
    }
	
	public struct FunctionDocumentation
	{
		public static FunctionDocumentation Default() {
			return new FunctionDocumentation("no function description", new string[] {});
		}
		
		public FunctionDocumentation(string pFunctionDescription, string[] pArgumentDescriptions) {
			_functionDescription = pFunctionDescription;
			_argumentDescriptions = pArgumentDescriptions;
		}
		
		public string GetFunctionDescription() { 
			return _functionDescription; 
		}
		
		public string GetArgumentDescription(int nr) 
		{
			if(nr < 0 || nr > _argumentDescriptions.Length - 1) {
				return "No description";
			} else {
				return _argumentDescriptions[nr];
			}
		}
		
		private string _functionDescription;
		private string[] _argumentDescriptions;
	}
}

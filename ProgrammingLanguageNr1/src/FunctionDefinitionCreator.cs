using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ProgrammingLanguageNr1;

namespace ProgrammingLanguageNr1
{
    public class SprakAPI : Attribute
    {
        public string[] Values { get; set; }
        public SprakAPI(params string[] values)
        {
            this.Values = values;
        }
    }

	public delegate void Log(string pString);

    public class FunctionDefinitionCreator
    {
		public static Log logger;

		public static void Log (string pString)
		{
			if (logger != null) {
				logger(pString);
			}
		}

        public static FunctionDefinition[] CreateDefinitions (object pProgramTarget, Type pClassType)
		{
			//Log("Creating function definition targeted on " + pProgramTarget.ToString () + " on class of type " + pClassType.ToString ());

            MethodInfo[] methodInfos = pClassType.GetMethods();

			var documentation = CreateDocumentation (methodInfos);
            var functionDefinitions = CreateFunctionDefinitions (pProgramTarget, documentation, methodInfos);

            return functionDefinitions.ToArray();
        }

		static Dictionary<string, FunctionDocumentation> CreateDocumentation (MethodInfo[] methodInfos)
		{
			Dictionary<string, FunctionDocumentation> functionDocumentations = new Dictionary<string, FunctionDocumentation>();

			foreach (MethodInfo methodInfo in methodInfos) {
				//Log("Found method " + methodInfo.Name);
				if (methodInfo.Name.StartsWith ("API_")) {
					SprakAPI[] helpAttributes = (SprakAPI[])methodInfo.GetCustomAttributes (typeof(SprakAPI), true);
					if (helpAttributes.Length > 0) {
						//Console.WriteLine("found " + String.Join( ",", help[0].Values));
						List<string> parameterHelp = new List<string> ();
						for (int i = 1; i < helpAttributes [0].Values.Length; i++) {
							parameterHelp.Add (helpAttributes [0].Values [i]);
						}
						string shortname = methodInfo.Name.Substring (4);
						FunctionDocumentation fd = new FunctionDocumentation (helpAttributes [0].Values [0], parameterHelp.ToArray ());
						functionDocumentations.Add (shortname, fd);
					}
				}
			}

			return functionDocumentations;
		}

		static List<FunctionDefinition> CreateFunctionDefinitions (object pProgramTarget, Dictionary<string, FunctionDocumentation> functionDocumentations, MethodInfo[] methodInfos)
		{
			List<FunctionDefinition> functionDefinitions = new List<FunctionDefinition>();

			foreach (MethodInfo methodInfo in methodInfos) 
			{
				if (!methodInfo.Name.StartsWith ("API_")) {
					continue;
				}

				/*if (methodInfo.ReturnType.IsArray) {
					throw new Exception ("FunctionDefinitionCreator can't handle array return value");
				}*/

				//Console.WriteLine("parsing " + mi.Name + " return Type " + mi.ReturnType.Name);
				string shortname = methodInfo.Name.Substring (4);
	
				List<ReturnValueType> parameterTypes = new List<ReturnValueType> ();
				List<string> parameterNames = new List<string> ();
				List<string> parameterTypeNames = new List<string> ();

				foreach (ParameterInfo parameterInfo in methodInfo.GetParameters ()) {
					/*
					if (parameterInfo.ParameterType.IsArray) {
						throw new Exception ("FunctionDefinitionCreator can't handle array parameters");
					}
					*/
					parameterNames.Add (parameterInfo.Name);
					parameterTypes.Add (ReturnValue.SystemTypeToReturnValueType (parameterInfo.ParameterType));
					parameterTypeNames.Add (ReturnValue.SystemTypeToReturnValueType (parameterInfo.ParameterType).ToString ().ToLower ());
				}

				MethodInfo lambdaMethodInfo = methodInfo; // "hard copy" because of c# lambda rules

				ExternalFunctionCreator.OnFunctionCall function = (sprakArguments =>  {

					ParameterInfo[] realParamInfo = lambdaMethodInfo.GetParameters ();
					List<object> parameters = new List<object> ();

					int i = 0;
					foreach (ReturnValue sprakArg in sprakArguments) {
						//Console.WriteLine(string.Format("Argument {0} in function {1} is of type {2}", i, shortname, realParamInfo[i].ParameterType));

						var realParamType = realParamInfo [i].ParameterType;
						//Console.WriteLine("Real param type is " + realParamType);
						
						if (realParamInfo[i].ParameterType.IsArray) {
							object[] converted = new object[sprakArg.ArrayValue.Count];
							int arrayCounter = 0;
							foreach(ReturnValue arrayItem in sprakArg.ArrayValue.Values) {
								converted[arrayCounter] = arrayItem.GetValueAsObject();
								arrayCounter++;
							}
							parameters.Add (converted);
						}
						else if (realParamType == typeof(int)) {
							parameters.Add (Convert.ToInt32 (sprakArg.NumberValue));
						}
						else if (realParamType == typeof(float)) {
							parameters.Add (sprakArg.NumberValue);
						}
						else if (realParamType == typeof(bool)) {
							parameters.Add (sprakArg.BoolValue);
						}
						else if (realParamType == typeof(string)) {
							parameters.Add (sprakArg.StringValue);
						}
						else if (realParamType == typeof(object)) {
							parameters.Add (sprakArg.GetValueAsObject());
						}
						else {
							throw new Error("Can't deal with arg " + i.ToString() + " in function " + shortname);
						}
						
						i++;
					}

					//Console.WriteLine("supplied parameter count" + parameters.Count + " neededParamter count " + lamdaMethodInfo.GetParameters().Length);

					object result = null;

					try {
						result = lambdaMethodInfo.Invoke (pProgramTarget, parameters.ToArray ());
					}
					catch(System.Reflection.TargetInvocationException e) {
						//Console.WriteLine("Got an exception when calling the lambda: " + e.ToString());
						//Console.WriteLine("The base exception: " + e.GetBaseException().ToString());
						throw e.GetBaseException();
					}

					if (lambdaMethodInfo.ReturnType == typeof(void)) {
						return new ReturnValue (ReturnValueType.VOID);
					}
					else {
						return new ReturnValue (ReturnValue.SystemTypeToReturnValueType (result.GetType() /*lambdaMethodInfo.ReturnType*/), result);
					}
				});

				ReturnValueType returnValueType = ReturnValue.SystemTypeToReturnValueType (methodInfo.ReturnType);

				FunctionDocumentation doc;

				if (functionDocumentations.ContainsKey (shortname)) {
					doc = functionDocumentations [shortname];
				} else {
					doc = FunctionDocumentation.Default ();
				}

				functionDefinitions.Add (new FunctionDefinition (
					returnValueType.ToString (), 
					shortname, 
					parameterTypeNames.ToArray (), 
					parameterNames.ToArray (), 
					function, // The lambda
					doc));
			}

			return functionDefinitions;
		}
    }
}

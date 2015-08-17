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

		static HashSet<Type> acceptableTypes = new HashSet<Type>() { typeof(float), typeof(string), typeof(bool), typeof(Range), typeof(void), typeof(object[]) };

		static List<FunctionDefinition> CreateFunctionDefinitions (object pProgramTarget, Dictionary<string, FunctionDocumentation> functionDocumentations, MethodInfo[] methodInfos)
		{
			List<FunctionDefinition> functionDefinitions = new List<FunctionDefinition>();

			foreach (MethodInfo methodInfo in methodInfos) 
			{
				if (!methodInfo.Name.StartsWith ("API_")) {
					continue;
				}

				MethodInfo lambdaMethodInfo = methodInfo; // "hard copy" because of c# lambda rules

				string shortname = lambdaMethodInfo.Name.Substring (4);
	
				List<ReturnValueType> parameterTypes = new List<ReturnValueType> ();
				List<string> parameterNames = new List<string> ();
				List<string> parameterTypeNames = new List<string> ();

				foreach (ParameterInfo parameterInfo in lambdaMethodInfo.GetParameters ()) {
					var t = ReturnValueConversions.SystemTypeToReturnValueType (parameterInfo.ParameterType);
					//Console.WriteLine("Registering parameter '" + parameterInfo.Name + "' (" + parameterInfo.ParameterType + ") with ReturnValueType " + t + " for function " + shortname);
					parameterNames.Add (parameterInfo.Name);
					parameterTypes.Add (t);
					parameterTypeNames.Add (t.ToString().ToLower());
				}

				ExternalFunctionCreator.OnFunctionCall function = (sprakArguments =>  {

					ParameterInfo[] realParamInfo = lambdaMethodInfo.GetParameters ();

					if(sprakArguments.Count() != realParamInfo.Length) {
						throw new Error("Should call '" + shortname + "' with " + realParamInfo.Length + " argument" + (realParamInfo.Length == 1 ? "" : "s"));
					}

					int i = 0;
					foreach (object sprakArg in sprakArguments) {
						Console.WriteLine(string.Format("Parameter {0} in function {1} is of type {2}", i, shortname, realParamInfo[i].ParameterType));

						var realParamType = realParamInfo [i].ParameterType;

						if(sprakArg.GetType() == typeof(SortedDictionary<KeyWrapper,object>)) {
							sprakArguments[i] = (sprakArg as SortedDictionary<KeyWrapper,object>).Values.ToArray();
						}

						if(sprakArg.GetType() == typeof(int)) {
							// YES, this is kind of a hack
							sprakArguments[i] = (float)sprakArg;
							realParamType = typeof(int);
						}

						if (acceptableTypes.Contains(realParamType)) {
							// OK
						}
						else {
							throw new Error("Can't deal with parameter " + i.ToString() + " of type " + realParamType + " in function " + shortname);
						}
						
						i++;
					}

					//Console.WriteLine("supplied parameter count" + parameters.Count + " neededParamter count " + lamdaMethodInfo.GetParameters().Length);

					object result = null;

					try {
						Console.WriteLine("Will call " + shortname  + " with sprak arguments:");
						int j = 0;
						foreach(var a in sprakArguments) {
							Console.WriteLine(" Argument " + (j++) + ": " + ReturnValueConversions.PrettyStringRepresenation(a) + " (" + a.GetType() + ")");
						}
						result = lambdaMethodInfo.Invoke (pProgramTarget, sprakArguments.ToArray ());
					}
					catch(System.Reflection.TargetInvocationException e) {
						//Console.WriteLine("Got an exception when calling the lambda: " + e.ToString());
						//Console.WriteLine("The base exception: " + e.GetBaseException().ToString());
						throw e.GetBaseException();
					}

					// HACK
					if(lambdaMethodInfo.ReturnType == typeof(int)) {
						return (float)(int)result;
					}

					if(!acceptableTypes.Contains(lambdaMethodInfo.ReturnType)) {
						throw new Error("Function '" + shortname + "' can't return value of type " + lambdaMethodInfo.ReturnType.ToString());
					}

					if (lambdaMethodInfo.ReturnType == typeof(void)) {
						return VoidType.voidType;
					}
					else {
						return result;
					}
				});

				ReturnValueType returnValueType = ReturnValueConversions.SystemTypeToReturnValueType (lambdaMethodInfo.ReturnType);

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

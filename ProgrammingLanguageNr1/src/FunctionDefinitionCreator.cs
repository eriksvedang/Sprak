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

            List<FunctionDefinition> functionDefinitions = new List<FunctionDefinition>();
            Dictionary<string, FunctionDocumentation> functionDocumentations = new Dictionary<string, FunctionDocumentation>();
            MethodInfo[] methodInfos = pClassType.GetMethods();

            foreach (MethodInfo methodInfo in methodInfos)
            {
				//Log("Found method " + methodInfo.Name);

                if (methodInfo.Name.StartsWith("API_"))
                {
                    SprakAPI[] helpAttributes = (SprakAPI[])methodInfo.GetCustomAttributes(typeof(SprakAPI), true);
                    if (helpAttributes.Length > 0)
                    {
                        //Console.WriteLine("found " + String.Join( ",", help[0].Values));
                        List<string> parameterHelp = new List<string>();
                        for (int i = 1; i < helpAttributes[0].Values.Length; i++) {
                            parameterHelp.Add(helpAttributes[0].Values[i]);
						}
                        string shortname = methodInfo.Name.Substring(4);
                        FunctionDocumentation fd = new FunctionDocumentation(helpAttributes[0].Values[0], parameterHelp.ToArray());
                        functionDocumentations.Add(shortname, fd);
                    }
                }
            }

            foreach (MethodInfo methodInfo in methodInfos)
            {
                if (methodInfo.Name.StartsWith("API_"))
                {
                    //Console.WriteLine("parsing " + mi.Name + " return Type " + mi.ReturnType.Name);
                    string shortname = methodInfo.Name.Substring(4);

					if (methodInfo.ReturnType.IsArray) {
						throw new Exception ("FunctionDefinitionCreator can't handle array return value");
					}

                    List<ReturnValueType> parameterTypes = new List<ReturnValueType>();
                    List<string> parameterNames = new List<string>();
                    List<string> parameterTypeNames = new List<string>();

                    foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
                    {
						if (parameterInfo.ParameterType.IsArray) {
							throw new Exception ("FunctionDefinitionCreator can't handle array parameters");
						}
                        parameterNames.Add(parameterInfo.Name);
                        parameterTypes.Add(ReturnValue.SystemTypeToReturnValueType(parameterInfo.ParameterType));
                        parameterTypeNames.Add(ReturnValue.SystemTypeToReturnValueType(parameterInfo.ParameterType).ToString().ToLower());
                    }

                    MethodInfo lamdaMethodInfo = methodInfo;
                    ExternalFunctionCreator.OnFunctionCall function = (retvals) =>
                    {
                        int i = 0;
                        ParameterInfo[] realParamInfo = lamdaMethodInfo.GetParameters();
                        List<object> parameters = new List<object>();

                        foreach (ReturnValue r in retvals)
                        {
                            if (realParamInfo[i++].ParameterType == typeof(int)) {
                                parameters.Add(Convert.ToInt32(r.Unpack()));
							}
                            else {
                                parameters.Add(r.Unpack());
							}
                        }

                        //Console.WriteLine("supplied parameter count" + parameters.Count + " neededParamter count " + lamdaMethodInfo.GetParameters().Length);
                        object result = lamdaMethodInfo.Invoke(pProgramTarget, parameters.ToArray());

                        if (lamdaMethodInfo.ReturnType == typeof(void)) {
                            return new ReturnValue(ReturnValueType.VOID);
						}
                        else {
                            return new ReturnValue(ReturnValue.SystemTypeToReturnValueType(lamdaMethodInfo.ReturnType), result);
                        }
                    };

                    ReturnValueType returnValueType = ReturnValue.SystemTypeToReturnValueType(methodInfo.ReturnType);
                    
					FunctionDocumentation doc;
					if (functionDocumentations.TryGetValue (shortname, out doc)) {
						functionDefinitions.Add (new FunctionDefinition(returnValueType.ToString(), shortname, parameterTypeNames.ToArray(), parameterNames.ToArray(), function, doc));
					} else {
						functionDefinitions.Add (new FunctionDefinition(returnValueType.ToString(), shortname, parameterTypeNames.ToArray(), parameterNames.ToArray(), function, FunctionDocumentation.Default()));
					}
                }
            }

            return functionDefinitions.ToArray();
        }
    }
}

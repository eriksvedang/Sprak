using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

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

    public class FunctionDefinitionCreator
    {
        public static FunctionDefinition[] CreateDefinitions(object o, Type t)
        {
            List<FunctionDefinition> methods = new List<FunctionDefinition>();
            Dictionary<string, FunctionDocumentation> HelpInfo = new Dictionary<string, FunctionDocumentation>();
            MethodInfo[] methodInfos = t.GetMethods();
            foreach (MethodInfo methodI in methodInfos)
            {
                if (methodI.Name.StartsWith("API_"))
                {
                    SprakAPI[] help = (SprakAPI[])methodI.GetCustomAttributes(typeof(SprakAPI), true);
                    if (help.Length > 0)
                    {
                        //Console.WriteLine("found " + String.Join( ",", help[0].Values));
                        List<string> parameterHelp = new List<string>();
                        for (int i = 1; i < help[0].Values.Length; i++)
                            parameterHelp.Add(help[0].Values[i]);
                        string shortname = methodI.Name.Substring(4);
                        FunctionDocumentation fd = new FunctionDocumentation(help[0].Values[0], parameterHelp.ToArray());
                        HelpInfo.Add(shortname, fd);
                    }
                }
            }
            foreach (MethodInfo mi in methodInfos)
            {
                if (mi.Name.StartsWith("API_"))
                {
                    //Console.WriteLine("parsing " + mi.Name + " return Type " + mi.ReturnType.Name);
                    string shortname = mi.Name.Substring(4);
                    if(mi.ReturnType.IsArray)
                        throw new Exception("FunctionDefinitionCreator can't handle array return value!");
                    List<ReturnValueType> parameterTypes = new List<ReturnValueType>();
                    List<string> parameterNames = new List<string>();
                    List<string> parameterTypeNames = new List<string>();
                    foreach (ParameterInfo pi in mi.GetParameters())
                    {
                        if (pi.ParameterType.IsArray)
                            throw new Exception("FunctionDefinitionCreator can't handle array parameters!");

                        parameterNames.Add(pi.Name);
                        parameterTypes.Add(ReturnValue.SystemTypeToReturnValueType(pi.ParameterType));
                        parameterTypeNames.Add(ReturnValue.SystemTypeToReturnValueType(pi.ParameterType).ToString().ToLower());
                    }
                    MethodInfo lamdaMethodInfo = mi;
                    ExternalFunctionCreator.OnFunctionCall function = (retvals) =>
                    {
                        int i = 0;
                        ParameterInfo[] realParamInfo = lamdaMethodInfo.GetParameters();
                        List<object> parameters = new List<object>();
                        foreach (ReturnValue r in retvals)
                        {
                            if (realParamInfo[i++].ParameterType == typeof(int))
                                parameters.Add(Convert.ToInt32(r.Unpack()));
                            else
                                parameters.Add(r.Unpack());
                        }
                        //Console.WriteLine("supplied parameter count" + parameters.Count + " neededParamter count " + lamdaMethodInfo.GetParameters().Length);
                        object result = lamdaMethodInfo.Invoke(o, parameters.ToArray());
                        if (lamdaMethodInfo.ReturnType == typeof(void))
                            return new ReturnValue(ReturnValueType.VOID);
                        else
                        {
                            return new ReturnValue(ReturnValue.SystemTypeToReturnValueType(lamdaMethodInfo.ReturnType), result);
                        }
                    };
                    ReturnValueType returnValueType = ReturnValue.SystemTypeToReturnValueType(mi.ReturnType);
                    FunctionDocumentation doc;
                    if (HelpInfo.TryGetValue(shortname, out doc))
                        methods.Add(new FunctionDefinition(returnValueType.ToString(), shortname, parameterTypeNames.ToArray(), parameterNames.ToArray(), function, doc));
                    else
                        methods.Add(new FunctionDefinition(returnValueType.ToString(), shortname, parameterTypeNames.ToArray(), parameterNames.ToArray(), function, FunctionDocumentation.Default()));
                }
            }
            return methods.ToArray();
        }
    }
}

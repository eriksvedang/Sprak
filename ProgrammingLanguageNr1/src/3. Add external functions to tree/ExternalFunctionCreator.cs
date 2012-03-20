using System;
using System.Collections.Generic;
using System.Text;

namespace ProgrammingLanguageNr1
{
    public class ExternalFunctionCreator
    {
        public delegate ReturnValue OnFunctionCall(ReturnValue[] pParameters);
        public Dictionary<string, OnFunctionCall> externalFunctions = new Dictionary<string, OnFunctionCall>();

        public ExternalFunctionCreator(FunctionDefinition[] functionDefinitions)
        {
            if (functionDefinitions != null)
            {
                foreach (FunctionDefinition f in functionDefinitions)
                {
                    defineFunction(f);
                }
            }
        }

        private void defineFunction(FunctionDefinition f)
        {
            if (externalFunctions.ContainsKey(f.functionName))
            {
                throw new Error("There is already a function called '" + f.functionName + "'", Error.ErrorType.UNDEFINED, 0, 0);
            }

            AST parameterList = new AST(new Token(Token.TokenType.NODE_GROUP, "<PARAMETER_LIST>"));
            for (int i = 0; i < f.parameterTypes.Length; ++i)
            {
                parameterList.addChild(createParameterDefinition(f.parameterTypes[i], f.parameterNames[i]));
            }

            AST functionNode = createFunctionDefinitionNode(f.returnType, f.functionName, parameterList);
            
            m_builtInFunctions.Add(functionNode);
            externalFunctions.Add(f.functionName, f.callback);
        }

        private AST_FunctionDefinitionNode createFunctionDefinitionNode(string returnTypeName, string functionName, AST parameterList)
        {

            AST_FunctionDefinitionNode functionNode =
                new AST_FunctionDefinitionNode(new Token(Token.TokenType.FUNC_DECLARATION, "<EXTERNAL_FUNC_DECLARATION>"));

            functionNode.addChild(new Token(Token.TokenType.BUILT_IN_TYPE_NAME, returnTypeName));
            functionNode.addChild(new Token(Token.TokenType.NAME, functionName));
            functionNode.addChild(parameterList);
            functionNode.addChild(new Token(Token.TokenType.STATEMENT_LIST, "<EMPTY_STATEMENT_LIST>"));

            return functionNode;
        }

        private AST createParameterDefinition(string typeName, string variableName)
        {
            AST parameter = new AST(new Token(Token.TokenType.PARAMETER, "<PARAMETER>"));

            AST declaration = new AST_VariableDeclaration(new Token(Token.TokenType.VAR_DECLARATION, "<PARAMETER_DECLARATION>"), ReturnValue.getReturnValueTypeFromString(typeName), variableName);
            AST assigment = new AST_Assignment(new Token(Token.TokenType.ASSIGNMENT, "="), variableName);

            parameter.addChild(declaration);
            parameter.addChild(assigment);

            return parameter;
        }

        public List<AST> FunctionASTs
        {
            get { return m_builtInFunctions; }
        }

        List<AST> m_builtInFunctions = new List<AST>();  
    }
}

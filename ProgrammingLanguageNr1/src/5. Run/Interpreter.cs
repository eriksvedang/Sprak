// #define WRITE_DEBUG_INFO

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProgrammingLanguageNr1
{
	public class Interpreter
	{
		public Interpreter (AST ast, Scope globalScope, ErrorHandler errorHandler, ExternalFunctionCreator externalFunctionCreator)
		{
			m_ast = ast;
			m_errorHandler = errorHandler;
			m_globalScope = globalScope;
			m_currentScope = m_globalScope;
            m_externalFunctionCreator = externalFunctionCreator;
		}
		
		public void run() 
		{
			m_globalMemorySpace = new MemorySpace("globals");
			m_currentMemorySpace = m_globalMemorySpace;
			
			execute(m_ast);
		}
		
		private ReturnValue execute(AST tree) {

            Token.TokenType tokenType = tree.getToken().getTokenType();
            ReturnValue returnValue = null;

            if (tokenType == Token.TokenType.FUNC_DECLARATION)
            {
                return new ReturnValue();
            }

#if WRITE_DEBUG_INFO
			Console.WriteLine("Executing " + tree.getTokenType() + " " + tree.getTokenString());
#endif
			
			if (tokenType == Token.TokenType.STATEMENT_LIST) 
			{
				executeAllChildNodes(tree);
			}
			else if (tokenType == Token.TokenType.FUNCTION_CALL) {
				returnValue = functionCall(tree);
			}
			else if (tokenType == Token.TokenType.NAME) {
				returnValue = name(tree);
			}
			else if (tokenType == Token.TokenType.NUMBER) {
				returnValue = number(tree);
			}
			else if (tokenType == Token.TokenType.OPERATOR) {
				returnValue = operation(tree);
			}
			else if (tokenType == Token.TokenType.QUOTED_STRING) {
				returnValue = quotedString(tree);
			}
			else if (tokenType == Token.TokenType.IF) {
				ifThenElse(tree);
			}
			else if (tokenType == Token.TokenType.VAR_DECLARATION) {
				varDeclaration(tree);
			}
			else if (tokenType == Token.TokenType.ASSIGNMENT) {
				assignment(tree);
			}
			else if (tokenType == Token.TokenType.RETURN) {
				returnStatement(tree);
			}
			else {
				throw new NotImplementedException("The interpreter hasn't got support for token type " + tokenType + " yet!");
			}
			return returnValue;
		}
		
		private void executeAllChildNodes(AST tree) {
			if(tree.getChildren() == null) { return; }
			foreach (AST childTree in tree.getChildren()) {
				execute(childTree);
			}
		}
		
		private void varDeclaration(AST tree) {
			string typeName = tree.getChild(0).getTokenString();
			ReturnValueType variableType = ReturnValue.getReturnValueTypeFromString(typeName);
			string variableName = tree.getChild(1).getTokenString();
			
			switch(variableType) {
                case ReturnValueType.FLOAT:
                    m_currentMemorySpace.setValue(variableName, new ReturnValue(0.0f));
                    break;
                case ReturnValueType.STRING:
                    m_currentMemorySpace.setValue(variableName, new ReturnValue(0.0f));
                    break;
                default:
                    throw new InvalidOperationException("Can't declare a variable of type " + typeName);
            }
		}
		
		private void assignment(AST tree) {
			string variableName = tree.getChild(0).getTokenString();
			AST expression = tree.getChild(1);
			ReturnValue expressionValue = execute(expression);
			assignValue(variableName, expressionValue);									
		}
		
		private void assignValue(string variableName, ReturnValue valueToAssign) {
			Assert.IsNotNull(m_currentScope);

            //Console.WriteLine("Current scope: " + m_currentScope.getName());

			Symbol symbol = m_currentScope.resolve(variableName);
			if (symbol == null) {
				throw new InvalidOperationException("Can't resolve variable with name " + variableName);
			}

            ReturnValueType variableType = symbol.getReturnValueType();
			
			switch(variableType) 
            {
                case ReturnValueType.FLOAT:
				    float floatValue = valueToAssign.FloatValue;
				    ReturnValue floatVal = new ReturnValue(floatValue);
                    m_currentMemorySpace.setValue(variableName, floatVal);
                    break;
			
                case ReturnValueType.STRING:
				    string stringValue = valueToAssign.StringValue;
				    ReturnValue stringVal = new ReturnValue(stringValue);
                    m_currentMemorySpace.setValue(variableName, stringVal);
                    break;

                default:
                    throw new InvalidOperationException("Can't assign to a variable of type + " + variableType);
			}
		}
		
		private ReturnValue functionCall(AST tree) {
			ReturnValue returnValue = null;

            if (m_externalFunctionCreator.externalFunctions.ContainsKey(tree.getTokenString()))
            {
                ExternalFunctionCreator.OnFunctionCall functionCall = m_externalFunctionCreator.externalFunctions[tree.getTokenString()];
                if (functionCall != null)
                {
                    ReturnValue[] parameters = new ReturnValue[tree.getChildren().Count];
                    int i = 0;
                    foreach (AST parameter in tree.getChildren())
                    {
                        parameters[i] = execute(parameter);
                        i++;
                    }
                    returnValue = functionCall(parameters);
                }
                else
                {
                    throw new Error("Can't find external function " + tree.getTokenString(), Error.ErrorType.UNDEFINED, tree.getToken().LineNr, tree.getToken().LinePosition);
                }
            }
			else
			{
				// Call user defined function
				string functionName = tree.getTokenString();
				AST functionTree = getFunctionTreeNode(functionName);
				Assert.IsNotNull(functionTree);
				
				// Create list of parameter values
				List<ReturnValue> parameterValues = new List<ReturnValue>();
				List<AST> functionCallChildNodes = tree.getChildren();
				if (functionCallChildNodes != null)
                {
					foreach(AST parameter in tree.getChildren())
                    {					
						ReturnValue val = execute(parameter);
						parameterValues.Add(val);
					}
				}
				
				returnValue = function(functionTree, parameterValues);
			}

			return returnValue;
		}
		
		private ReturnValue function(AST tree, List<ReturnValue> parameterValues) {
			
			// Push scope
            Scope m_previousScope = m_currentScope;
			AST_FunctionDefinitionNode functionDefinitionNode = (AST_FunctionDefinitionNode)(tree);
			Assert.IsNotNull(functionDefinitionNode);
			m_currentScope = (Scope)functionDefinitionNode.getScope();
			Assert.IsNotNull(m_currentScope);
			
			// Push memory space
			MemorySpace m_previousMemorySpace = m_currentMemorySpace;
			MemorySpace functionMemorySpace = 
				new MemorySpace("<FUNCTION_SPACE " + tree.getChild(1).getTokenString() + ">");
			m_memoryStack.Push(functionMemorySpace);
			m_currentMemorySpace = functionMemorySpace;
			
			// Add parameters to memory space
			List<AST> parameterDeclarations = tree.getChild(2).getChildren();
			if(parameterDeclarations != null) {
				
				if(parameterDeclarations.Count != parameterValues.Count) {
					m_errorHandler.errorOccured(
						"The number of arguments in function " + 
						tree.getChild(1).getTokenString() + 
						" does not match!", 
						Error.ErrorType.SYNTAX);
				}
				
				foreach(AST parameter in parameterDeclarations) {
					varDeclaration(parameter);
				}
			}
			
			// Assign values to parameters
			if(parameterValues != null) {
				int i = 0;
				foreach(ReturnValue parameterValue in parameterValues) {
					
					string parameterName = parameterDeclarations[i].getChild(1).getTokenString();
					assignValue(parameterName, parameterValue);
					i++;
				}
			}
			
			// Execute function
			ReturnValue returnValue = null;
			
			try {
				executeAllChildNodes(tree.getChild(3)); // child 3 is the function body
			}
			catch(ReturnValue functionReturnValue) {
				returnValue = functionReturnValue;
			}
			
			// Pop memory space
			m_memoryStack.Pop();
			m_currentMemorySpace = m_previousMemorySpace;
			
			// Pop scope
            m_currentScope = m_previousScope;
			Assert.IsNotNull(m_currentScope);
			
			return returnValue;
		}
		
		private void returnStatement(AST tree) {
            ReturnValue returnValue = new ReturnValue();
            if (tree.getChildren().Count > 0)
            {
                returnValue = execute(tree.getChild(0));
            }
			if(returnValue != null) {
#if WRITE_DEBUG_INFO
				Console.Write("Return value was: ");
				printReturnValue(returnValue);
#endif
				throw returnValue;
			}
		}
		
		private void printReturnValue(ReturnValue returnValue) {
			if (returnValue.getType() == ReturnValueType.FLOAT) {
				Console.WriteLine(returnValue.FloatValue);
			}
			else if (returnValue.getType() == ReturnValueType.STRING) {
				Console.WriteLine(returnValue.StringValue);
			}
			else {
				Console.WriteLine("NULL");
			}
		}
		
		private AST getFunctionTreeNode(string functionName) {
			FunctionSymbol funcSym = (FunctionSymbol)m_globalScope.resolve(functionName);
			if (funcSym == null) {
				throw new InvalidOperationException("Can't find function with name " + functionName);
			}
			return funcSym.getFunctionDefinitionNode();
		}
		
		private void ifThenElse(AST tree) {
			
			// Push scope
			AST_IfNode ifNode = (AST_IfNode)(tree);
			Assert.IsNotNull(ifNode);
			m_currentScope = (Scope)ifNode.getScope();
			Assert.IsNotNull(m_currentScope);
			
			// Evaluate conditional
			ReturnValue conditionalExpression = execute(tree.getChild(0));
			
			if (conditionalExpression.FloatValue != 0) {
				Assert.IsNotNull(tree.getChild(1));
				execute(tree.getChild(1));
			}
			else {
				if (tree.getChildren().Count == 3) {
					Assert.IsNotNull(tree.getChild(2));
					execute(tree.getChild(2));
				}
			}
			
			// Pop scope
			m_currentScope = (Scope)ifNode.getScope().getEnclosingScope();
			Assert.IsNotNull(m_currentScope);
		}
		
		private ReturnValue name(AST tree) {
			
			string name = tree.getTokenString();
			ReturnValue val = null;
			
			val = m_currentMemorySpace.getValue(name);
			if (val == null) {
				val = m_globalMemorySpace.getValue(name);
			}
	
			Assert.IsNotNull(val);
			
#if WRITE_DEBUG_INFO
			Console.WriteLine("The fetched value is of type " + val.getReturnType());
			if(val.getReturnType() == ReturnType.FLOAT) {
				Console.WriteLine("And has value: " + val.FloatValue);
			}
			else if(val.getReturnType() == ReturnType.STRING) {
				Console.WriteLine("And has value: " + val.StringValue);
			}
#endif
			return val;
		}
		
		private ReturnValue quotedString(AST tree) {
			ReturnValue returnValue = new ReturnValue(tree.getTokenString());
			return returnValue;
		}
		
		private ReturnValue number(AST tree) {
			ReturnValue returnValue = new ReturnValue((float)Convert.ToDouble(tree.getTokenString()));
			return returnValue;
		}
		
		private ReturnValue operation(AST tree) {
			ReturnValue returnValue = null;
			
			float lhs = execute(tree.getChild(0)).FloatValue;
			float rhs = execute(tree.getChild(1)).FloatValue;
			
			if(tree.getTokenString() == "+") {
				returnValue = new ReturnValue(lhs + rhs);
			}
			else if(tree.getTokenString() == "-") {
				returnValue = new ReturnValue(lhs - rhs);
			}
			else if(tree.getTokenString() == "*") {
				returnValue = new ReturnValue(lhs * rhs);
			}
			else if(tree.getTokenString() == "/") {
				returnValue = new ReturnValue(lhs / rhs);
			}
			else if(tree.getTokenString() == "<") {
				float v = lhs < rhs ? 1 : 0;
				returnValue = new ReturnValue(v);
			}
			else if(tree.getTokenString() == ">") {
				float v = lhs > rhs ? 1 : 0;
				returnValue = new ReturnValue(v);
			}
			else if(tree.getTokenString() == "<=") {
				float v = lhs <= rhs ? 1 : 0;
				returnValue = new ReturnValue(v);
			}
			else if(tree.getTokenString() == ">=") {
				float v = lhs >= rhs ? 1 : 0;
				returnValue = new ReturnValue(v);
			}
			else if(tree.getTokenString() == "==") {
				float v = lhs == rhs ? 1 : 0;
				returnValue = new ReturnValue(v);
			}
			else if(tree.getTokenString() == "!=") {
				float v = lhs != rhs ? 1 : 0;
				returnValue = new ReturnValue(v);
			}
			else if(tree.getTokenString() == "&&") {
				float v = ((lhs != 0 ? true : false) && (rhs != 0 ? true : false)) ? 1 : 0;
				returnValue = new ReturnValue(v);
			}
			else if(tree.getTokenString() == "||") {
				float v = ((lhs != 0 ? true : false) || (rhs != 0 ? true : false)) ? 1 : 0;
				returnValue = new ReturnValue(v);
			}
			else {
				throw new NotImplementedException("Operator " + tree.getTokenString() + " isn't implemented yet!");
			}
			
			return returnValue;
		}
		
		public ErrorHandler getErrorHandler() { return m_errorHandler; } // TODO: remove this getter, it's unnecessary
		
		AST m_ast;
        ExternalFunctionCreator m_externalFunctionCreator;
		ErrorHandler m_errorHandler;
		Scope m_globalScope;
		Scope m_currentScope;
		MemorySpace m_globalMemorySpace;
		MemorySpace m_currentMemorySpace;
		Stack<MemorySpace> m_memoryStack = new Stack<MemorySpace>();
    }

}


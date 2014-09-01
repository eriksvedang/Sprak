//#define WRITE_DEBUG_INFO
//#define PRINT_STACK
//#define WRITE_CONVERT_INFO
//#define LOG_SCOPES

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProgrammingLanguageNr1
{
    public class InterpreterTwo : IEnumerable<InterpreterTwo.Status>
    {
        public enum Status {
            OK,
            ERROR,
            FINISHED
        }

        public InterpreterTwo(AST ast, Scope globalScope, ErrorHandler errorHandler, ExternalFunctionCreator externalFunctionCreator)
        {
			#if WRITE_DEBUG_INFO
            	Console.WriteLine("\nCreated Interpreter!!");
			#endif
			
            m_ast = ast; 
            m_errorHandler = errorHandler;
            m_globalScope = globalScope;
            m_currentScope = m_globalScope;
            m_externalFunctionCreator = externalFunctionCreator;
            Reset();
        }

        public void Reset()
        {
			m_globalMemorySpace = new MemorySpace("globals", m_ast.getChild (0), m_globalScope, m_memorySpaceNodeListCache);
			m_currentMemorySpace = m_globalMemorySpace;

			m_memorySpaceStack.Clear ();

			m_currentScope = m_globalScope;
			m_currentScope.ClearMemorySpaces();
			m_currentScope.PushMemorySpace(m_currentMemorySpace);

			m_memorySpaceNodeListCache.clear();

			m_topLevelDepth = 0;
        }

        public IEnumerator<Status> GetEnumerator()
        {
            while (ExecuteNextStatement())
            {
                yield return Status.OK;
            }
			if(m_errorHandler.getErrors().Count > 0) {
				yield return Status.ERROR;
			}
			else {
            	yield return Status.FINISHED;
			}
        }

        private void PushNewScope(Scope newScope, string nameOfNewMemorySpace, AST startNode) {
			Debug.Assert(newScope != null);
			Debug.Assert(startNode != null);
            m_currentScope = newScope;
            m_memorySpaceStack.Push(m_currentMemorySpace);
            m_currentMemorySpace = new MemorySpace(nameOfNewMemorySpace, startNode, m_currentScope, m_memorySpaceNodeListCache);
            m_currentScope.PushMemorySpace(m_currentMemorySpace);
#if LOG_SCOPES
			Console.WriteLine("Pushed new scope " + newScope.getName() + " with memory space " + m_currentMemorySpace.getName() + " " + DumpStack());
#endif
        }

        private void PopCurrentScope()
        {
            m_currentScope.PopMemorySpace();
            m_currentMemorySpace = m_memorySpaceStack.Pop();
            m_currentScope = m_currentMemorySpace.Scope;
            m_currentScope.PushMemorySpace(m_currentMemorySpace);
#if LOG_SCOPES
            Console.WriteLine("Popped back to scope " + m_currentScope.getName());
#endif
        }

        private void PrintValueStack()
        {
            Console.Write("VALUE_STACK: ");
            foreach (ReturnValue rv in m_valueStack)
            {
                Console.Write(rv.ToString() + ", ");
            }
            Console.Write("\n");
        }

        public void PrintMemoryStack()
        {
            Console.WriteLine("MEMORY_STACK:");
            Console.WriteLine("\t" + m_currentMemorySpace.getName() + ":");
            m_currentMemorySpace.PrintValues();
            foreach (MemorySpace m in m_memorySpaceStack)
            {
                Console.WriteLine("\t" + m.getName() + ":");
                m.PrintValues();
            }
        }
		
        public void SwapStackTopValueTo(ReturnValue pValue)
        {
			if(m_valueStack.Count > 0) {
            	m_valueStack.Pop();
            	m_valueStack.Push(pValue);
			} else {
				System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
				Console.WriteLine("SwapStackTopValueTo '" + pValue + "' but stack is empty, stacktrace: " + t.ToString());
				throw new Error("Can't return value (stack empty)");
			}
        }

		public void SetProgramToExecuteFunction (string functionName, ReturnValue[] args)
		{
			//Console.WriteLine ("Will execute '" + functionName + "' in global scope '" + m_globalScope + "'");

			FunctionSymbol functionSymbol = (FunctionSymbol)m_globalScope.resolve(functionName);
			//Console.WriteLine("Found function symbol: " + functionSymbol.ToString());

			if(functionSymbol == null) {
				throw new Error("Can't find function '" + functionName + "' in program");
			}

			if (IsFunctionExternal(functionName)) {
				CallExternalFunction(functionName, args);
			} else {
				AST_FunctionDefinitionNode functionDefinitionNode = (AST_FunctionDefinitionNode)functionSymbol.getFunctionDefinitionNode();

				if (functionDefinitionNode != null) {

					int nrOfParameters = functionDefinitionNode.getChild(2).getChildren().Count;
					if (nrOfParameters != args.Length) {
						throw new Error ("The function " + functionName + " takes " + nrOfParameters + " arguments, not " + args.Length);
					}

					Reset();

					m_topLevelDepth = 1;

					m_currentScope = functionDefinitionNode.getScope();
					m_currentScope.ClearMemorySpaces();

					string nameOfNewMemorySpace = functionName + "_memorySpace" + functionCounter++;
					PushNewScope(functionDefinitionNode.getScope(), nameOfNewMemorySpace, functionDefinitionNode);

					for (int i = args.Length - 1; i >= 0; i--) {
						PushValue(args[i]); // reverse order
					}

					//Console.WriteLine ("Ready to start running function '" + functionName + "' with memory space '" + nameOfNewMemorySpace + "'");
				} else {
					throw new Error(functionName + " has got no function definition node!");
				}
			}
		}
			
		public ReturnValue GetGlobalVariableValue(string pName) 
		{
            return m_globalMemorySpace.getValue(pName);
		}

        private bool ExecuteNextStatement() {
            Debug.Assert(m_currentMemorySpace != null);

            while (!m_currentMemorySpace.Next())
            {
                //Console.WriteLine(m_currentMemorySpace.getName() + " is out of statements to execute.");
				//Console.WriteLine (DumpStack ());

				if (m_memorySpaceStack.Count == m_topLevelDepth)
                {
                    //Console.WriteLine("Stack is empty, finishing execution.");
                    return false;
                }
                else
                {
                    PopCurrentScope();
                }
            }
        
#if WRITE_DEBUG_INFO
            Console.WriteLine("\nExecuting " + CurrentNode.getTokenString());
#endif
			try {
				SwitchOnStatement();
			}
			catch(Error e) {
				if(e.getLineNr() < 0) {
					m_errorHandler.errorOccured(new Error(e.getMessage(), e.getErrorType(), CurrentNode.getToken().LineNr, CurrentNode.getToken().LinePosition));
				}
				else {
					m_errorHandler.errorOccured(e);
				}				
			}

            return true;
        }
		
		private void SwitchOnStatement() 
		{
			CurrentNode.Executions++;
			
            switch (CurrentNode.getTokenType())
            {

                case Token.TokenType.STATEMENT_LIST:
                case Token.TokenType.NODE_GROUP:
                case Token.TokenType.BUILT_IN_TYPE_NAME:

                    // do nothing
                    break;

                case Token.TokenType.IF:
                    EvaluateIf();
                    break;

                case Token.TokenType.PARAMETER:
                    break;

                case Token.TokenType.FUNCTION_CALL:
                    JumpToFunction();
                    break;

                case Token.TokenType.QUOTED_STRING:
                    PushValueFromToken();
                    break;

                case Token.TokenType.FUNC_DECLARATION:
                    throw new Exception("Can't happen");

                case Token.TokenType.VAR_DECLARATION:
                    VariableDeclaration();
                    break;

                case Token.TokenType.ASSIGNMENT:
					AssignmentSignal();
                    break;
				
				case Token.TokenType.ASSIGNMENT_TO_ARRAY:
                    AssignmentToArrayElementSignal();
                    break;
				
				case Token.TokenType.ARRAY_END_SIGNAL:
                    ArrayEndSignal();
                    break;

                case Token.TokenType.NAME:
                    ResolveVariableName();
                    break;

                case Token.TokenType.NUMBER:
                    PushValueFromToken();
                    break;
				
				case Token.TokenType.ARRAY:
                    PushValueFromToken();
                    break;
				
				case Token.TokenType.BOOLEAN_VALUE:
                    PushValueFromToken();
                    break;

                case Token.TokenType.OPERATOR:
                    Operator();
                    break;

                case Token.TokenType.RETURN:
                    ReturnSignal();
                    break;

                case Token.TokenType.LOOP:
                    Loop();
                    break;
				
				case Token.TokenType.LOOP_BLOCK:
                    LoopBlock();
                    break;
				
				case Token.TokenType.GOTO_BEGINNING_OF_LOOP:
					GotoBeginningOfLoop();
					break;
	
                case Token.TokenType.BREAK:
                    BreakStatement();
                    break;
				
				case Token.TokenType.ARRAY_LOOKUP:
                    ArrayLookup();
                    break;

                default:
                    throw new Exception("Hasn't implemented support for token type " + m_currentMemorySpace.CurrentNode.getTokenType() + " yet!");
            }
		}

        static int ifCounter = 0;

        private void EvaluateIf()
        {
            AST_IfNode ifnode = CurrentNode as AST_IfNode;
            Debug.Assert(ifnode != null);

            ReturnValue r = PopValue();
            Debug.Assert(r != null);

            AST subNode = null;

            if (r.NumberValue > 0.0f)
            {
                subNode = ifnode.getChild(1);
            }
            else
            {
                if (ifnode.getChildren().Count == 3)
                {
                    subNode = ifnode.getChild(2);
                }
                else
                {
                    //Console.WriteLine("There is no else-clause in statement");
                }
            }

            if (subNode != null)
            {
                //Console.WriteLine("entering node");
                PushNewScope(ifnode.getScope(), "IF_memorySpace" + ifCounter++, subNode);                
            }
            else
            {
                //Console.WriteLine("can't enter node");
            }
        }

        private AST CurrentNode
        {
            get
            {
                Debug.Assert(m_currentMemorySpace != null);
                return m_currentMemorySpace.CurrentNode;
            }
        }

        static int functionCounter = 0;

		bool IsFunctionExternal(string pFunctionName)
		{
			return m_externalFunctionCreator.externalFunctions.ContainsKey(pFunctionName);
		}

		void CallExternalFunction(string pFunctionName, ReturnValue[] pParameters)
		{
			//Console.WriteLine("Calling external function " + functionName);
			ExternalFunctionCreator.OnFunctionCall fc = m_externalFunctionCreator.externalFunctions[pFunctionName];
			ReturnValue rv = fc(pParameters);
			if (rv.getReturnValueType() != ReturnValueType.VOID) {
				PushValue(rv);
			}
		}

        private void JumpToFunction()
        {
			AST_FunctionDefinitionNode functionDefinitionNode = (CurrentNode as AST_FunctionCall).FunctionDefinitionRef;
            string functionName = functionDefinitionNode.getChild(1).getTokenString();

            int nrOfParameters = functionDefinitionNode.getChild(2).getChildren().Count;
            ReturnValue[] parameters = new ReturnValue[nrOfParameters];
            for (int i = nrOfParameters - 1; i >= 0; i--)
            {
                parameters[i] = PopValue();
            }

			if (IsFunctionExternal(functionName)) {
				CallExternalFunction(functionName, parameters);
			} else {
				PushNewScope(functionDefinitionNode.getScope(), functionName + "_memorySpace" + functionCounter++, functionDefinitionNode);

				for (int i = nrOfParameters - 1; i >= 0; i--) {
					PushValue(parameters[i]); // reverse order
				}
			}
        }

        private void Operator()
        {
            ReturnValue result;
            float rhs, lhs;

            switch (CurrentNode.getTokenString())
            {
                case "+":
                    result = AddStuffTogetherHack();
                    break;

                case "-":
                    rhs = PopValue().NumberValue;
                    lhs = PopValue().NumberValue;
                    result = new ReturnValue(lhs - rhs);
                    break;

                case "*":
                    result = new ReturnValue(PopValue().NumberValue * PopValue().NumberValue);
                    break;

                case "/":
                    rhs = PopValue().NumberValue;
                    lhs = PopValue().NumberValue;
                    result = new ReturnValue(lhs / rhs);
                    break;
                case "<":
                    rhs = PopValue().NumberValue;
                    lhs = PopValue().NumberValue;
                    result = new ReturnValue(lhs < rhs);
                    break;
                case ">":
                    rhs = PopValue().NumberValue;
                    lhs = PopValue().NumberValue;
                    result = new ReturnValue(lhs > rhs);
                    break;
				case ">=":
                    rhs = PopValue().NumberValue;
                    lhs = PopValue().NumberValue;
                    result = new ReturnValue(lhs >= rhs);
                    break;
				case "<=":
                    rhs = PopValue().NumberValue;
                    lhs = PopValue().NumberValue;
                    result = new ReturnValue(lhs <= rhs);
                    break;
				case "==":
                    result = equalityTest();
                    break;
				case "!=":
                    result = new ReturnValue(!equalityTest().BoolValue);
                    break;
                case "&&":
					result = new ReturnValue(PopValue().BoolValue && PopValue().BoolValue);
                    break;
				case "||":
					result = new ReturnValue(PopValue().BoolValue || PopValue().BoolValue);
					break;

                default:
                    throw new Exception("Operator " + CurrentNode.getTokenString() + " is not implemented yet!");
            }

            //Console.WriteLine("Executing operator " + CurrentNode.getTokenString() + " with result " + result);
			
            PushValue(result);
        }
		
		private ReturnValue equalityTest() {
			ReturnValue rhs = PopValue();
            ReturnValue lhs = PopValue();
			
			if(rhs.getReturnValueType() == ReturnValueType.NUMBER && 
			   lhs.getReturnValueType() == ReturnValueType.NUMBER) 
			{
				return new ReturnValue(lhs.NumberValue == rhs.NumberValue);
			}
			
			if(rhs.getReturnValueType() == ReturnValueType.BOOL && 
			   lhs.getReturnValueType() == ReturnValueType.BOOL) 
			{
				return new ReturnValue(lhs.BoolValue == rhs.BoolValue);
			}
			
			if(rhs.getReturnValueType() == ReturnValueType.STRING && 
			   lhs.getReturnValueType() == ReturnValueType.STRING) 
			{
				return new ReturnValue(lhs.StringValue.ToLower() == rhs.StringValue.ToLower());
			}
			
			throw new Error("Can't compare those two things (" + lhs.ToString() + " of type " + lhs.getReturnValueType() + " and " + rhs.ToString() + " of type " + rhs.getReturnValueType() + ")");
		}
		
		private ReturnValue AddStuffTogetherHack() {
		
			ReturnValue rhs = PopValue();
			ReturnValue lhs = PopValue();
				
			if( rhs.getReturnValueType() == ReturnValueType.NUMBER && 
				lhs.getReturnValueType() == ReturnValueType.NUMBER) {
				return new ReturnValue(rhs.NumberValue + lhs.NumberValue);
			}
			else if( rhs.getReturnValueType() != ReturnValueType.STRING && 
					 lhs.getReturnValueType() != ReturnValueType.STRING) {
				SortedDictionary<int, ReturnValue> newArray = new SortedDictionary<int, ReturnValue>();
				//int totalLength = lhs.ArrayValue.Count + rhs.ArrayValue.Count;
				//ReturnValue[] array = new ReturnValue[totalLength];
				for(int i = 0; i < lhs.ArrayValue.Count; i++) {
					newArray.Add(i, lhs.ArrayValue[i]);
				}
				for(int i = 0; i < rhs.ArrayValue.Count; i++) {
					newArray.Add(i + lhs.ArrayValue.Count, rhs.ArrayValue[i]);
				}
				return new ReturnValue(newArray);
			}
			else {
				return new ReturnValue(lhs.ToString() + rhs.ToString());
			}		
		}

        private void ResolveVariableName()
        {
            ReturnValue value = m_currentScope.getValue(CurrentNode.getTokenString());
            PushValue(value);
        }
		
		private void ArrayLookup() 
		{
			ReturnValue index = PopValue();
			ReturnValue array = m_currentScope.getValue(CurrentNode.getTokenString());
			int i = (int)index.NumberValue;
			ReturnValue val = null;
			if(array.ArrayValue.ContainsKey(i)) {
				val = array.ArrayValue[i];
			}
			else {
				val = new ReturnValue(0f);
			}
			PushValue(val);
		}

		void PushValueFromToken ()
		{
			/*#if DEBUG
			if (CurrentNode == null) {
				throw new Exception("Current node is null");
			}
			#endif*/

			TokenWithValue t = CurrentNode.getToken() as TokenWithValue;
			if (t == null) {
				throw new Exception ("Can't convert current node to TokenWithValue: " + CurrentNode + ", it's of type " + CurrentNode.getTokenType());
			}
			PushValue(t.getValue());
		}

        private void VariableDeclaration()
        {
            ReturnValueType type = (CurrentNode as AST_VariableDeclaration).Type;
            string variableName = (CurrentNode as AST_VariableDeclaration).Name;
            ReturnValue initValue = new ReturnValue(type);
            m_currentScope.setValue(variableName, initValue);
        }
		
		private ReturnValue ConvertToType(ReturnValue valueToConvert, ReturnValueType type) {
#if WRITE_CONVERT_INFO
			Console.WriteLine("Converting from " + valueToConvert.getReturnValueType() +
			                  " (" + valueToConvert.ToString() + ")" +
			              	  " to " + type);
#endif
			ReturnValue result = null;
			
			if(type == ReturnValueType.VOID) {
				throw new Error("Can't convert to void.");
			}
			else if(type == ReturnValueType.UNKNOWN_TYPE) {
				// Don't convert but make a new copy with the same type
				type = valueToConvert.getReturnValueType();
			}
			
			if(type == ReturnValueType.ARRAY) {
				result = new ReturnValue(valueToConvert.ArrayValue);
			}
			else if(type == ReturnValueType.BOOL) {
				result = new ReturnValue(valueToConvert.BoolValue);
			}
			else if(type == ReturnValueType.NUMBER) {
				result = new ReturnValue(valueToConvert.NumberValue);
			}
			else if(type == ReturnValueType.STRING) {
				result = new ReturnValue(valueToConvert.StringValue);
			}

#if WRITE_CONVERT_INFO
			Console.WriteLine("Result: " + result.ToString());
#endif
			return result;
		}

        private void AssignmentSignal()
        {
            string variableName = (CurrentNode as AST_Assignment).VariableName;
			ReturnValue expressionValue = PopValue();
			ReturnValueType type = m_currentScope.getValue(variableName).getReturnValueType();
			ReturnValue convertedValue = ConvertToType(expressionValue, type);
			m_currentScope.setValue(variableName, convertedValue);
        }
		
		private void AssignmentToArrayElementSignal() {
			string variableName = (CurrentNode as AST_Assignment).VariableName;
			ReturnValue valueToSet = PopValue();
			int i = (int)PopValue().NumberValue;
			
			ReturnValue rv = m_currentScope.getValue(variableName);
			rv.setType(ReturnValueType.ARRAY);
			
			SortedDictionary<int, ReturnValue> array = rv.ArrayValue;

			if(array.ContainsKey(i)) {
				array[i] = valueToSet;
			}
			else {
				array.Add(i, valueToSet);
			}
		}

        private void ArrayEndSignal() 
		{
			// pop the right number of values and add them to a new ReturnValue of array type
			AST_ArrayEndSignal arrayEndSignal = CurrentNode as AST_ArrayEndSignal;
			SortedDictionary<int, ReturnValue> array = new SortedDictionary<int, ReturnValue>();
			ReturnValue[] values = new ReturnValue[arrayEndSignal.ArraySize];
			for(int i = 0; i < arrayEndSignal.ArraySize; i++) {
				values[i] = PopValue();
			}
			//for(int i = 0; i < arrayEndSignal.ArraySize; i++) {
			for(int i = arrayEndSignal.ArraySize - 1; i >= 0; i--) {
				array.Add(arrayEndSignal.ArraySize - i - 1, values[i]);
			}
			PushValue(new ReturnValue(array));
		}

        private void ReturnSignal()
        {
            // Pop back to function-scope
            while ( (m_currentScope.scopeType != Scope.ScopeType.FUNCTION_SCOPE) &&
			        (m_currentScope.scopeType != Scope.ScopeType.MAIN_SCOPE) )
            {
				PopCurrentScope();
			}
			// .. and then pop one more (the actual function scope)
            m_currentMemorySpace.MoveToEnd(); // (must use MoveToEnd to make returning from main possible)
        }
		
		static int loopBlockCounter = 0;
		private void LoopBlock() {
			AST_LoopBlockNode loopBlockNode = CurrentNode as AST_LoopBlockNode;
			Debug.Assert(loopBlockNode != null);			
			PushNewScope(loopBlockNode.getScope(), "LoopBlock_memorySpace" + loopBlockCounter++, loopBlockNode.getChild(0));
		}
		
		static int loopCounter = 0;
        private void Loop()
        {
			AST_LoopNode loopNode = CurrentNode as AST_LoopNode;
			Debug.Assert(loopNode != null);
			PushNewScope(loopNode.getScope(), "Loop_memorySpace" + loopCounter++, loopNode.getChild(0));
        }

        private void BreakStatement()
        {
			// Pop back to loop-scope
			while( (m_currentScope.scopeType != Scope.ScopeType.LOOP_SCOPE) &&
			       (m_currentScope.scopeType != Scope.ScopeType.MAIN_SCOPE) ) 
			{
				PopCurrentScope();
			}
			// .. and then pop one more
			m_currentMemorySpace.MoveToEnd();
        }
		
		private void GotoBeginningOfLoop() 
		{
			//m_currentMemorySpace.MoveToStart();
			PopCurrentScope();
			m_currentMemorySpace.Jump(-1); // move back to the start of the loop
		}
		
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        
        public int stackSize
        {
            get { return m_memorySpaceStack.Count; }
        }

        public string DumpStack()
        {
            System.Text.StringBuilder b = new System.Text.StringBuilder();
			b.Append ("STACK:");
            foreach (MemorySpace s in m_memorySpaceStack)
            {
				b.Append(" " + s.getName());
            }
            return b.ToString();
        }
        
        public ReturnValue PopValue()
        {
			if(m_valueStack.Count == 0) {
				throw new Error("Can't access value (have you forgotten to return a value from a function?)");
			}
#if PRINT_STACK
            	Console.WriteLine("Popping value " + m_valueStack.Peek());
#endif			
            ReturnValue poppedValue = m_valueStack.Pop();
#if PRINT_STACK
			PrintMemoryStack();
            PrintValueStack();
#endif
            return poppedValue;
        }

        public void PushValue(ReturnValue value)
        {
#if PRINT_STACK
            Console.WriteLine("Pushing value " + value);
#endif
            m_valueStack.Push(value);
#if PRINT_STACK
            PrintMemoryStack();
            PrintValueStack();
#endif
        }

        public bool ValueStackIsEmpty()
        {
            return m_valueStack.Count == 0;
        }

        AST m_ast;
        ExternalFunctionCreator m_externalFunctionCreator;
        ErrorHandler m_errorHandler;
        Scope m_globalScope;
        Scope m_currentScope;
        MemorySpace m_globalMemorySpace;
        MemorySpace m_currentMemorySpace;
        Stack<MemorySpace> m_memorySpaceStack = new Stack<MemorySpace>();
        Stack<ReturnValue> m_valueStack = new Stack<ReturnValue>();
		MemorySpaceNodeListCache m_memorySpaceNodeListCache = new MemorySpaceNodeListCache();
		int m_topLevelDepth = 0; // the stack depth at wich the program starts and ends, normally 0 but can be 1 if jumping into a function
    }
}
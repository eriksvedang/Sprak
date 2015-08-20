//#define WRITE_DEBUG_INFO
//#define PRINT_STACK
//#define WRITE_CONVERT_INFO
//#define LOG_SCOPES

//#define BUILT_IN_PROFILING

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
			#if BUILT_IN_PROFILING
			m_profileData.Clear ();
			#endif

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
			#if DEBUG
			Debug.Assert(newScope != null);
			Debug.Assert(startNode != null);
			#endif

			if (m_memorySpaceStack.Count > 100) {
				var token = startNode.getToken ();
				throw new Error ("Stack overflow!", Error.ErrorType.RUNTIME, token.LineNr, token.LinePosition);
			}

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
            foreach (object rv in m_valueStack)
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
		
        public void SwapStackTopValueTo(object pValue)
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

		public bool HasFunction(string functionName) {
			return m_globalScope.resolve(functionName) != null;
		}

		/// <summary>
		/// Sets the program to execute function.
		/// Returns true if the program had the function.
		/// </summary>
		public bool SetProgramToExecuteFunction (string functionName, object[] args)
		{
			//Console.WriteLine ("Will execute '" + functionName + "' in global scope '" + m_globalScope + "'");

			FunctionSymbol functionSymbol = (FunctionSymbol)m_globalScope.resolve(functionName);
			//Console.WriteLine("Found function symbol: " + functionSymbol.ToString());

			if(functionSymbol == null) {
				return false;
			}

			if (IsFunctionExternal(functionName)) {
				CallExternalFunction(functionName, args);
			} else {
				AST_FunctionDefinitionNode functionDefinitionNode = (AST_FunctionDefinitionNode)functionSymbol.getFunctionDefinitionNode();

				if (functionDefinitionNode != null) {

					var parameters = functionDefinitionNode.getChild(2).getChildren();
					int nrOfParameters = parameters.Count;
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
						var declaration = parameters[i].getChild(0) as AST_VariableDeclaration;
						object convertedValue = ReturnValueConversions.ChangeTypeBasedOnReturnValueType(args[i], declaration.Type);
						PushValue(convertedValue); // reverse order
					}

					//Console.WriteLine ("Ready to start running function '" + functionName + "' with memory space '" + nameOfNewMemorySpace + "'");
				} else {
					throw new Error(functionName + " has got no function definition node!");
				}
			}

			return true; // all went well (starting the function)
		}
			
		public object GetGlobalVariableValue(string pName) 
		{
            return m_globalMemorySpace.getValue(pName);
		}

        private bool ExecuteNextStatement() {
			#if DEBUG
            Debug.Assert(m_currentMemorySpace != null);
			#endif

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
			#if DEBUG
            Debug.Assert(ifnode != null);
			#endif

            object r = PopValue();
			#if DEBUG
            Debug.Assert(r != null);
			#endif

            AST subNode = null;

			if (r.GetType() != typeof(bool) && r.GetType() != typeof(float)) {
				var token = ifnode.getToken ();
				throw new Error ("Can't use value " + r + " of type " + ReturnValueConversions.PrettyObjectType (r.GetType()) + " in if-statement", Error.ErrorType.RUNTIME, token.LineNr, token.LinePosition);
			}

			if (ConvertToBool(r))
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
				#if DEBUG
                Debug.Assert(m_currentMemorySpace != null);
				#endif
                return m_currentMemorySpace.CurrentNode;
            }
        }

        static int functionCounter = 0;

		bool IsFunctionExternal(string pFunctionName)
		{
			return m_externalFunctionCreator.externalFunctions.ContainsKey(pFunctionName);
		}

		void CallExternalFunction(string pFunctionName, object[] pParameters)
		{
//			Console.WriteLine("Calling external function " + pFunctionName + " with parameters:");
//			foreach (var p in pParameters) {
//				if (p == null) {
//					Console.WriteLine ("null");
//				} else {
//					Console.WriteLine ("" + p);
//				}
//			}

			ExternalFunctionCreator.OnFunctionCall fc = m_externalFunctionCreator.externalFunctions[pFunctionName];
			object rv = fc(pParameters);
			if (!(rv is VoidType)) {
				PushValue(rv);
			}
		}

        private void JumpToFunction()
        {
			AST_FunctionDefinitionNode functionDefinitionNode = (CurrentNode as AST_FunctionCall).FunctionDefinitionRef;
            string functionName = functionDefinitionNode.getChild(1).getTokenString();

			#if BUILT_IN_PROFILING
			ProfileData data = null;
			if (m_profileData.TryGetValue (functionName, out data)) {
				data.calls++;
			} else {
				m_profileData.Add (functionName, new ProfileData () {
					calls = 1,
					totalTime = 0f,
				});
			}
			#endif

			var parameterDefs = functionDefinitionNode.getChild(2).getChildren();

			//ASTPainter painter = new ASTPainter();

			int nrOfParameters = parameterDefs.Count;
            object[] parameters = new object[nrOfParameters];
            for (int i = nrOfParameters - 1; i >= 0; i--)
            {
				//painter.PaintAST(parameterDefs[i]);

				var paramDef = parameterDefs[i];
				var declaration = paramDef.getChild(0) as AST_VariableDeclaration;

				parameters[i] = ReturnValueConversions.ChangeTypeBasedOnReturnValueType(PopValue(), declaration.Type);

            }

			//try {

				if (IsFunctionExternal(functionName)) {
					CallExternalFunction(functionName, parameters);
				} else {
					PushNewScope(functionDefinitionNode.getScope(), functionName + "_memorySpace" + functionCounter++, functionDefinitionNode);

					for (int i = nrOfParameters - 1; i >= 0; i--) {
						PushValue(parameters[i]); // reverse order
					}
				}

//			}
//			catch(Exception e) {
//				Console.WriteLine("Exception when calling " + functionName + ": " + e.StackTrace);
//				throw e;
//			}
        }

		private float ConvertToNumber(object o) {
			if(o.GetType() == typeof(float)) {
				return (float)o;
			}
			else if(o.GetType() == typeof(int)) {
				return (float)(int)o;
			}
			else if(o.GetType() == typeof(string)) {
				float f = 0f;
				if(float.TryParse((string)o, out f)) {
					return f;
				}
			}

			throw new Error("Can't convert value " + o + " of type " + ReturnValueConversions.PrettyObjectType(o.GetType()) + " to number");
		}

		private bool ConvertToBool(object o) {
			if(o.GetType() == typeof(bool)) {
				return (bool)o;
			}
			else if(o.GetType() == typeof(float)) {
				return ((float)o == 0f ? false : true);
			}
			else if(o.GetType() == typeof(int)) {
				return ((int)o == 0 ? false : true);
			}
			throw new Error("Can't convert value " + o + " of type " + ReturnValueConversions.PrettyObjectType(o.GetType()) + " to bool");
		}

        private void Operator()
        {
            object result;
            float rhs, lhs;

            switch (CurrentNode.getTokenString())
            {
                case "+":
                    result = AddStuffTogetherHack();
                    break;

                case "-":
					rhs = ConvertToNumber(PopValue());
                    lhs = ConvertToNumber(PopValue());
                    result = lhs - rhs;
                    break;

                case "*":
                    result = ConvertToNumber(PopValue()) * ConvertToNumber(PopValue());
                    break;

                case "/":
                    rhs = ConvertToNumber(PopValue());
                    lhs = ConvertToNumber(PopValue());
                    result = lhs / rhs;
                    break;
                case "<":
                    rhs = ConvertToNumber(PopValue());
                    lhs = ConvertToNumber(PopValue());
                    result = lhs < rhs;
                    break;
                case ">":
                    rhs = ConvertToNumber(PopValue());
                    lhs = ConvertToNumber(PopValue());
                    result = lhs > rhs;
                    break;
				case ">=":
                    rhs = ConvertToNumber(PopValue());
                    lhs = ConvertToNumber(PopValue());
                    result = lhs >= rhs;
                    break;
				case "<=":
                    rhs = ConvertToNumber(PopValue());
                    lhs = ConvertToNumber(PopValue());
                    result = lhs <= rhs;
                    break;
				case "==":
                    result = equalityTest();
                    break;
				case "!=":
					result = !ConvertToBool(PopValue());
                    break;
                case "&&":
					result = ConvertToBool(PopValue()) && ConvertToBool(PopValue());
                    break;
				case "||":
					result = ConvertToBool(PopValue()) || ConvertToBool(PopValue());
					break;

                default:
                    throw new Exception("Operator " + CurrentNode.getTokenString() + " is not implemented yet!");
            }

            //Console.WriteLine("Executing operator " + CurrentNode.getTokenString() + " with result " + result);
			
            PushValue(result);
        }
		
		private object equalityTest() {
			object rhs = PopValue();
            object lhs = PopValue();

			if (lhs == rhs) {
				return true;
			}

			if(lhs.GetType() == typeof(float) && lhs.GetType() == rhs.GetType()) {
				return (((float)rhs) == ((float)lhs));
			}
			else if(lhs.GetType() == typeof(int) && lhs.GetType() == rhs.GetType()) {
				return (((int)rhs) == ((int)lhs));
			}
			else if(lhs.GetType() == rhs.GetType() && rhs is IComparable && lhs is IComparable)
			{
				return (rhs as IComparable).CompareTo(lhs as IComparable) == 0;
			}
						
			//throw new Error("Can't compare those two things (" + lhs.ToString() + " of type " + lhs.GetType() + " and " + rhs.ToString() + " of type " + rhs.GetType() + ")");

			return false;
		}
		
		private object AddStuffTogetherHack() {
		
			object rhs = PopValue();
			object lhs = PopValue();

			var rightValueType = rhs.GetType ();
			var leftValueType = lhs.GetType ();
				
			//Console.WriteLine("Adding " + lhs + " of type " + leftValueType + " together with " + rhs + " of type " + rightValueType);

			if (rightValueType == typeof(float) && leftValueType == typeof(float)) {
				return (float)rhs + (float)lhs;
			} if (rightValueType == typeof(int) && leftValueType == typeof(int)) {
				return (float)((int)rhs + (int)lhs);
			} else if (rightValueType == typeof(string) || leftValueType == typeof(string)) {
				return ReturnValueConversions.PrettyStringRepresenation(lhs) + ReturnValueConversions.PrettyStringRepresenation(rhs);
			} else if (rightValueType == typeof(object[]) && leftValueType == typeof(object[])) {
				throw new Error("Primitive array concatenation is temporarily disabled.");
			} else if (rightValueType == typeof(SortedDictionary<KeyWrapper, object>) && leftValueType == typeof(SortedDictionary<KeyWrapper, object>)) {
				var lhsArray = lhs as SortedDictionary<KeyWrapper, object>;
				var rhsArray = rhs as SortedDictionary<KeyWrapper, object>;
				var newArray = new SortedDictionary<KeyWrapper, object>();
				for(int i = 0; i < lhsArray.Count; i++) {
					newArray.Add(new KeyWrapper(i), lhsArray[new KeyWrapper(i)]);
				}
				for(int i = 0; i < rhsArray.Count; i++) {
					newArray.Add(new KeyWrapper(i + lhsArray.Count), rhsArray[new KeyWrapper(i)]);
				}
				Console.WriteLine ("Created new array by concatenation: " + ReturnValueConversions.PrettyStringRepresenation(newArray));
				return newArray;
			}
			else {
				throw new Error ("Can't add " + lhs + " to " + rhs);
			}		
		}

        private void ResolveVariableName()
        {
            object value = m_currentScope.getValue(CurrentNode.getTokenString());
            PushValue(value);
        }
		
		private void ArrayLookup() 
		{
			object index = PopValue();
			object array = m_currentScope.getValue(CurrentNode.getTokenString());
			object val = null;

			if (array is Range) {
				//Console.WriteLine ("LOOKING UP KEY " + index + " IN RANGE " + array.ToString ());

				if (index.GetType () == typeof(float)) {
					Range range = (Range)array;
					float i = range.step * (int)(float)index;
					float theNumber = range.start + i;
					float lowerBound = 0;
					float upperBound = 0;
					if (range.step > 0) {
						lowerBound = range.start;
						upperBound = range.end;
					} else {
						lowerBound = range.end;
						upperBound = range.start;
					}
					if (theNumber < lowerBound) {
						throw new Error ("Index " + index.ToString () + " is outside the range " + array.ToString ());
					} else if (theNumber > upperBound) {
						throw new Error ("Index " + index.ToString () + " is outside the range " + array.ToString ());
					}
					val = (float)theNumber;
					//Console.WriteLine("The result was " + val);
				} else {
					throw new Error ("Can't look up " + index.ToString () + " in the range " + array.ToString ());
				}

			} else if (array.GetType () == typeof(SortedDictionary<KeyWrapper,object>)) {
				//Console.WriteLine ("LOOKING UP KEY " + index + " of type " + index.GetType() + " IN ARRAY " + ReturnValueConversions.PrettyStringRepresenation(array));

				var a = array as SortedDictionary<KeyWrapper,object>;

				if (a.TryGetValue(new KeyWrapper(index), out val)) {
					//Console.WriteLine("The result was " + val);
				} else {
					throw new Error ("Can't find the index '" + index + "' (" + index.GetType () + ") in the array '" + CurrentNode.getTokenString () + "'", Error.ErrorType.RUNTIME, CurrentNode.getToken ().LineNr, CurrentNode.getToken ().LinePosition);
				}
			} else if (array.GetType () == typeof(object[])) {
				throw new Exception("object[] array: " + ReturnValueConversions.PrettyStringRepresenation(array));
//				var a = (object[])array;
//				if(index.GetType() != typeof(float)) {
//					throw new Exception("Index " + index + " is of wrong type: " + index.GetType());
//				}
//				int i = (int)(float)index;
//				val = a[i];
			} else if (array.GetType () == typeof(string)) {
				int i = 0;
				if(index.GetType() == typeof(float)) {
					i = (int)(float)index;
				}
				else if(index.GetType() == typeof(int)) {
					i = (int)index;
				} else {
					throw new Error("Must use nr when looking up index in string");
				}
				string s = (string)array;
				if (i >= 0 && i < s.Length) {
					val = s[i].ToString();
				} else {
					throw new Error ("The index '" + i + "' (" + index.GetType () + ") is outside the bounds of the string '" + CurrentNode.getTokenString () + "'", Error.ErrorType.RUNTIME, CurrentNode.getToken ().LineNr, CurrentNode.getToken ().LinePosition);
				}
			} else {
				throw new Error ("Can't convert " + array.ToString () + " to an array (for lookup)");
			}

			PushValue (val);
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
            object initValue = DefaultValue(type);
            m_currentScope.setValue(variableName, initValue);
        }

		object DefaultValue (ReturnValueType type)
		{
			if(type == ReturnValueType.STRING) {
				return "";
			}
			else if(type == ReturnValueType.BOOL) {
				return false;
			}
			else if(type == ReturnValueType.NUMBER) {
				return 0.0f;
			}
			else if(type == ReturnValueType.RANGE) {
				return new Range(0, 0, 0);
			}
			else if(type == ReturnValueType.ARRAY) {
				return new SortedDictionary<KeyWrapper, object>();
			}
			else if(type == ReturnValueType.VOID) {
				return VoidType.voidType;
			}
			else if(type == ReturnValueType.UNKNOWN_TYPE) {
				return UnknownType.unknownType;
			}
			else {
				throw new Error("No default value for " + type);
			}
		}
		
		private object ConvertToType(object valueToConvert, Type type) {
			var returnValueType = ReturnValueConversions.SystemTypeToReturnValueType(type);
//			Console.WriteLine("Assignment of " + ReturnValueConversions.PrettyStringRepresenation(valueToConvert) + " will convert it from " + valueToConvert.GetType() + " to " + type.ToString() + " (" + returnValueType + ")");
			object newObject = ReturnValueConversions.ChangeTypeBasedOnReturnValueType(valueToConvert, returnValueType);
			return newObject;
		}

        private void AssignmentSignal()
        {
            string variableName = (CurrentNode as AST_Assignment).VariableName;
			object expressionValue = PopValue();
			Type type = m_currentScope.getValue(variableName).GetType();
			object convertedValue = ConvertToType(expressionValue, type);
			m_currentScope.setValue(variableName, convertedValue);
        }
		
		private void AssignmentToArrayElementSignal() {
			string variableName = (CurrentNode as AST_Assignment).VariableName;
			object valueToSet = PopValue();
			object index = PopValue();

			object rv = m_currentScope.getValue(variableName);

			if (rv.GetType () == typeof(SortedDictionary<KeyWrapper,object>)) {
				SortedDictionary<KeyWrapper, object> array = rv as SortedDictionary<KeyWrapper, object>;				

				//Console.WriteLine("Checking if index " + index + " of type " + index.GetType() + " is within range of array of length " + array.Count);

				if(array.ContainsKey(new KeyWrapper(index))) {
					array[new KeyWrapper(index)] = valueToSet;
				}
				else {
					array.Add(new KeyWrapper(index), valueToSet);
				}
			}
			else {
				var token = (CurrentNode as AST_Assignment).getToken();
				throw new Error ("Can't assign to the variable '" + variableName + "' since it's of the type " + ReturnValueConversions.PrettyObjectType(rv.GetType()), Error.ErrorType.RUNTIME, token.LineNr, token.LinePosition);
			}
		}

        private void ArrayEndSignal() 
		{
			// pop the right number of values and add them to a new object of array type
			AST_ArrayEndSignal arrayEndSignal = CurrentNode as AST_ArrayEndSignal;
			SortedDictionary<KeyWrapper, object> array = new SortedDictionary<KeyWrapper, object>();
			object[] values = new object[arrayEndSignal.ArraySize];
			for(int i = 0; i < arrayEndSignal.ArraySize; i++) {
				values[i] = PopValue();
			}
			//for(int i = 0; i < arrayEndSignal.ArraySize; i++) {
			for(int i = arrayEndSignal.ArraySize - 1; i >= 0; i--) {
				array.Add(new KeyWrapper(arrayEndSignal.ArraySize - i - 1), values[i]);
			}
			PushValue(array);
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
			#if DEBUG
			Debug.Assert(loopBlockNode != null);			
			#endif
			PushNewScope(loopBlockNode.getScope(), "LoopBlock_memorySpace" + loopBlockCounter++, loopBlockNode.getChild(0));
		}
		
		static int loopCounter = 0;
        private void Loop()
        {
			AST_LoopNode loopNode = CurrentNode as AST_LoopNode;
			#if DEBUG
			Debug.Assert(loopNode != null);
			#endif
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
			b.Append ("[");
            foreach (MemorySpace s in m_memorySpaceStack)
            {
				b.Append(" " + s.getName());
            }
			b.Append (" ]");
            return b.ToString();
        }
        
        public object PopValue()
        {
			if(m_valueStack.Count == 0) {
				throw new Error("Can't access value (have you forgotten to return a value from a function?)");
			}
#if PRINT_STACK
            	Console.WriteLine("Popping value " + m_valueStack.Peek());
#endif			
            object poppedValue = m_valueStack.Pop();
#if PRINT_STACK
			PrintMemoryStack();
            PrintValueStack();
#endif
            return poppedValue;
        }

//		public float PopNumberValue() {
//			object n = PopValue();
//			if(n.GetType() == typeof(float)) {
//				return (float)n;
//			}
//			else if(n.GetType() == typeof(int)) {
//				return (float)(int)n;
//			}
//			else {
//				throw new Error("Can't convert value " + n.ToString() + " of type " + n.GetType() + " to a number");
//			}
//		}
//
//		public bool PopBoolValue() {
//			object n = PopValue();
//			if(n.GetType() == typeof(bool)) {
//				return (bool)n;
//			}
//			else if(n.GetType() == typeof(float)) {
//				return ((float)n != 0f) ? true : false;
//			}
//			else {
//				throw new Error("Can't convert value " + n.ToString() + " of type " + n.GetType() + " to a bool");
//			}
//		}

        public void PushValue(object value)
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

		// Profiling
		#if BUILT_IN_PROFILING
		public Dictionary<string, ProfileData> profileData {
			get {
				return m_profileData;
			}
		}
		public bool profilingOn = true;
		Dictionary<string, ProfileData> m_profileData = new Dictionary<string, ProfileData> ();
		#endif

		// Members
        AST m_ast;
        ExternalFunctionCreator m_externalFunctionCreator;
        ErrorHandler m_errorHandler;
        Scope m_globalScope;
        Scope m_currentScope;
        MemorySpace m_globalMemorySpace;
        MemorySpace m_currentMemorySpace;
        Stack<MemorySpace> m_memorySpaceStack = new Stack<MemorySpace>();
        Stack<object> m_valueStack = new Stack<object>();
		MemorySpaceNodeListCache m_memorySpaceNodeListCache = new MemorySpaceNodeListCache();
		int m_topLevelDepth = 0; // the stack depth at wich the program starts and ends, normally 0 but can be 1 if jumping into a function
    }
}


public class ProfileData {
	public int calls;
	public float totalTime;
}

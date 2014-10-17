using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProgrammingLanguageNr1
{

    public class SprakRunner
	{
        private static List<FunctionDefinition> __CreateBuiltInFunctionDefinitions()
        {
            List<FunctionDefinition> result = new List<FunctionDefinition>();
            FunctionDocumentation functionDoc_Count =
        new FunctionDocumentation("Count the number of elements in an array", new string[] { "The array" });
            result.Add(new FunctionDefinition("number", "Count", new string[] { "array" }, new string[] { "a" }, new ExternalFunctionCreator.OnFunctionCall(API_count), functionDoc_Count));

			// Need an add-function???!
//			FunctionDocumentation functionDoc_add =
//				new FunctionDocumentation("Add an element to the end of the array", new string[] { "The element" });
//			result.Add(new FunctionDefinition("array", "add", new string[] { "var" }, new string[] { "element" }, new ExternalFunctionCreator.OnFunctionCall(API_add), functionDoc_add));

//            FunctionDocumentation functionDoc_allocate =
//                new FunctionDocumentation("Create a new array with X number of elements", new string[] { "How many elements the array should hold" });
//            result.Add(new FunctionDefinition("number", "allocate", new string[] { "number" }, new string[] { "X" }, new ExternalFunctionCreator.OnFunctionCall(API_allocate), functionDoc_allocate));

            FunctionDocumentation functionDoc_Range =
				new FunctionDocumentation("Create a range of numbers from 'min' to (and including) 'max'", new string[] { "The start value of the range", "The end value of the range" });
            result.Add(new FunctionDefinition("number", "Range", new string[] { "number", "number" }, new string[] { "min", "max" }, new ExternalFunctionCreator.OnFunctionCall(API_range), functionDoc_Range));

//            FunctionDocumentation functionDoc_toArray =
//                new FunctionDocumentation("Convert something to an array", new string[] { "The value to convert" });
//            result.Add(new FunctionDefinition("array", "toArray", new string[] { "var" }, new string[] { "value" }, new ExternalFunctionCreator.OnFunctionCall(API_toArray), functionDoc_toArray));
//
//            FunctionDocumentation functionDoc_toNumber =
//                new FunctionDocumentation("Convert something to a number", new string[] { "The value to convert" });
//            result.Add(new FunctionDefinition("number", "toNumber", new string[] { "var" }, new string[] { "value" }, new ExternalFunctionCreator.OnFunctionCall(API_toNumber), functionDoc_toNumber));
//
//            FunctionDocumentation functionDoc_toString =
//                new FunctionDocumentation("Convert something to a string", new string[] { "The value to convert" });
//            result.Add(new FunctionDefinition("string", "toString", new string[] { "var" }, new string[] { "value" }, new ExternalFunctionCreator.OnFunctionCall(API_toString), functionDoc_toString));
//
//            FunctionDocumentation functionDoc_toBool =
//                new FunctionDocumentation("Convert something to a bool", new string[] { "The value to convert" });
//            result.Add(new FunctionDefinition("bool", "toBool", new string[] { "var" }, new string[] { "value" }, new ExternalFunctionCreator.OnFunctionCall(API_toBool), functionDoc_toBool));

            FunctionDocumentation functionDoc_GetIndexes =
                new FunctionDocumentation("Create a new array that contains the indexes of another array", new string[] { "The array with indexes" });
            result.Add(new FunctionDefinition("array", "GetIndexes", new string[] { "array" }, new string[] { "a" }, new ExternalFunctionCreator.OnFunctionCall(API_createArrayOrRangeOfIndexes), functionDoc_GetIndexes));

            FunctionDocumentation functionDoc_RemoveElement =
                new FunctionDocumentation("Remove an element from an array", new string[] { "The array to remove an element from", "The index in the array to remove" });
            result.Add(new FunctionDefinition("void", "Remove", new string[] { "array", "number" }, new string[] { "array", "position" }, new ExternalFunctionCreator.OnFunctionCall(API_removeElement), functionDoc_RemoveElement));

			FunctionDocumentation functionDoc_HasIndex =
				new FunctionDocumentation("Check if an index is in the array", new string[] { "The array to check in", "The index to check for in the array" });
			result.Add(new FunctionDefinition("bool", "HasIndex", new string[] { "array", "var" }, new string[] { "array", "key" }, new ExternalFunctionCreator.OnFunctionCall(API_hasKey), functionDoc_HasIndex));

			FunctionDocumentation functionDoc_Append =
				new FunctionDocumentation("Add an element to the end of an array", new string[] { "The array to add an element to", "The element to add" });
			result.Add(new FunctionDefinition("void", "Append", new string[] { "array", "var" }, new string[] { "array", "elem" }, new ExternalFunctionCreator.OnFunctionCall(API_append), functionDoc_Append));

//            FunctionDocumentation functionDoc_type =
//                new FunctionDocumentation("Get the type of something (returns a string)", new string[] { "The value to get the type of" });
//            result.Add(new FunctionDefinition("string", "type", new string[] { "var" }, new string[] { "value" }, new ExternalFunctionCreator.OnFunctionCall(API_type), functionDoc_type));

			FunctionDocumentation functionDoc_Round =
				new FunctionDocumentation("Round a number to the nearest integer", new string[] { "The number to round" });
			result.Add(new FunctionDefinition("number", "Round", new string[] { "var" }, new string[] { "x" }, new ExternalFunctionCreator.OnFunctionCall(API_round), functionDoc_Round));

			FunctionDocumentation functionDoc_Int =
				new FunctionDocumentation("Remove the decimals of a float", new string[] { "The number to convert to an integer" });
			result.Add(new FunctionDefinition("number", "Int", new string[] { "var" }, new string[] { "x" }, new ExternalFunctionCreator.OnFunctionCall(API_int), functionDoc_Int));

			FunctionDocumentation functionDoc_Mod =
				new FunctionDocumentation("Remove the decimals of a float", new string[] { "Get the remainder of x / y" });
			result.Add(new FunctionDefinition("number", "Mod", new string[] { "var", "var" }, new string[] { "x", "y" }, new ExternalFunctionCreator.OnFunctionCall(API_mod), functionDoc_Mod));

            return result;
        }
        private static List<FunctionDefinition> __builtInFunctions = null;
        public static List<FunctionDefinition> builtInFunctions 
        { 
            get {
                if (__builtInFunctions == null)
                    __builtInFunctions = __CreateBuiltInFunctionDefinitions();
                return __builtInFunctions;        
            } 
        }
		public SprakRunner (TextReader stream, FunctionDefinition[] functionDefinitions)
		{
            construct(stream, functionDefinitions, null);
		}
		
		public SprakRunner (TextReader stream, FunctionDefinition[] functionDefinitions, VariableDefinition[] variableDefinitions)
		{
            construct(stream, functionDefinitions, variableDefinitions);
		}
		
		private void construct(TextReader stream, FunctionDefinition[] functionDefinitions, VariableDefinition[] variableDefinitions)
        {
			Debug.Assert(stream != null);
            Debug.Assert(functionDefinitions != null);
			
            m_compileTimeErrorHandler = new ErrorHandler();
			m_runtimeErrorHandler = new ErrorHandler();
            m_tokens = Tokenize(stream);

			try {
				//PrintTokens ();

	            m_ast = Parse(m_tokens);
				if(m_compileTimeErrorHandler.getErrors().Count > 0) { m_compileTimeErrorHandler.printErrorsToConsole(); return; }

				//PaintAST(m_ast);

				AddLocalVariables(m_ast, variableDefinitions);
				ExternalFunctionCreator externalFunctionCreator = AddExternalFunctions(functionDefinitions, m_ast);
				Scope globalScope = CreateScopeTree(m_ast);
				
				if(m_compileTimeErrorHandler.getErrors().Count > 0) { m_compileTimeErrorHandler.printErrorsToConsole(); return; }
				
				m_interpreter = new InterpreterTwo(m_ast, globalScope, m_runtimeErrorHandler, externalFunctionCreator);
	            m_started = false;
			}
			catch(Error e) {
				m_compileTimeErrorHandler.errorOccured (e);
				return;
			}
		}

		List<Token> Tokenize (TextReader stream)
		{
            Tokenizer tokenizer = new Tokenizer(m_compileTimeErrorHandler, true);
			return tokenizer.process(stream);
		}
        
        // Used for the getInput() function
        public void SwapStackTopValueTo(ReturnValue pValue)
        {
			if (m_interpreter == null) {
				// TODO: this can happen, like when a function resets the code of itself when running
				return;
			}
			if(pValue == null) {
				return;
			}
            m_interpreter.SwapStackTopValueTo(pValue);
        }
		
		private AST Parse(List<Token> tokens)
        {
            Parser parser = new Parser(tokens, m_compileTimeErrorHandler);
            parser.process();
            AST ast = parser.getAST();
            return ast;
        }
		
		void AddLocalVariables (AST ast, VariableDefinition[] variableDefinitions)
		{
			AST nodeForDefiningGlobalVariables = ast.getChild(0).getChild(0);
			
			if(variableDefinitions == null) { return; }
			
			foreach(VariableDefinition vd in variableDefinitions) {
				                                
				Token token = new Token(Token.TokenType.VAR_DECLARATION,"<VAR_DECL>", ast.getToken().LineNr, ast.getToken().LinePosition);
				
				AST_VariableDeclaration declarationTree =
					new AST_VariableDeclaration(token,
					                            vd.initValue.getReturnValueType(), 
					                            vd.variableName);
				
				if(vd.initValue != null) 
				{                    
					AST assignmentTree = CreateAssignmentTreeFromInitValue(vd.variableName, vd.initValue);
					AST declarationAndAssignmentTree = new AST(new Token(Token.TokenType.STATEMENT_LIST, "<DECLARATION_AND_ASSIGNMENT>", declarationTree.getToken().LineNr, declarationTree.getToken().LinePosition));
					declarationAndAssignmentTree.addChild(declarationTree);
					declarationAndAssignmentTree.addChild(assignmentTree);
					nodeForDefiningGlobalVariables.addChild(declarationAndAssignmentTree);
				}
				else 
				{
					nodeForDefiningGlobalVariables.addChild(declarationTree);
				}
			}
		}
		
		private AST CreateAssignmentTreeFromInitValue(string pVariableName, ReturnValue pInitValue) {				
			Token.TokenType tokenType;
			switch(pInitValue.getReturnValueType()) {
				case ReturnValueType.BOOL:
					tokenType = Token.TokenType.BOOLEAN_VALUE;
				break;
				case ReturnValueType.STRING:
					tokenType = Token.TokenType.QUOTED_STRING;
				break;
				case ReturnValueType.NUMBER:
					tokenType = Token.TokenType.NUMBER;
				break;
				case ReturnValueType.ARRAY:
					tokenType = Token.TokenType.ARRAY;
				break;
				case ReturnValueType.VOID:
					throw new Error("Can't assign void to variable");
				default:
					throw new Exception("Forgot to implement support for a type?");
			}
			Token initValueToken = new TokenWithValue(tokenType, pInitValue.ToString(), pInitValue);
			AST assignmentTree = new AST_Assignment(new Token(Token.TokenType.ASSIGNMENT, "="), pVariableName);
			assignmentTree.addChild(initValueToken);
			return assignmentTree;
		}
		
		public void ChangeGlobalVariableInitValue(string pName, ReturnValue pReturnValue) {
			bool foundVariable = false;
			AST statementListTree = m_ast.getChild(0);
			AST globalVarDefs = statementListTree.getChild(0);
			if(globalVarDefs.getTokenString() != "<GLOBAL_VARIABLE_DEFINITIONS_LIST>") { throw new Exception("Wrong node, " + globalVarDefs.getTokenString()); }
			if(globalVarDefs.getChildren() != null && globalVarDefs.getChildren().Count > 0) {
				foreach(AST defAndAssignmentNode in globalVarDefs.getChildren()) {
					AST_Assignment assigmentNode = (AST_Assignment)defAndAssignmentNode.getChild(1);
					if(assigmentNode.VariableName == pName) {
						defAndAssignmentNode.removeChild(1);
						defAndAssignmentNode.addChild(CreateAssignmentTreeFromInitValue(pName, pReturnValue));
						foundVariable = true;
						break;
					}
				}
			}
			if(!foundVariable) {
				throw new Exception("Couldn't find and change the variable " + pName);
			}
		}
		
		public ReturnValue GetGlobalVariableValue(string pName) 
		{
			return m_interpreter.GetGlobalVariableValue(pName);
		}

        private ExternalFunctionCreator AddExternalFunctions(FunctionDefinition[] functionDefinitions, AST ast)
        {
			List<FunctionDefinition> allFunctionDefinitions = new List<FunctionDefinition>();
            allFunctionDefinitions.AddRange(builtInFunctions);
            allFunctionDefinitions.AddRange(functionDefinitions);
            
			
			ExternalFunctionCreator externalFunctionCreator = new ExternalFunctionCreator(allFunctionDefinitions.ToArray());
            AST functionList = ast.getChild(1);
            foreach (AST externalFunction in externalFunctionCreator.FunctionASTs)
            {
                functionList.addChild(externalFunction);
            }
            return externalFunctionCreator;
        }

		public ReturnValue RunFunction(string functionName, ReturnValue[] args) 
		{
			m_interpreter.SetProgramToExecuteFunction (functionName, args);
			run ();
			return GetFinalReturnValue();
		}

		public ReturnValue GetFinalReturnValue()
		{
			if (!m_interpreter.ValueStackIsEmpty()) {
				ReturnValue result = m_interpreter.PopValue();
				//Console.WriteLine("GetFinalReturnValue: " + result);
				return result;
			} else {
				// no value left on the stack to pop, might be OK in some circumstances (like calling a void remote fn with the normal RemoteFunctionCall function)
				//Console.WriteLine("GetFinalReturnValue: Stacksize 0");
				return new ReturnValue(ReturnValueType.VOID);
			}
		}

        private static ReturnValue API_type(ReturnValue[] args)
        {
			return new ReturnValue(args[0].getReturnValueType().ToString());
		}

        private static ReturnValue API_createArrayOrRangeOfIndexes(ReturnValue[] args)
        {
			if (args [0].getReturnValueType () == ReturnValueType.ARRAY) {
				SortedDictionary<ReturnValue, ReturnValue> originalArray = args [0].ArrayValue;
				SortedDictionary<ReturnValue, ReturnValue> newArray = new SortedDictionary<ReturnValue, ReturnValue> ();
				int i = 0;
				foreach (var index in originalArray.Keys) {
					newArray.Add (new ReturnValue (i), index);
					i++;
				}		
				return new ReturnValue (newArray);
			} else if (args [0].getReturnValueType () == ReturnValueType.RANGE) {
				Range r = args [0].RangeValue;
				Range indexRange = new Range (0, Math.Abs (r.end - r.start) + 1, 1);
				//Console.WriteLine ("GetIndexes created index range: " + indexRange);
				return new ReturnValue (indexRange);
			} else if (args [0].getReturnValueType () == ReturnValueType.STRING) {
				var indexRange = new Range (0, args [0].StringValue.Length, 1);
				return new ReturnValue (indexRange);
			}
			else {
				throw new Error("Can't convert " + args[0].ToString() + " to an array in GetIndexes()");
			}
		}

		private static ReturnValue API_hasKey(ReturnValue[] args)
		{
			SortedDictionary<ReturnValue, ReturnValue> array = args[0].ArrayValue;
			ReturnValue index = args[1];
			return new ReturnValue (array.ContainsKey (index));
		}

        private static ReturnValue API_removeElement(ReturnValue[] args)
        {
			SortedDictionary<ReturnValue, ReturnValue> array = args[0].ArrayValue;
			ReturnValue index = args[1];
			if (array.ContainsKey (index)) {
				array.Remove (index);
				return new ReturnValue ();
			} else {
				throw new Error ("Can't remove item with key " + index + " from array");
			}
		}

		private static ReturnValue API_append(ReturnValue[] args)
		{
			SortedDictionary<ReturnValue, ReturnValue> array = args[0].ArrayValue;
			ReturnValue val = args [1];

			// Slow but correct way of doing it:
			int maxArrayIndex = -1;
			foreach (var key in array.Keys) {
				if (key.getReturnValueType () == ReturnValueType.NUMBER &&
				    maxArrayIndex < key.NumberValue) {
					maxArrayIndex = (int)key.NumberValue;
				}
			}
			//int maxArrayIndex = array.Count; // TODO: this is a bug if the array contains sparse indexes or stuff like that

			array.Add (new ReturnValue(maxArrayIndex + 1), val);
			return new ReturnValue(); // void
		}

        private static ReturnValue API_count(ReturnValue[] args)
        {
			if (args [0].getReturnValueType () == ReturnValueType.ARRAY) {
				SortedDictionary<ReturnValue, ReturnValue> array = args[0].ArrayValue;
				return new ReturnValue((float)array.Count);
			}
			else if(args [0].getReturnValueType () == ReturnValueType.RANGE) {
				Range r = args [0].RangeValue;
				int length = r.end - r.start;
				return new ReturnValue ((float)length);
			}
			else if(args [0].getReturnValueType () == ReturnValueType.STRING) {
				return new ReturnValue(args [0].StringValue.Length);
			}
			else {
				throw new Error("Can't convert " + args[0].ToString() + " to an array in Count()");
			}
		}

        private static ReturnValue API_allocate(ReturnValue[] args)
        {
			int size = (int)args[0].NumberValue;
			SortedDictionary<ReturnValue, ReturnValue> array = new SortedDictionary<ReturnValue, ReturnValue>();
			for(int i  = 0; i < size; i++) {
				array.Add(new ReturnValue(i), new ReturnValue(ReturnValueType.NUMBER));
			}
			return new ReturnValue(array);
		}

//		private static ReturnValue API_add(ReturnValue[] args)
//		{
//			ReturnValue elem = args [0];
//
//			array.Add(i, new ReturnValue(ReturnValueType.NUMBER));
//		}

        private static ReturnValue API_range(ReturnValue[] args)
        {
			int start = (int)args[0].NumberValue;
			int end = (int)args[1].NumberValue;

			if (Math.Abs (start - end) > 50) {
				// Create a range
				int step = start < end ? 1 : -1;
				var range = new ReturnValue(new Range (start, end, step));
				//Console.WriteLine ("Created a range: " + range.ToString ());
				return range;
			} else {
				// Create a normal array
				SortedDictionary<ReturnValue, ReturnValue> array = new SortedDictionary<ReturnValue, ReturnValue> ();
			
				int step = 0;
				if (start < end) { 
					step = 1;
					end++;
				} else {
					step = -1;
					end--;
				}		
				int index = 0;
				for (int nr = start; nr != end; nr += step) {
					//Console.WriteLine("nr: " + nr);
					array [new ReturnValue (index)] = new ReturnValue ((float)nr);
					index++;
				}
				return new ReturnValue (array);
			}
		}

        private static ReturnValue API_toArray(ReturnValue[] args)
        {
			return new ReturnValue(args[0].ArrayValue);
		}

        private static ReturnValue API_toNumber(ReturnValue[] args)
        {
			return new ReturnValue(args[0].NumberValue);
		}

        private static ReturnValue API_toString(ReturnValue[] args)
        {
			return new ReturnValue(args[0].StringValue);
		}

        private static ReturnValue API_toBool(ReturnValue[] args)
        {
			return new ReturnValue(args[0].BoolValue);
		}

		private static ReturnValue API_round(ReturnValue[] args)
		{
			return new ReturnValue((float)Math.Round(args[0].NumberValue));
		}

		private static ReturnValue API_int(ReturnValue[] args)
		{
			return new ReturnValue((int)args[0].NumberValue);
		}

		private static ReturnValue API_mod(ReturnValue[] args)
		{
			return new ReturnValue((int)args[0].NumberValue % (int)args[1].NumberValue);
		}
		
		private Scope CreateScopeTree(AST ast)
        {
            ScopeBuilder scopeBuilder = new ScopeBuilder(ast, m_compileTimeErrorHandler);
            scopeBuilder.process();
            Scope globalScope = scopeBuilder.getGlobalScope();
			//Console.WriteLine("\nScopes: \n" + globalScope + "\n\n");
            return globalScope;
        }
		
		private void PaintAST(AST ast) {
			ASTPainter painter = new ASTPainter();
            painter.PaintAST(ast);
		}

		void PrintTokens ()
		{
			Console.WriteLine ("TOKENS");
			Console.WriteLine ("======");
			foreach (var token in m_tokens) {
				Console.WriteLine (" " + token.ToString());
			}
			Console.WriteLine ("======");
		}

        public void run()
        {
            run(100000);
        }
		
        public void run(int pMaxNrOfExecutions) 
		{
            Debug.Assert(pMaxNrOfExecutions < int.MaxValue);

			if (m_compileTimeErrorHandler.getErrors().Count == 0)
            {
                int executions = 0;
                int stacksize = 0;

                foreach (InterpreterTwo.Status s in m_interpreter)
                {
                    stacksize = m_interpreter.stackSize > stacksize ? m_interpreter.stackSize : stacksize;
                    executions++;
					if (executions >= pMaxNrOfExecutions) {
						Console.WriteLine ("\nHit maximum execution count limit!");
						break;
					} else if (m_runtimeErrorHandler.getErrors ().Count > 0) {
						Console.WriteLine ("\nRuntime error occured:");
						m_runtimeErrorHandler.printErrorsToConsole ();
						Console.WriteLine ("Stack: " + m_interpreter.DumpStack ());
						break;
					}
                }

                Console.WriteLine("\nCompleted " + executions + " executions");
                Console.WriteLine("Maximum stacksize was " + stacksize + "\n");
			}
            else 
            {
				//Console.WriteLine("Can't run program since it contains errors!");
                //m_compileTimeErrorHandler.printErrorsToConsole();
			}
		}
        
        public void Reset()
        {
            if (m_programIterator != null)
            {
                m_programIterator.Dispose();
                m_programIterator = null;
            }
            m_started = false;
			if(m_interpreter != null) {
				m_interpreter.Reset();
			}
        }

		/// <summary>
		/// Returns true if the function existed
		/// </summary>
		public bool ResetAtFunction(string functionName, ReturnValue[] args) 
		{
			if (m_interpreter == null) {
				//throw new Exception("Interpreter is null!");
				return false;
			}
			Reset ();
			return m_interpreter.SetProgramToExecuteFunction (functionName, args);
		}

        public bool Start()
        {
            if (m_compileTimeErrorHandler.getErrors().Count != 0)
            {
				//Console.WriteLine("Can't run program since it contains errors!");
                return m_started = false;
            }
            m_programIterator = m_interpreter.GetEnumerator();
            return m_started = true;
        }
        
        public InterpreterTwo.Status Step()
        {
            if (m_compileTimeErrorHandler.getErrors().Count != 0 &&
			    m_runtimeErrorHandler.getErrors().Count != 0 )
            {
                Console.WriteLine("Can't continue to run program since it contains errors!");
                return InterpreterTwo.Status.ERROR;
            }
			
			try {
	            if (m_programIterator.MoveNext()) {
	                return m_programIterator.Current;
	            }
	            else {
	                return InterpreterTwo.Status.FINISHED;
	            }
			}
			catch(Error sprakError) {
				Console.Write ("Caught sprak error in SprakRunner Step(): " + sprakError);
				m_runtimeErrorHandler.errorOccured(sprakError);
				return InterpreterTwo.Status.ERROR;
			}
			/*catch(Exception e) {
				m_runtimeErrorHandler.errorOccured(new Error("Exception: " + e.Message));
				return InterpreterTwo.Status.ERROR;
			}*/
        }

        public bool isStarted
        {
            get { return m_started; }
        }

        public List<Token> Tokens
        {
            get { return m_tokens; }
        }
		
		public void printTree(bool printExecutionCounters)
		{
			if(m_compileTimeErrorHandler.getErrors().Count == 0 &&
			   m_runtimeErrorHandler.getErrors().Count == 0
			   ) {
				ASTPainter p = new ASTPainter();
				p.PrintExecutions = printExecutionCounters;
				p.PaintAST(m_ast);
			}
		}

		public Dictionary<string, ProfileData> GetProfileData() {
			#if BUILT_IN_PROFILING
				return m_interpreter.profileData;
			#else
				return new Dictionary<string, ProfileData>();
			#endif
		}

		public ErrorHandler getCompileTimeErrorHandler() { return m_compileTimeErrorHandler; }
		public ErrorHandler getRuntimeErrorHandler() { return m_runtimeErrorHandler; }
		private AST m_ast;
        private bool m_started;
        private List<Token> m_tokens;
		private InterpreterTwo m_interpreter;
        private ErrorHandler m_compileTimeErrorHandler;
		private ErrorHandler m_runtimeErrorHandler;
        private IEnumerator<InterpreterTwo.Status> m_programIterator;
    }
}


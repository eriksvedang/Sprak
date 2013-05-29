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
            FunctionDocumentation functionDoc_count =
        new FunctionDocumentation("Count the number of elements in an array", new string[] { "The array" });
            result.Add(new FunctionDefinition("number", "count", new string[] { "array" }, new string[] { "a" }, new ExternalFunctionCreator.OnFunctionCall(API_count), functionDoc_count));

            FunctionDocumentation functionDoc_allocate =
                new FunctionDocumentation("Create a new array with X number of elements", new string[] { "How many elements the array should hold" });
            result.Add(new FunctionDefinition("number", "allocate", new string[] { "number" }, new string[] { "X" }, new ExternalFunctionCreator.OnFunctionCall(API_allocate), functionDoc_allocate));

            FunctionDocumentation functionDoc_range =
                new FunctionDocumentation("Create a new array that contains a range of numbers", new string[] { "The start value of the range", "The end value of the range" });
            result.Add(new FunctionDefinition("number", "range", new string[] { "number", "number" }, new string[] { "min", "max" }, new ExternalFunctionCreator.OnFunctionCall(API_range), functionDoc_range));

            FunctionDocumentation functionDoc_toArray =
                new FunctionDocumentation("Convert something to an array", new string[] { "The value to convert" });
            result.Add(new FunctionDefinition("array", "toArray", new string[] { "var" }, new string[] { "value" }, new ExternalFunctionCreator.OnFunctionCall(API_toArray), functionDoc_toArray));

            FunctionDocumentation functionDoc_toNumber =
                new FunctionDocumentation("Convert something to a number", new string[] { "The value to convert" });
            result.Add(new FunctionDefinition("number", "toNumber", new string[] { "var" }, new string[] { "value" }, new ExternalFunctionCreator.OnFunctionCall(API_toNumber), functionDoc_toNumber));

            FunctionDocumentation functionDoc_toString =
                new FunctionDocumentation("Convert something to a string", new string[] { "The value to convert" });
            result.Add(new FunctionDefinition("string", "toString", new string[] { "var" }, new string[] { "value" }, new ExternalFunctionCreator.OnFunctionCall(API_toString), functionDoc_toString));

            FunctionDocumentation functionDoc_toBool =
                new FunctionDocumentation("Convert something to a bool", new string[] { "The value to convert" });
            result.Add(new FunctionDefinition("bool", "toBool", new string[] { "var" }, new string[] { "value" }, new ExternalFunctionCreator.OnFunctionCall(API_toBool), functionDoc_toBool));

            FunctionDocumentation functionDoc_getIndexes =
                new FunctionDocumentation("Create a new array that contains the indexes of another array", new string[] { "The array with indexes" });
            result.Add(new FunctionDefinition("array", "getIndexes", new string[] { "array" }, new string[] { "a" }, new ExternalFunctionCreator.OnFunctionCall(API_createArrayOfArrayIndexes), functionDoc_getIndexes));

            FunctionDocumentation functionDoc_removeElement =
                new FunctionDocumentation("Remove an element from an array", new string[] { "The array to remove an element from", "The position in the array to remove (starts at 0)" });
            result.Add(new FunctionDefinition("void", "removeElement", new string[] { "array", "number" }, new string[] { "array", "position" }, new ExternalFunctionCreator.OnFunctionCall(API_removeElement), functionDoc_removeElement));

            FunctionDocumentation functionDoc_type =
                new FunctionDocumentation("Get the type of something (returns a string)", new string[] { "The value to get the type of" });
            result.Add(new FunctionDefinition("string", "type", new string[] { "var" }, new string[] { "value" }, new ExternalFunctionCreator.OnFunctionCall(API_type), functionDoc_type));
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
            m_ast = Parse(m_tokens);
			if(m_compileTimeErrorHandler.getErrors().Count > 0) { m_compileTimeErrorHandler.printErrorsToConsole(); return; }
			
			AddLocalVariables(m_ast, variableDefinitions);
			ExternalFunctionCreator externalFunctionCreator = AddExternalFunctions(functionDefinitions, m_ast);
			Scope globalScope = CreateScopeTree(m_ast);
			
			if(m_compileTimeErrorHandler.getErrors().Count > 0) { m_compileTimeErrorHandler.printErrorsToConsole(); return; }
			
			m_interpreter = new InterpreterTwo(m_ast, globalScope, m_runtimeErrorHandler, externalFunctionCreator);
            m_started = false; 
			
			//PaintAST(m_ast);
		}

		List<Token> Tokenize (TextReader stream)
		{
            Tokenizer tokenizer = new Tokenizer(m_compileTimeErrorHandler, true);
			return tokenizer.process(stream);
		}
        
        // Used for the getInput() function
        public void SwapStackTopValueTo(ReturnValue pValue)
        {
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

        private static ReturnValue API_type(ReturnValue[] args)
        {
			return new ReturnValue(args[0].getReturnValueType().ToString());
		}

        private static ReturnValue API_createArrayOfArrayIndexes(ReturnValue[] args)
        {
			SortedDictionary<int, ReturnValue> originalArray = args[0].ArrayValue;
			SortedDictionary<int, ReturnValue> newArray = new SortedDictionary<int, ReturnValue>();
			int i = 0;
			foreach(int index in originalArray.Keys) {
				newArray.Add(i, new ReturnValue((float)index));
				i++;
			}		
			return new ReturnValue(newArray);
		}

        private static ReturnValue API_removeElement(ReturnValue[] args)
        {
			SortedDictionary<int, ReturnValue> array = args[0].ArrayValue;
			int index = (int)args[1].NumberValue;
			array.Remove(index);
			return new ReturnValue();
		}

        private static ReturnValue API_count(ReturnValue[] args)
        {
			SortedDictionary<int, ReturnValue> array = args[0].ArrayValue;
			return new ReturnValue((float)array.Count);
		}

        private static ReturnValue API_allocate(ReturnValue[] args)
        {
			int size = (int)args[0].NumberValue;
			SortedDictionary<int, ReturnValue> array = new SortedDictionary<int, ReturnValue>();
			for(int i  = 0; i < size; i++) {
				array.Add(i, new ReturnValue(ReturnValueType.NUMBER));
			}
			return new ReturnValue(array);
		}

        private static ReturnValue API_range(ReturnValue[] args)
        {
			int start = (int)args[0].NumberValue;
			int end = (int)args[1].NumberValue;
			
			SortedDictionary<int, ReturnValue> array = new SortedDictionary<int, ReturnValue>();
			
			int step = 0;
			if(start < end) { 
				step = 1;
				end++;
			} else {
				step = -1;
				end--;
			}		
			int index = 0;
			for(int nr = start; nr != end; nr += step) {
				//Console.WriteLine("nr: " + nr);
				array[index] = new ReturnValue((float)nr);
				index++;
			}
			return new ReturnValue(array);
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
                    if (executions >= pMaxNrOfExecutions)
                    {
                        Console.WriteLine("\nHit maximum execution count limit!");
                        break;
                    }
                }

                Console.WriteLine("\nCompleted " + executions + " executions");
                Console.WriteLine("Maximum stacksize was " + stacksize + "\n");
			}
            else 
            {
				Console.WriteLine("Can't run program since it contains errors!");
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

        public bool Start()
        {
            if (m_compileTimeErrorHandler.getErrors().Count != 0)
            {
                Console.WriteLine("Can't run program since it contains errors!");
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
            if (m_programIterator.MoveNext()) {
                return m_programIterator.Current;
            }
            else {
                return InterpreterTwo.Status.FINISHED;
            }
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


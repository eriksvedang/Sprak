#define WRITE_DEBUG_INFO

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ProgrammingLanguageNr1
{
	public class ScopeBuilder
	{
		public ScopeBuilder (AST ast, ErrorHandler errorHandler)
		{
			Debug.Assert(ast != null);
			Debug.Assert(errorHandler != null);

            m_errorHandler = errorHandler;
			m_ast = ast;
		}
		
		public void process() {
			m_globalScope = new Scope(Scope.ScopeType.MAIN_SCOPE, "global scope");
			m_currentScope = m_globalScope;

            #if WRITE_DEBUG_INFO
            Console.WriteLine("Evaluate scope declarations:");
            #endif

			evaluateScopeDeclarations(m_ast);

            #if WRITE_DEBUG_INFO
            Console.WriteLine("\nEvaluate references:");
            #endif

			evaluateReferences(m_ast);
		}
		
		private void evaluateScopeDeclarations(AST tree) {
			Debug.Assert(tree != null);
			
			if (tree.getTokenType() == Token.TokenType.FUNC_DECLARATION) 
			{
                evaluateFunctionScope(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.IF) {
                evaluateIfScope(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.LOOP) {
                evaluateLoopScope(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.LOOP_BLOCK) {
               	evaluateLoopBlockScope(tree);
			}
			else if (tree.getChildren() != null) 
			{
				evaluateScopeDeclarationsInAllChildren(tree);
			}
		}

        private void evaluateScopeDeclarationsInAllChildren(AST tree)
        {
            foreach (AST subtree in tree.getChildren())
            {
                evaluateScopeDeclarations(subtree);
            }
        }

        private void evaluateFunctionScope(AST tree)
        {
            // Define function name
            ReturnValueType returnType = ReturnValue.getReturnValueTypeFromString(tree.getChild(0).getTokenString());
            string functionName = tree.getChild(1).getTokenString();

            Symbol functionScope = new FunctionSymbol(m_currentScope, functionName, returnType, tree);
            m_globalScope.define(functionScope); // all functions are saved in the global scope
            m_currentScope = (Scope)functionScope;
            AST_FunctionDefinitionNode functionCallNode = (AST_FunctionDefinitionNode)(tree);
            functionCallNode.setScope((Scope)functionScope);

            #if WRITE_DEBUG_INFO
            Console.WriteLine("\nDefined function with name " + functionName + " and return type " + returnType);
            #endif

            // Process the body of the function
            evaluateScopeDeclarations(tree.getChild(3));

            m_currentScope = m_currentScope.getEnclosingScope(); // pop scope
        }

        private void evaluateIfScope(AST tree)
        {
            Scope subscope = new Scope(Scope.ScopeType.IF_SCOPE,"<IF-SUBSCOPE>", m_currentScope);

            m_currentScope = subscope;

            AST_IfNode ifNode = (tree as AST_IfNode);
            Debug.Assert(ifNode != null);

			#if WRITE_DEBUG_INFO
			Console.WriteLine("\nDefined IF-subscope for ifNode at line " + ifNode.getToken().LineNr);
			#endif

            ifNode.setScope(subscope); // save the new scope in the IF-token tree node
            
            // Evaluate expression
            evaluateScopeDeclarationsInAllChildren(tree.getChild(0));

            AST trueNode = ifNode.getChild(1);
            AST falseNode = null;
            if (ifNode.getChildren().Count == 3)
            {
                falseNode = ifNode.getChild(2);
            }

			evaluateScopeDeclarationsInAllChildren(trueNode);
            if (falseNode != null)
            {
                evaluateScopeDeclarationsInAllChildren(falseNode);
            }

            m_currentScope = m_currentScope.getEnclosingScope(); // pop scope
        }
		
		private void evaluateLoopScope(AST tree)
        {
            Scope subscope = new Scope(Scope.ScopeType.LOOP_SCOPE, "<LOOP-SUBSCOPE>", m_currentScope);
			m_currentScope = subscope;

#if WRITE_DEBUG_INFO
            Console.WriteLine("\nDefined LOOP-subscope");
#endif

            AST_LoopNode loopNode = (tree as AST_LoopNode);
            Debug.Assert(loopNode != null);
            evaluateScopeDeclarationsInAllChildren(loopNode);
			loopNode.setScope(m_currentScope);

            m_currentScope = m_currentScope.getEnclosingScope(); // pop scope
        }
		
		private void evaluateLoopBlockScope(AST tree)
        {
            Scope subscope = new Scope(Scope.ScopeType.LOOP_BLOCK_SCOPE, "<LOOP-BLOCK-SUBSCOPE>", m_currentScope);
			m_currentScope = subscope;

#if WRITE_DEBUG_INFO
            Console.WriteLine("\nDefined LOOP BLOCK-subscope");
#endif

            AST_LoopBlockNode loopBlockNode = (tree as AST_LoopBlockNode);
            Debug.Assert(loopBlockNode != null);
            evaluateScopeDeclarationsInAllChildren(loopBlockNode);
			loopBlockNode.setScope(m_currentScope);

            m_currentScope = m_currentScope.getEnclosingScope(); // pop scope
        }

		private void evaluateReferences(AST tree) {
			Debug.Assert(tree != null);
			
			if (tree.getTokenType() == Token.TokenType.VAR_DECLARATION) 
			{
                evaluateReferencesForVAR_DECLARATION(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.ASSIGNMENT) 
			{
                evaluateReferencesForASSIGNMENT(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.ASSIGNMENT_TO_ARRAY) 
			{
				evaluateReferencesForASSIGNMENT_TO_ARRAY(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.FUNC_DECLARATION) 
			{
                evaluateReferencesForFUNC_DECLARATION(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.FUNCTION_CALL) 
			{
                evaluateReferencesForFUNCTION_CALL(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.IF) 
			{
                evaluateReferencesForIF(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.NAME) 
			{
                evaluateReferencesForNAME(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.LOOP_BLOCK) 
			{
                evaluateReferencesForLOOP_BLOCK(tree);
			}
			else if (tree.getTokenType() == Token.TokenType.LOOP) 
			{
                evaluateReferencesForLOOP(tree);
			}
			else 
			{
				evaluateReferencesInAllChildren(tree);
			}
		}
		
        private void evaluateReferencesInAllChildren(AST tree)
        {
            if (tree.getChildren() != null)
            {
                // Go through all other subtrees
                foreach (AST subtree in tree.getChildren())
                {
                    evaluateReferences(subtree);
                }
            }
        }
		
		private void evaluateReferencesForASSIGNMENT(AST tree)
		{
			AST_Assignment assignment = tree as AST_Assignment;
			
			Symbol variableNameSymbol = m_currentScope.resolve(assignment.VariableName);
			if(variableNameSymbol == null) {
				m_errorHandler.errorOccured("Can't assign to undefined variable " + assignment.VariableName,
				                Error.ErrorType.SYNTAX,
				                tree.getToken().LineNr,
				                tree.getToken().LinePosition);
			}
			
			evaluateReferencesInAllChildren(tree);
		}
		
		private void evaluateReferencesForASSIGNMENT_TO_ARRAY(AST tree)
		{
			AST_Assignment assignment = tree as AST_Assignment;
			
			Symbol variableNameSymbol = m_currentScope.resolve(assignment.VariableName);
			if(variableNameSymbol == null) {
				m_errorHandler.errorOccured("Can't assign to undefined array " + assignment.VariableName,
				                Error.ErrorType.SYNTAX,
				                tree.getToken().LineNr,
				                tree.getToken().LinePosition);
			}
			
			evaluateReferencesInAllChildren(tree);
		}

        private void evaluateReferencesForVAR_DECLARATION(AST tree)
        {
            AST_VariableDeclaration varDeclaration = tree as AST_VariableDeclaration;

            ReturnValueType typeToDeclare = varDeclaration.Type;
            string variableName = varDeclaration.Name;

            if (m_currentScope.isDefined(variableName))
            {
                m_errorHandler.errorOccured(
                    new Error("There is already a variable called '" + variableName + "'",
                    Error.ErrorType.LOGIC,
                    tree.getToken().LineNr,
                    tree.getToken().LinePosition));
            }
            else
            {
                m_currentScope.define(new VariableSymbol(variableName, typeToDeclare));
#if WRITE_DEBUG_INFO
                Console.WriteLine("Defined variable with name " + variableName + " and type " + typeToDeclare + " (on line " + tree.getToken().LineNr + ")" + " in " + m_currentScope);
#endif
            }
        }

        private void evaluateReferencesForFUNCTION_CALL(AST tree)
        {
            // Function name:
            string functionName = tree.getTokenString();
            FunctionSymbol function = (FunctionSymbol)m_currentScope.resolve(functionName);

            if (function == null)
            {
                m_errorHandler.errorOccured("Can't find function with name " + functionName, 
				                            Error.ErrorType.SCOPE,
				                            tree.getToken().LineNr,
				                            tree.getToken().LinePosition
				                            );
            }
            else
            {
                #if WRITE_DEBUG_INFO
                Console.WriteLine("Resolved function call with name " + functionName + " (on line " + tree.getToken().LineNr + ")");
                #endif

                // Parameters
                evaluateReferencesInAllChildren(tree);

                AST node = function.getFunctionDefinitionNode();
                AST_FunctionDefinitionNode functionDefinitionTree = (AST_FunctionDefinitionNode)(node);
				
				/*if(functionDefinitionTree.getTokenString() != "<EXTERNAL_FUNC_DECLARATION>") {
                	evaluateReferencesForFUNC_DECLARATION(functionDefinitionTree);
				}*/

                // Setup reference to Function Definition AST node
                AST_FunctionCall functionCallAst = tree as AST_FunctionCall;
                Debug.Assert(functionCallAst != null);
				functionCallAst.FunctionDefinitionRef = functionDefinitionTree;
				
                List<AST> calleeParameterList = functionDefinitionTree.getChild(2).getChildren();

                // Check that the number of arguments is right
                AST callerParameterList = tree.getChild(0);
                List<AST> arguments = callerParameterList.getChildren();

                if (arguments.Count != calleeParameterList.Count)
                {
                    m_errorHandler.errorOccured(
						"Wrong number of arguments to function '" + functionDefinitionTree.getChild(1).getTokenString() + "' , expected " + calleeParameterList.Count + " but got " + arguments.Count
                        , Error.ErrorType.SYNTAX, tree.getToken().LineNr, tree.getToken().LinePosition);
                }
            }
        }


        private void evaluateReferencesForIF(AST tree)
        {
            AST_IfNode ifNode = (AST_IfNode)(tree);
            m_currentScope = (Scope)ifNode.getScope(); // push IF-subscope
            evaluateReferencesInAllChildren(tree);
            m_currentScope = m_currentScope.getEnclosingScope(); // pop scope
        }

        private void evaluateReferencesForNAME(AST tree)
        {
#if DEBUG
			if (m_currentScope == null) {
				throw new Exception ("m_currentScope is null");
			}
			if (tree == null) {
				throw new Exception("tree is null");
			}
#endif

            Symbol symbol = m_currentScope.resolve(tree.getTokenString());
			
			if(symbol == null) {
				m_errorHandler.errorOccured(
				                            new  Error("Can't find anything called '" + tree.getTokenString() + "'", 
				                                       Error.ErrorType.SYNTAX, 
				                                       tree.getToken().LineNr, 
				                                       tree.getToken().LinePosition));
			}
			else if (symbol is FunctionSymbol) {
				m_errorHandler.errorOccured(
				                            new  Error("'" + tree.getTokenString() + "' is a function and must be called with ()", 
				                                       Error.ErrorType.SYNTAX, 
				                                       tree.getToken().LineNr, 
				                                       tree.getToken().LinePosition));
			}

            #if WRITE_DEBUG_INFO
            Console.WriteLine("Resolved symbol with name " + tree.getTokenString() + " (on line " + tree.getToken().LineNr + ")" + " in " + m_currentScope);
            #endif

            evaluateReferencesInAllChildren(tree);
        }

        private void evaluateReferencesForFUNC_DECLARATION(AST tree)
        {
            string functionName = tree.getChild(1).getTokenString();
            m_currentScope = (Scope)m_currentScope.resolve(functionName); // push the scope with the function

            #if WRITE_DEBUG_INFO
            Console.WriteLine("\n Trying to resolve function parameters and body of " + functionName);
            #endif

            evaluateReferencesInAllChildren(tree.getChild(2)); // parameters
            evaluateReferencesInAllChildren(tree.getChild(3)); // function body

            m_currentScope = m_currentScope.getEnclosingScope(); // pop scope
        }   
		
		private void evaluateReferencesForLOOP_BLOCK(AST tree)
        {
			AST_LoopBlockNode loopBlockNode = tree as AST_LoopBlockNode;
            m_currentScope = loopBlockNode.getScope();

            #if WRITE_DEBUG_INFO
            Console.WriteLine("\n Trying to resolve body of loop block");
            #endif

            evaluateReferencesInAllChildren(tree);

            m_currentScope = m_currentScope.getEnclosingScope(); // pop scope
        }  
		
		private void evaluateReferencesForLOOP(AST tree)
        {
			AST_LoopNode loopBlockNode = tree as AST_LoopNode;
            m_currentScope = loopBlockNode.getScope();

            #if WRITE_DEBUG_INFO
            Console.WriteLine("\n Trying to resolve body of loop");
            #endif

            evaluateReferencesInAllChildren(tree);

            m_currentScope = m_currentScope.getEnclosingScope(); // pop scope
        }
		
		public Scope getGlobalScope() {
			Debug.Assert(m_globalScope != null, "The global scope is null, this probably means that you haven't called process() on ScopeBuilder");
			return m_globalScope;
		}
		
		AST m_ast;
		Scope m_globalScope;
		Scope m_currentScope;
		ErrorHandler m_errorHandler;
	}
}


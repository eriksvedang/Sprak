//#define WRITE_DEBUG_INFO

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ProgrammingLanguageNr1
{
	public class Parser
	{
		public Parser (List<Token> tokens, ErrorHandler errorHandler)
		{
            m_tokens = tokens;
            m_errorHandler = errorHandler;
			
			m_nextTokenIndex = 0;
			m_lookahead = new Token[k];
			m_lookaheadIndex = 0;
			
			//Fill lookahead buffer:
			for (int i = 0; i < k; i++) {
				consumeCurrentToken();
			}
			
			m_programAST = new AST(new Token(Token.TokenType.PROGRAM_ROOT, "<PROGRAM_ROOT>"));
			m_isInsideFunctionDefinition = false;
			m_processed = false;
		}
		
		public void process() {
			
			if(m_processed) {
				throw new InvalidOperationException("Has already processed tokens!");
			}
			
			if(m_tokens.Count == 0) {
				throw new InvalidOperationException("No tokens to process!");
			}
			
			program();
			m_processed = true;
			
			if(m_isInsideFunctionDefinition) {
				//m_errorHandler.errorOccured("
			}
		}
		
		private void program() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("program");
#endif
            m_functionList = new AST(new Token(Token.TokenType.STATEMENT_LIST, "<FUNCTION_LIST>"));
			AST statements = statementList(true);
			statements.addChildFirst(new AST(new Token(Token.TokenType.STATEMENT_LIST, "<GLOBAL_VARIABLE_DEFINITIONS_LIST>")));
			
            m_programAST.addChild(statements);
            m_programAST.addChild(m_functionList);
		}

		private AST statementList(bool isInMainScope) {
#if WRITE_DEBUG_INFO
			Console.WriteLine("statement list");
#endif		
			AST statementListTree = new AST(new Token(Token.TokenType.STATEMENT_LIST, "<STATEMENT_LIST>"));
			
			while (lookAheadType(1) != Token.TokenType.EOF &&
			       lookAheadType(1) != Token.TokenType.ELSE)
			{
				if(lookAheadType(1) == Token.TokenType.BLOCK_END) {
					if(isInMainScope) {
						m_errorHandler.errorOccured("Found the word 'end' where it makes no sense", 
						                            Error.ErrorType.SYNTAX, lookAhead(1).LineNr, lookAhead(1).LinePosition); 
					}
					break;
				}
				
				AST statementTree = statement();
				if(statementTree != null) {
					statementListTree.addChild(statementTree);
				}
				else {
#if WRITE_DEBUG_INFO
					Console.WriteLine("null statement");
#endif					
				}
			}
			
			return statementListTree;
		}
		
		private AST statement() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("statement");
#endif
			AST statementTree = null;
			
			try {
				statementTree = figureOutStatementType();
			}
			catch(Error e) {
				m_errorHandler.errorOccured(e);
#if WRITE_DEBUG_INFO
				Console.WriteLine("Error: " + e.getMessage());
#endif
				skipStuffUntilNextLine();
			}
			
			return statementTree;
		}
			
		private AST figureOutStatementType() {
			AST statementTree = null;

			if (lookAheadType(1) == Token.TokenType.NAME &&
			    lookAheadType(2) == Token.TokenType.NAME) {
				throw new Error("Can't understand what the word '" + lookAhead(1).getTokenString() + "' means here", Error.ErrorType.SYNTAX, lookAhead(1).LineNr, lookAhead(1).LinePosition);
			}
			else if ( lookAheadType(1) == Token.TokenType.BUILT_IN_TYPE_NAME &&
				 lookAheadType(2) == Token.TokenType.NAME &&
				 lookAheadType(3) == Token.TokenType.PARANTHESIS_LEFT ) 
			{
                m_functionList.addChild(functionDeclaration());
				checkThatItsTheEndOfTheLine();
			}
            else if (lookAheadType(1) == Token.TokenType.LOOP)
            {
                statementTree = loop();
            }
            else if (lookAheadType(1) == Token.TokenType.BUILT_IN_TYPE_NAME &&
					 lookAheadType(2) == Token.TokenType.NAME) 
			{
				if (lookAheadType(3) == Token.TokenType.ASSIGNMENT) {
					statementTree = declarationAndAssignment();
					checkThatItsTheEndOfTheLine();
				} else {
					statementTree = declaration();
					checkThatItsTheEndOfTheLine();
				}
			}
			else if (lookAhead(1).getTokenString() == "@" &&
			         lookAheadType(2) == Token.TokenType.ASSIGNMENT)
			{
				throw new Error("Can't assign to @", Error.ErrorType.SYNTAX, lookAhead(1).LineNr, lookAhead(1).LinePosition);
			}
			else if (lookAheadType(1) == Token.TokenType.NAME)
			{
				if (lookAheadType(2) == Token.TokenType.ASSIGNMENT) 
				{
					statementTree = assignment();
					checkThatItsTheEndOfTheLine();
				} 
				else if(lookAheadType(2) == Token.TokenType.OPERATOR &&
					   (lookAhead(2).getTokenString() == "++" || lookAhead(2).getTokenString() == "--") ) 
				{
					statementTree = plusplusOrMinusminus();
				} 
				else if(lookAheadType(2) == Token.TokenType.BRACKET_LEFT) 
				{
					statementTree = assignmentToArray();
				}
				else if(lookAhead(2).getTokenString() == "+=" || lookAhead(2).getTokenString() == "*=" || lookAhead(2).getTokenString() == "-=" || lookAhead(2).getTokenString() == "/=")
				{
					statementTree = assignmentAndOperator();
				}
				else 
				{
					statementTree = expression();
					checkThatItsTheEndOfTheLine(); // makes the bodies of if-statements require new line for statements
				}
			}
			else if (lookAheadType(1) == Token.TokenType.NUMBER ||
					 lookAheadType(1) == Token.TokenType.PARANTHESIS_LEFT) 
			{
				statementTree = expression();
				checkThatItsTheEndOfTheLine(); // see above
			}				
			else if (lookAheadType(1) == Token.TokenType.IF) 
            {
				statementTree = ifThenElse();
			}
			else if (lookAheadType(1) == Token.TokenType.RETURN) 
            {
				statementTree = returnFromFunction();
			}
            else if (lookAheadType(1) == Token.TokenType.BREAK)
            {
                statementTree = breakStatement();
            }
			else if (lookAheadType(1) == Token.TokenType.NEW_LINE) 
            {
				match(Token.TokenType.NEW_LINE); // just skip
			}
			else
            {
                throw new Error("Can't understand what the word '" + lookAhead(1).getTokenString() + "' means here",
                    Error.ErrorType.SYNTAX, lookAhead(1).LineNr, lookAhead(1).LinePosition);
			}
			
			return statementTree;
		}
		
		private void checkThatItsTheEndOfTheLine() {
			if (lookAheadType(1) == Token.TokenType.EOF) {
				match(Token.TokenType.EOF);
			}
			else if(lookAheadType(1) == Token.TokenType.NEW_LINE) {
				match(Token.TokenType.NEW_LINE);
			} else {
				throw new Error("Can't understand the words at the end of this line", Error.ErrorType.SYNTAX, lookAhead(1).LineNr, lookAhead(1).LinePosition);
			}
		}
		
		private void skipStuffUntilNextLine() {
			while(	lookAheadType(1) != Token.TokenType.NEW_LINE &&
					lookAheadType(1) != Token.TokenType.EOF ) 
			{
				consumeCurrentToken();
			}
		}
		
#if WRITE_DEBUG_INFO		
		static ASTPainter p = new ASTPainter();
#endif
		
		private AST expression() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("expression");
#endif		
			AST expression = booleanExpression();
#if WRITE_DEBUG_INFO
			//if(expression != null) {
			//	Console.WriteLine("Got expression tree: ");
			//	p.PaintAST(expression);
			//}
#endif				
			return expression;	
		}
		
		private AST booleanExpression() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("boolean expression");
#endif
			
			AST lhs = comparisonExpression();
			
			if ( lookAhead(1).getTokenString() == "&&" ||
				 lookAhead(1).getTokenString() == "||"
				)
			{
				Token operatorToken = match(Token.TokenType.OPERATOR);
				Console.WriteLine ("Matched operator token " + operatorToken.getTokenString() + " at line " + operatorToken.LineNr + " and pos " + operatorToken.LinePosition);

				AST rhs = booleanExpression();
				checkLeftHandSide(lhs, operatorToken);
				checkRightHandSide(rhs, operatorToken);
				
				AST booleanExpressionTree = new AST(operatorToken);
				booleanExpressionTree.addChild(lhs);
				booleanExpressionTree.addChild(rhs);
				return booleanExpressionTree;
				
			} else {
				return lhs;
			}
		}
		
		private AST comparisonExpression() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("comparison expression");
#endif
			
			AST lhs = plusOrMinusExpression();
			
			if ( lookAhead(1).getTokenString() == "<" ||
				 lookAhead(1).getTokenString() == ">" ||
				 lookAhead(1).getTokenString() == "<=" ||
				 lookAhead(1).getTokenString() == ">=" ||
				 lookAhead(1).getTokenString() == "!=" ||
				 lookAhead(1).getTokenString() == "=="
				)
			{
				Token operatorToken = match(Token.TokenType.OPERATOR);
				AST rhs = plusOrMinusExpression();
				checkLeftHandSide(lhs, operatorToken);
				checkRightHandSide(rhs, operatorToken);

                AST comparisonExpressionTree = new AST(operatorToken);
				comparisonExpressionTree.addChild(lhs);
				comparisonExpressionTree.addChild(rhs);
				return comparisonExpressionTree;
				
			} else {
				return lhs;
			}
		}
		
		private AST plusOrMinusExpression() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("plus or minus expression");
#endif
		
			AST lhs = multiplicationExpression();
			
			if ( lookAhead(1).getTokenString() == "+" ||
				 lookAhead(1).getTokenString() == "-" ) 
			{
				Token operatorToken = match(Token.TokenType.OPERATOR);
				AST rhs = plusOrMinusExpression();
				checkLeftHandSide(lhs, operatorToken);
				checkRightHandSide(rhs, operatorToken);				

                AST plusMinusExpressionTree = new AST(operatorToken);
				plusMinusExpressionTree.addChild(lhs);
				plusMinusExpressionTree.addChild(rhs);
				return plusMinusExpressionTree;
			} else {
				return lhs;
			}
		}
		
		private AST multiplicationExpression() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("multiplication expression");
#endif
			
			AST lhs = dotNotationExpression();
			
			if ( lookAhead(1).getTokenString() == "*" ||
				 lookAhead(1).getTokenString() == "/" ) 
			{
				Token operatorToken = match(Token.TokenType.OPERATOR);			
				AST rhs = multiplicationExpression();
				checkLeftHandSide(lhs, operatorToken);
				checkRightHandSide(rhs, operatorToken);				

                AST multiplicationTree = new AST(operatorToken);
				multiplicationTree.addChild(lhs);
				multiplicationTree.addChild(rhs);
				return multiplicationTree;
				
			} else {
				return lhs;
			}
		}

		private AST dotNotationExpression() {
			#if WRITE_DEBUG_INFO
			Console.WriteLine("dot notation expression");
			#endif

			AST lhs = parenthesisExpression();

			if ( lookAhead(1).getTokenType() == Token.TokenType.DOT)
			{
				match (Token.TokenType.DOT);

				#if WRITE_DEBUG_INFO
				Console.WriteLine("It's a dot notation expression!!! for example: object.f(a, b)");
				#endif

				Token nameToken = match(Token.TokenType.NAME);

				AST functionCallTree =
					new AST_FunctionCall(new Token(Token.TokenType.FUNCTION_CALL, "RemoteFunctionCall", nameToken.LineNr, nameToken.LinePosition));

				// These are the argument sent through the remote function call
				AST innerArgumentList = FunctionArgumentList ();

				// The call to RemoteFunctionCall always takes three args: <id>, <functionName> & <args>
				AST argumentList = new AST (new Token (Token.TokenType.NODE_GROUP, "<ARGUMENT_LIST>"));

				// <ID>
				argumentList.addChild (lhs); // use whatever is on the left side of the dot in the function (method rather) call (.)
				Console.WriteLine ("Method call ID: ");
				(new ASTPainter()).PaintAST(lhs);

				// <FunctionName>
				Token functionNameToken = new TokenWithValue (
					Token.TokenType.QUOTED_STRING, 
					nameToken.getTokenString (), 
					nameToken.LineNr, 
					nameToken.LinePosition,
					new ReturnValue(nameToken.getTokenString()));

				argumentList.addChild (new AST(functionNameToken));

				// <args>
				AST_ArrayEndSignal argsArray = new AST_ArrayEndSignal(new Token(Token.TokenType.ARRAY_END_SIGNAL, "<ARRAY>"));
				Console.WriteLine ("Inner args:");
				foreach (var child in innerArgumentList.getChildren()) {
					Console.WriteLine (child.getTokenString ());
					argsArray.addChild (child);
				}
				argsArray.ArraySize = innerArgumentList.getChildren().Count; // DAMNIT DON'T FORGET THIS ONE
				argumentList.addChild (argsArray); // send the arguments as an array to RemoteFunctionCall

				functionCallTree.addChild(argumentList);
				return functionCallTree;
			} else {
				return lhs;
			}
		}
		
		private AST parenthesisExpression() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("parenthesis expression");
#endif		
			AST lhs;
				
			if (lookAheadType(1) == Token.TokenType.PARANTHESIS_LEFT) {
#if WRITE_DEBUG_INFO
			Console.WriteLine("paranthesis left");
#endif
				match(Token.TokenType.PARANTHESIS_LEFT);
				lhs = expression();
#if WRITE_DEBUG_INFO
			Console.WriteLine("paranthesis right");
#endif
				match(Token.TokenType.PARANTHESIS_RIGHT);
			} else {
				return operand();
			}
			
			if (lookAheadType(1) == Token.TokenType.OPERATOR) { // two parenthesis expressions with an operator between them
				Token operatorToken = match(Token.TokenType.OPERATOR);
				
				AST rightParenthesisTree = expression();

                AST duoParenthesisTree = new AST(operatorToken);
				duoParenthesisTree.addChild(lhs);
				duoParenthesisTree.addChild(rightParenthesisTree);
				return duoParenthesisTree;
			} else {
				return lhs;
			}
		}
		
		void checkLeftHandSide(AST lhs, Token operatorToken)
		{
			if(lhs == null) {
				throw new Error("No expression on the left side of '" + operatorToken.getTokenString() + "'",
				    Error.ErrorType.SYNTAX, operatorToken.LineNr, operatorToken.LinePosition - 1);
			}
		}
		
		void checkRightHandSide(AST rhs, Token operatorToken)
		{
			if(rhs == null) {
				throw new Error("No expression on the right side of '" + operatorToken.getTokenString() + "'",
				    Error.ErrorType.SYNTAX, operatorToken.LineNr, operatorToken.LinePosition + 2);
			}
		}

		private AST operand() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("operand");
#endif
			
			AST operandTree = null;
		
			if (lookAheadType(1) == Token.TokenType.NAME && 
				lookAheadType(2) == Token.TokenType.PARANTHESIS_LEFT) 
			{
				operandTree = functionCall();
			}
			else if ( 	lookAheadType(1) == Token.TokenType.NAME &&
				 		lookAheadType(2) == Token.TokenType.BRACKET_LEFT ) 
			{
                operandTree = arrayLookup();
			}
			else if ( lookAheadType(1) == Token.TokenType.FROM ) 
			{
                operandTree = fromMinToMaxArrayCreation();
			}
			else if (lookAheadType(1) == Token.TokenType.NAME) 
			{
				Token operandToken = match(Token.TokenType.NAME);
				operandTree = new AST(operandToken);
			}
			else if (lookAheadType(1) == Token.TokenType.NUMBER) 
			{
				Token operandToken = match(Token.TokenType.NUMBER);
				
				
				float number = (float)Convert.ToDouble(operandToken.getTokenString(),
				                                       CultureInfo.InvariantCulture
				                                       );
				
				operandTree = new AST(new TokenWithValue(operandToken.getTokenType(), 
				                                         operandToken.getTokenString(),
				                                         new ReturnValue(number)));
			}
			else if (lookAheadType(1) == Token.TokenType.QUOTED_STRING) 
			{
				Token operandToken = match(Token.TokenType.QUOTED_STRING);
				operandTree = new AST(new TokenWithValue(operandToken.getTokenType(), 
				                                         operandToken.getTokenString(),
				                                         new ReturnValue(operandToken.getTokenString())));
			}
			else if (lookAheadType(1) == Token.TokenType.BOOLEAN_VALUE) 
			{
				Token operandToken = match(Token.TokenType.BOOLEAN_VALUE);
				bool boolean = operandToken.getTokenString() == "true" ? true : false;
				operandTree = new AST(new TokenWithValue(operandToken.getTokenType(), 
				                                         operandToken.getTokenString(),
				                                         new ReturnValue(boolean)));
			}
			else if (lookAheadType(1) == Token.TokenType.OPERATOR &&
					 lookAhead(1).getTokenString() == "-")
			{
				operandTree = negativeExpression();
			}
			else if (lookAheadType(1) == Token.TokenType.BRACKET_LEFT) 
			{
				operandTree = arrayCreation();
			}

			return operandTree;
		}
				
		private AST arrayCreation() 
		{
#if WRITE_DEBUG_INFO
			Console.WriteLine("array");
#endif
			match(Token.TokenType.BRACKET_LEFT);
			
			AST_ArrayEndSignal arrayTree = 
				new AST_ArrayEndSignal(new Token(Token.TokenType.ARRAY_END_SIGNAL, "<ARRAY>"));			                                  
			
			int length = 0;
			
			if(lookAheadType(1) != Token.TokenType.BRACKET_RIGHT) {
				while(true) {
					arrayTree.addChild(expression());				
					length++;
					
					if(lookAheadType(1) == Token.TokenType.BRACKET_RIGHT) {
						break;
					}
					else if(lookAheadType(1) == Token.TokenType.COMMA) {
						match(Token.TokenType.COMMA);
					}
					else {
						// whitespace is enough! i.e. [1 2 3]
					}
				}
				arrayTree.ArraySize = length;
			}
			else {
				// "array a = []" scenario
				arrayTree.ArraySize = 0;
			}
			
			match(Token.TokenType.BRACKET_RIGHT);
			
			return arrayTree;
		}
		
		private AST arrayLookup() {
			AST arrayName = new AST(match(Token.TokenType.NAME));
			AST arrayLookupNode = new AST(new Token(Token.TokenType.ARRAY_LOOKUP, arrayName.getTokenString()));		
			match(Token.TokenType.BRACKET_LEFT);
			AST arrayIndex = expression();
			match(Token.TokenType.BRACKET_RIGHT);
			arrayLookupNode.addChild(arrayIndex);
			return arrayLookupNode;
		}
		
		private AST plusplusOrMinusminus() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("plusplusOrMinusminus");
#endif
			Token nameToken = match(Token.TokenType.NAME);
			Token operatorToken = match(Token.TokenType.OPERATOR);
			
			AST operationTree = null;
			
			if(operatorToken.getTokenString() == "++") {
				operationTree = new AST(new Token(Token.TokenType.OPERATOR, "+"));
			}
			else if(operatorToken.getTokenString() == "--") {
				operationTree = new AST(new Token(Token.TokenType.OPERATOR, "-"));
			}
			else {
				throw new Exception("Error!");	
			}
			
			operationTree.addChild(new AST(nameToken));
			operationTree.addChild(new AST(new TokenWithValue(Token.TokenType.NUMBER, "1", new ReturnValue(1.0f))));
			
			AST assignmentTree = new AST_Assignment(new Token(Token.TokenType.ASSIGNMENT, "="), nameToken.getTokenString());
			assignmentTree.addChild(operationTree);
			
			return assignmentTree;
		}
		
		private AST negativeExpression() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("negative expression");
#endif		
			match(Token.TokenType.OPERATOR); // the minus sign
			
			AST negativeExpressionTree = new AST(new Token(Token.TokenType.OPERATOR, "*"));

			AST minusSign = new AST(new TokenWithValue(Token.TokenType.NUMBER, "-1", lookAhead(1).LineNr, lookAhead(1).LinePosition, new ReturnValue(-1.0f))); 
			AST expressionTree = parenthesisExpression(); 
			//operand();
			negativeExpressionTree.addChild(minusSign);
			negativeExpressionTree.addChild(expressionTree);
			
			return negativeExpressionTree;
		}
		
		private AST quotedString() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("quoted string");
#endif
			
			Token token = match(Token.TokenType.QUOTED_STRING);
			return new AST(token);
		}
		
		private AST functionCall() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("function call");
#endif

            Token nameToken = match(Token.TokenType.NAME);

            AST functionCallTree =
                new AST_FunctionCall(new Token(Token.TokenType.FUNCTION_CALL, nameToken.getTokenString(), nameToken.LineNr, nameToken.LinePosition));
			
			var argumentList = FunctionArgumentList ();

			functionCallTree.getToken ().LineNr = nameToken.LineNr;
			functionCallTree.getToken ().LinePosition = nameToken.LinePosition;
            functionCallTree.addChild(argumentList);

			return functionCallTree;
		}

		AST FunctionArgumentList ()
		{
			match (Token.TokenType.PARANTHESIS_LEFT);

			AST argumentList = new AST (new Token (Token.TokenType.NODE_GROUP, "<ARGUMENT_LIST>"));
			if (lookAheadType (1) != Token.TokenType.PARANTHESIS_RIGHT) {
				while (true) {
					AST expressionTree = expression ();
					if (expressionTree != null) {
						argumentList.addChild (expressionTree);
						// add arguments as subtrees
					}
					else {
						throw new Error ("Something is wrong with the argument list", Error.ErrorType.SYNTAX, lookAhead (1).LineNr, lookAhead (1).LinePosition);
					}
					if (lookAheadType (1) == Token.TokenType.COMMA) {
						match (Token.TokenType.COMMA);
						continue;
					}
					else {
						// Is something wrong?
						if (lookAheadType (1) == Token.TokenType.NEW_LINE || lookAheadType (1) == Token.TokenType.EOF) {
							throw new Error ("Ending parenthesis is missing in function call", Error.ErrorType.SYNTAX, lookAhead (1).LineNr, lookAhead (1).LinePosition);
						}
						else
							if (lookAheadType (1) == Token.TokenType.NAME || lookAheadType (1) == Token.TokenType.QUOTED_STRING || lookAheadType (1) == Token.TokenType.NUMBER) {
								//throw new Error("A comma is missing in argument list", Error.ErrorType.SYNTAX, lookAhead(1).LineNr, lookAhead(1).LinePosition);
								// allow missing commas in function call 
								continue;
							}
						break;
					}
				}
			}
			match (Token.TokenType.PARANTHESIS_RIGHT);
			return argumentList;
		}
		
		private AST ifThenElse() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("if block");
#endif
			AST ifThenElseTree;
            AST trueChild;
            AST falseChild = null;
            AST expr;

			try {
                match(Token.TokenType.IF);
                expr = expression(); // child 0
				match(Token.TokenType.NEW_LINE);
				trueChild = statementList(false); // child 1

				if ((lookAheadType(1) == Token.TokenType.ELSE) && (lookAheadType(2) == Token.TokenType.IF)) {
#if WRITE_DEBUG_INFO
Console.WriteLine("if else block");
#endif
					match(Token.TokenType.ELSE);
					var ifElseBranch = statement(); // ifThenElse();
					// have to put it into a statement list to make scopes work
					falseChild = new AST(new Token(Token.TokenType.STATEMENT_LIST, "<STATEMENT_LIST>"));
					if(ifElseBranch != null) {
						falseChild.addChild(ifElseBranch);
					}
#if WRITE_DEBUG_INFO
Console.WriteLine("Popping out from ifElse branch");
#endif
				}
				else if (lookAheadType(1) == Token.TokenType.ELSE) {
#if WRITE_DEBUG_INFO
					Console.WriteLine("else block");
#endif
					match(Token.TokenType.ELSE);
					match(Token.TokenType.NEW_LINE);
					falseChild = statementList(false); // child 2
					match(Token.TokenType.BLOCK_END);
				}
				else {
#if WRITE_DEBUG_INFO
					Console.WriteLine("no else block");
#endif
					match(Token.TokenType.BLOCK_END);
				}
			}
			catch(Error e) {
				// The error caught here will probably be from the match() function.
				// Since that means we're missing some part of the if-statement we can give a better 
				// error message by throwing a new one.
				throw new Error("Something is wrong with the IF-statement", Error.ErrorType.SYNTAX, e.getLineNr(), e.getLinePosition());
			}
			
            ifThenElseTree = new AST_IfNode(new Token(Token.TokenType.IF, "if", lookAhead(1).LineNr, lookAhead(1).LinePosition));
            ifThenElseTree.addChild(expr);

            ifThenElseTree.addChild(trueChild);
            if (falseChild != null)
            {
                ifThenElseTree.addChild(falseChild);
            }

			return ifThenElseTree;
		}
		
		private void allowLineBreak() {
			if (lookAheadType(1) == Token.TokenType.NEW_LINE) {
				match(Token.TokenType.NEW_LINE); // allow line break
			}
		}
		
		private AST returnFromFunction() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("return from function");
#endif			
			
			AST returnTree = new AST(match(Token.TokenType.RETURN));
			AST returnExpression = expression();
			
			if(returnExpression != null) {
				returnTree.addChild(returnExpression);
			}
            //else {
            //    throw new Error("No expression in return statement", Error.ErrorType.SYNTAX,
            //        lookAhead(1).LineNr, lookAhead(1).LinePosition);
            //}
			
			return returnTree;						
		}

        private AST_VariableDeclaration declaration()
        {
#if WRITE_DEBUG_INFO
			Console.WriteLine("declaration");
#endif		
			Token typeName = match(Token.TokenType.BUILT_IN_TYPE_NAME);
			Token variableName = match(Token.TokenType.NAME);

            AST_VariableDeclaration declarationTree = new AST_VariableDeclaration(
                new Token(Token.TokenType.VAR_DECLARATION, "<VAR_DECL>", lookAhead(1).LineNr, lookAhead(1).LinePosition),
                ReturnValue.getReturnValueTypeFromString(typeName.getTokenString()),
                variableName.getTokenString());
						
			return declarationTree;
		}
		
		private AST assignment() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("assignment");
#endif
			
			Token nameToken = match(Token.TokenType.NAME);
			Token assignmentToken = match(Token.TokenType.ASSIGNMENT);
			AST expressionTree = expression();
			
			if(expressionTree != null) {
				AST_Assignment assignmentTree = new AST_Assignment(assignmentToken, nameToken.getTokenString());
				assignmentTree.addChild(expressionTree);
			
				return assignmentTree;
			}
			else {
				throw new Error("The expression after = makes no sense", Error.ErrorType.SYNTAX,
					assignmentToken.LineNr, assignmentToken.LinePosition);
			}
		}
		
		private AST assignmentAndOperator() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("assignment and operator");
#endif
			
			Token nameToken = match(Token.TokenType.NAME);
			Token operatorToken = match(Token.TokenType.OPERATOR);
			AST expressionTree = expression();
			
			if(expressionTree != null) {
				AST_Assignment assignmentTree = new AST_Assignment(new Token(Token.TokenType.ASSIGNMENT, "="), nameToken.getTokenString());
				AST selfOperation = null;
				
				if(operatorToken.getTokenString() == "+=") {
					selfOperation = new AST(new Token(Token.TokenType.OPERATOR, "+"));
				}
				else if(operatorToken.getTokenString() == "*=") {
					selfOperation = new AST(new Token(Token.TokenType.OPERATOR, "*"));
				}
				else if(operatorToken.getTokenString() == "-=") {
					selfOperation = new AST(new Token(Token.TokenType.OPERATOR, "-"));
				}
				else if(operatorToken.getTokenString() == "/=") {
					selfOperation = new AST(new Token(Token.TokenType.OPERATOR, "/"));
				}
				else {
					throw new Error("Can't handle the operator '" + operatorToken.getTokenString() + "'",
					                Error.ErrorType.SYNTAX, assignmentTree.getToken().LineNr,
					                assignmentTree.getToken().LinePosition);
				}
				selfOperation.addChild(nameToken);
				selfOperation.addChild(expressionTree);
				assignmentTree.addChild(selfOperation);
			
				return assignmentTree;
			}
			else {
				throw new Error("The expression after " + operatorToken.getTokenString() 
				                + " makes no sense", Error.ErrorType.SYNTAX,
					operatorToken.LineNr, operatorToken.LinePosition);
			}
		}
		
		private AST assignmentToArray()
		{
#if WRITE_DEBUG_INFO
			Console.WriteLine("assignment to array");
#endif	
			
			Token nameToken = null;
			AST indexNode = null;
			
			#if WRITE_DEBUG_INFO
				Console.WriteLine("normal array");
			#endif	
			nameToken = match(Token.TokenType.NAME);
			match(Token.TokenType.BRACKET_LEFT);
			indexNode = expression();			
			match(Token.TokenType.BRACKET_RIGHT);
			
			if(lookAheadType(1) == Token.TokenType.NEW_LINE ||
			   lookAheadType(1) == Token.TokenType.EOF) 
			{
				// it's a statement without assignment
				return indexNode;
			}
			
			Token assignmentToken = match(Token.TokenType.ASSIGNMENT);
			AST expressionTree = expression();
			
			if(expressionTree != null) {
				Token arrayAssignmentToken = new Token(Token.TokenType.ASSIGNMENT_TO_ARRAY, "=", assignmentToken.LineNr, assignmentToken.LinePosition);
				AST_Assignment assignmentTree = new AST_Assignment(arrayAssignmentToken, nameToken.getTokenString());
				assignmentTree.addChild(indexNode);
				assignmentTree.addChild(expressionTree);
				
				/*ASTPainter p = new ASTPainter();
				Console.WriteLine("---AST---");
				p.PaintAST(assignmentTree);
				Console.WriteLine("---------");*/
				return assignmentTree;
			}
			else {
				throw new Error("The expression after = makes no sense", Error.ErrorType.SYNTAX,
					assignmentToken.LineNr, assignmentToken.LinePosition);
			}
		}
		
		private AST declarationAndAssignment() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("declaration and assignment");
#endif
			
			AST_VariableDeclaration declarationTree = declaration();
			Token assignmentToken = match(Token.TokenType.ASSIGNMENT);
			AST expressionTree = expression();
			
			if(expressionTree != null) {
                AST_Assignment assignmentTree = new AST_Assignment(assignmentToken, declarationTree.Name);
				
                assignmentTree.addChild(expressionTree);
				
				AST declarationAndAssignmentTree =
                		new AST(new Token(Token.TokenType.STATEMENT_LIST, "<DECLARATION_AND_ASSIGNMENT>", declarationTree.getToken().LineNr, declarationTree.getToken().LinePosition));
				declarationAndAssignmentTree.addChild(declarationTree);
				declarationAndAssignmentTree.addChild(assignmentTree);
			
				return declarationAndAssignmentTree;
			} else {
				throw new Error("The expression after = makes no sense", Error.ErrorType.SYNTAX,
					lookAhead(1).LineNr, lookAhead(1).LinePosition);
			}
		}
		
		private AST functionDeclaration() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("function declaration");
#endif
			
			if (m_isInsideFunctionDefinition) {
				throw new Error("Trying to define a function inside a function (are you missing the END word?)", Error.ErrorType.SYNTAX,
					lookAhead(1).LineNr, lookAhead(1).LinePosition);
			} else {
				m_isInsideFunctionDefinition = true;
			}			
			
			AST_FunctionDefinitionNode funcDeclarationTree = 
				new AST_FunctionDefinitionNode(new Token(Token.TokenType.FUNC_DECLARATION, "<FUNC_DECL>", lookAhead(1).LineNr, lookAhead(1).LinePosition));
			
			funcDeclarationTree.addChild(match(Token.TokenType.BUILT_IN_TYPE_NAME)); // child 0 (function return type)
			funcDeclarationTree.addChild(match(Token.TokenType.NAME)); // child 1 (function name)
			
			match(Token.TokenType.PARANTHESIS_LEFT);
			funcDeclarationTree.addChild(parameterList()); // child 2 (parameter list)
			match(Token.TokenType.PARANTHESIS_RIGHT);
			allowLineBreak();
			funcDeclarationTree.addChild(statementList(false)); // child 3
			match(Token.TokenType.BLOCK_END);
			
			m_isInsideFunctionDefinition = false;
			
			return funcDeclarationTree;
		}
		
		private AST parameterList() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("parameter list");
#endif
			
			AST parameterListTree = new AST(new Token(Token.TokenType.NODE_GROUP, "<PARAMETER_LIST>", lookAhead(1).LineNr, lookAhead(1).LinePosition));
			
			if (lookAheadType(1) != Token.TokenType.PARANTHESIS_RIGHT) {

				while(true) {
					
					AST parameterTree = parameter();
					parameterListTree.addChild(parameterTree);
					
					if (lookAheadType(1) == Token.TokenType.COMMA) {
						match(Token.TokenType.COMMA);
						continue;
					} else {
						break;
					}
				}
			}
			
			return parameterListTree;
		}

        private AST parameter()
        {
#if WRITE_DEBUG_INFO
			Console.WriteLine("parameter");
#endif
			
			AST parameterTree = new AST(new Token(Token.TokenType.PARAMETER, "<PARAMETER>", lookAhead(1).LineNr, lookAhead(1).LinePosition));

            AST type = new AST(match(Token.TokenType.BUILT_IN_TYPE_NAME));
			AST name = new AST(match(Token.TokenType.NAME));

            AST declaration = new AST_VariableDeclaration(new Token(Token.TokenType.VAR_DECLARATION, "<PARAMETER_DECLARATION>"),
                ReturnValue.getReturnValueTypeFromString(type.getTokenString()), name.getTokenString());


            AST assigment = new AST_Assignment(new Token(Token.TokenType.ASSIGNMENT, "="), name.getTokenString());

            parameterTree.addChild(declaration);
            parameterTree.addChild(assigment);
			
			return parameterTree;
		}
		
		private AST fromMinToMaxArrayCreation() {
#if WRITE_DEBUG_INFO
			Console.WriteLine("fromMinToMaxArrayCreation");
#endif
			
			try {	
				Token fromToken = match(Token.TokenType.FROM);
				
				AST minValue = expression();
				
				if(minValue == null) { throw new Error("Missing expression after 'from'", 
					                         	Error.ErrorType.SYNTAX,
					                            	fromToken.LineNr, fromToken.LinePosition + 5 ); }
				
				match(Token.TokenType.TO);
				AST maxValue = expression();
				
				if(maxValue == null) { throw new Error("Missing expression after 'to'", 
					                         	Error.ErrorType.SYNTAX,
					                            	fromToken.LineNr, fromToken.LinePosition + 3 ); }
				
				AST_FunctionCall callRangeFunction = new AST_FunctionCall(new Token(Token.TokenType.FUNCTION_CALL, "range"));
				AST argumentList = new AST(new Token(Token.TokenType.NODE_GROUP, "<ARGUMENT_LIST>"));
				argumentList.addChild(minValue);
				argumentList.addChild(maxValue);
				callRangeFunction.addChild(argumentList);
				
				return callRangeFunction;
			}
			catch(Error e) {
				m_errorHandler.errorOccured(e);
				return null;
			}
		}

        private AST loop()
        {
#if WRITE_DEBUG_INFO
			Console.WriteLine("loop");
#endif
			AST loopBlockStatements = new AST(new Token(Token.TokenType.STATEMENT_LIST, "<LOOP_BLOCK_STATEMENTS>"));

			AST_LoopNode loopTree = new AST_LoopNode(match(Token.TokenType.LOOP));

			string loopVariableName = "@"; // default name if no name is specified directly after 'loop' keyword
			
			bool isForeachLoop = false;
			if(lookAheadType(1) != Token.TokenType.NEW_LINE)
			{
#if WRITE_DEBUG_INFO
			Console.WriteLine("foreach loop!");
#endif	
				isForeachLoop = true;
				
				// __index__
				AST_VariableDeclaration loopIndexDeclaration 
				= new AST_VariableDeclaration(new Token(Token.TokenType.VAR_DECLARATION, "<VAR_DECL>"),
			                                            ReturnValueType.NUMBER,
			                                            "__index__");
				loopBlockStatements.addChild(loopIndexDeclaration);
				
				AST_Assignment loopIndexAssignment
					= new AST_Assignment(new Token(Token.TokenType.ASSIGNMENT, "="), "__index__");
				loopIndexAssignment.addChild(new AST(new TokenWithValue(Token.TokenType.NUMBER, "-1", new ReturnValue(-1.0f))));
				
				loopBlockStatements.addChild(loopIndexAssignment);

				Token savePoint = lookAhead(1); // will backtrack from here if matching loopVariable + loopRangeExpression fails
				Console.WriteLine("Created save point at token " + savePoint);
				
				Token loopVariable = null;
				AST loopRangeExpression = null; // what the loop will loop through

				if (lookAheadType (1) == Token.TokenType.NAME && lookAheadType(2) == Token.TokenType.IN) {
					loopVariable = match (Token.TokenType.NAME);
					match (Token.TokenType.IN);
					try {
						Console.WriteLine ("Found a potential loop variable " + loopVariable.getTokenString() + ", trying to match loop range expression with a loop variable");
						loopRangeExpression = expression();
						if(loopRangeExpression == null) {
							Console.WriteLine ("null! Failed to match statement after loop variable, will backtrack and assume this loop does not use a loop variable");
							backtrackToToken (savePoint);
						} else {
							loopVariableName = loopVariable.getTokenString();
							Console.WriteLine("Success, loop variable is called: " + loopVariableName);
						}
					}
					catch(Error e) {
						Console.WriteLine ("Failed to match statement after loop variable, will backtrack and assume this loop does not use a loop variable");
						backtrackToToken (savePoint);
					}
				}

				if (loopRangeExpression == null) {
					Console.WriteLine ("There is no loop variable, trying to match a bare loop range expression");
					loopRangeExpression = expression();
				}

				if (loopRangeExpression == null) {
					throw new Error ("Failed to match the expression after 'loop'", Error.ErrorType.SYNTAX, loopTree.getToken ().LineNr, loopTree.getToken ().LinePosition);
				}

				Console.WriteLine ("Loop range/array expression: ");
				(new ASTPainter ()).PaintAST (loopRangeExpression);
				
				// __array__ (is a copy of the array to loop over)
				AST_VariableDeclaration loopArrayDeclaration = new AST_VariableDeclaration(
					new Token(Token.TokenType.VAR_DECLARATION, "<VAR_DECL>"), 
					ReturnValueType.UNKNOWN_TYPE,
				    "__array__");

				loopBlockStatements.addChild(loopArrayDeclaration);
				
				AST_Assignment loopArrayAssignment = 
					new AST_Assignment(new Token(Token.TokenType.ASSIGNMENT, "="), "__array__");
				
				if(loopRangeExpression != null) {
					loopArrayAssignment.addChild(loopRangeExpression);
				}
				else {
					throw new Error("Can't understand array expression in loop", Error.ErrorType.SYNTAX, 
					                loopArrayAssignment.getToken().LineNr, 
					                loopArrayAssignment.getToken().LinePosition);
				}
				
				loopBlockStatements.addChild(loopArrayAssignment);
				
				// __indexes__ (holds all the indexes in the array, since it works like a SortedDictionary)
				// __indexes = getIndexes(__array__)
				AST_VariableDeclaration indexesDeclaration = new AST_VariableDeclaration(
					new Token(Token.TokenType.VAR_DECLARATION, "<VAR_DECL>"),
					ReturnValueType.UNKNOWN_TYPE,
                    "__indexes__");

				loopBlockStatements.addChild(indexesDeclaration);
				
				AST_FunctionCall getArrayIndexes = new AST_FunctionCall(new Token(Token.TokenType.FUNCTION_CALL, "getIndexes"));
				AST argumentList = new AST(new Token(Token.TokenType.NODE_GROUP, "<ARGUMENT_LIST>"));
				argumentList.addChild(new Token(Token.TokenType.NAME, "__array__"));
				getArrayIndexes.addChild(argumentList);
				                                                                  
				AST_Assignment indexesAssignment = new AST_Assignment(new Token(Token.TokenType.ASSIGNMENT, "="), "__indexes__");
				indexesAssignment.addChild(getArrayIndexes);
				loopBlockStatements.addChild(indexesAssignment);
			}
			else 
			{
#if WRITE_DEBUG_INFO
			Console.WriteLine("infinite loop!");
#endif		
			}
			
			/*
			 * 		loopParentTree
			 * 			__index__-declaration
			 * 			__array__-declaration & assigment
			 * 			loop tree
			 * 				loop body
			 * 					foreach stuff
			 * 					rest of statements
			 * 					goto beginning of loop
			 * 
			 * */
			
			allowLineBreak();
			AST loopBody = statementList(false);
			loopBody.addChild(new AST(new Token(Token.TokenType.GOTO_BEGINNING_OF_LOOP, "<GOTO_BEGINNING_OF_LOOP>")));
			allowLineBreak();
			match(Token.TokenType.BLOCK_END);
			
			if(isForeachLoop) {
				loopBody.addChildFirst(foreachStuff(loopVariableName));
			}			
			
			loopTree.addChild(loopBody);        
			
			loopBlockStatements.addChild(loopTree);
			
			AST_LoopBlockNode loopBlock = new AST_LoopBlockNode(new Token(Token.TokenType.LOOP_BLOCK, "<LOOP_BLOCK>"));
			loopBlock.addChild(loopBlockStatements);
			
            return loopBlock;
        }
				                       
		private AST foreachStuff(string pLoopVariableName) {
			AST statementList = new AST(new Token(Token.TokenType.STATEMENT_LIST, "<FOREACH_STATEMENTS>"));
			
			// increase __index__
			AST incrementNode = new AST(new Token(Token.TokenType.OPERATOR, "+"));
			incrementNode.addChild(new AST(new Token(Token.TokenType.NAME, "__index__")));
			incrementNode.addChild(new AST(new TokenWithValue(Token.TokenType.NUMBER, "1", new ReturnValue(1.0f))));
			AST_Assignment assignmentNode = new AST_Assignment(new Token(Token.TokenType.ASSIGNMENT, "="), "__index__");
			assignmentNode.addChild(incrementNode);			
			statementList.addChild(assignmentNode);   
			
			// if(__index__ >= count(__indexes__)) { break }
			AST_FunctionCall lengthOfArray = new AST_FunctionCall(new Token(Token.TokenType.FUNCTION_CALL, "count"));
			AST argumentList = new AST(new Token(Token.TokenType.NODE_GROUP, "<ARGUMENT_LIST>"));
			argumentList.addChild(new Token(Token.TokenType.NAME, "__indexes__"));
			lengthOfArray.addChild(argumentList);
			
			AST breakStatement = new AST_IfNode(new Token(Token.TokenType.IF, "IF"));
			AST operatorTree = new AST(new Token(Token.TokenType.OPERATOR, ">="));
			operatorTree.addChild(new Token(Token.TokenType.NAME, "__index__"));
			operatorTree.addChild(lengthOfArray);
			
			breakStatement.addChild(operatorTree);
			breakStatement.addChild(new Token(Token.TokenType.BREAK, "break"));
			statementList.addChild(breakStatement);
			
			// Loop variable
			AST_VariableDeclaration declarationTree = 
				new AST_VariableDeclaration(new Token(Token.TokenType.VAR_DECLARATION, "<VAR_DECL>"), ReturnValueType.UNKNOWN_TYPE, pLoopVariableName);
			statementList.addChild(declarationTree);
			
			AST arrayIndexLookup = new AST(new Token(Token.TokenType.ARRAY_LOOKUP, "__indexes__"));
			arrayIndexLookup.addChild(new AST(new Token(Token.TokenType.NAME, "__index__")));
			
			AST arrayValueLookup = new AST(new Token(Token.TokenType.ARRAY_LOOKUP, "__array__"));
			arrayValueLookup.addChild(arrayIndexLookup);
			
			AST_Assignment assignmentTree = 
				new AST_Assignment(new Token(Token.TokenType.ASSIGNMENT, "="), pLoopVariableName);
			assignmentTree.addChild(arrayValueLookup);
			statementList.addChild(assignmentTree);		
			
			return statementList;
		}
		
        private AST breakStatement()
        {
            return new AST(match(Token.TokenType.BREAK));
        }

        public virtual Token match(Token.TokenType expectedTokenType)
        {
			Token matchedToken = lookAhead(1);
			
			if(lookAheadType(1) == expectedTokenType) {
#if WRITE_DEBUG_INFO
			Console.WriteLine("MATCHED TOKEN " + lookAhead(1).getTokenString() + " (line " + lookAhead(1).LineNr + ")");
#endif
				consumeCurrentToken();
				
			} else {
#if WRITE_DEBUG_INFO
			Console.WriteLine("FAILED TO MATCH TOKEN OF TYPE " + expectedTokenType.ToString() + 
					" ...FOUND " + lookAhead(1).getTokenString() + " (line " + lookAhead(1).LineNr + ")");
#endif				
				throw new Error(
					"The code word '" + lookAhead(1).getTokenString() + "'" +
					" does not compute. Expected " + expectedTokenType,
					Error.ErrorType.SYNTAX,
					lookAhead(1).LineNr,
					lookAhead(1).LinePosition);
			}
			
			return matchedToken;
		}
		
		public void consumeCurrentToken() {
			
			Token nextToken;
			
			if (m_nextTokenIndex < m_tokens.Count) {
				nextToken = m_tokens[m_nextTokenIndex];
				m_nextTokenIndex++;
			}
			else {
				nextToken = new Token(Token.TokenType.EOF, "<EOF>");
			}
			
			m_lookahead[m_lookaheadIndex] = nextToken;
			m_lookaheadIndex = (m_lookaheadIndex + 1) % k;
		}

		public void backtrackToToken(Token pToken) {
			m_nextTokenIndex = 0;
			m_lookaheadIndex = 0;

			for (int i = 0; i < k; i++) {
				consumeCurrentToken();
			}

			while (lookAhead(1) != pToken) {
				consumeCurrentToken ();
			}

			Console.WriteLine ("Found token");
		}
		
		public Token lookAhead(int i) {
			return m_lookahead[(m_lookaheadIndex + i - 1) % k];
		}
		
		public Token.TokenType lookAheadType(int i) {
			return lookAhead(i).getTokenType();
		}
		
		public AST getAST() {
			if(m_programAST == null) {
				throw new Exception("AST is null, this probably means that you haven't called process() on Parser");
			}
			return m_programAST; 
		}
		
		bool m_processed = false;
		List<Token> m_tokens;
		int m_nextTokenIndex;
		
		Token[] m_lookahead;
		int k = 4; // how many lookahead symbols
		int m_lookaheadIndex = 0; // cirkular index
		
		AST m_programAST;
		ErrorHandler m_errorHandler;
		
		bool m_isInsideFunctionDefinition;

        AST m_functionList;
	}
}
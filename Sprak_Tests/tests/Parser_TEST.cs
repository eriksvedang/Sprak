using System;
using NUnit.Framework;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace ProgrammingLanguageNr1.tests
{
	[TestFixture()]
	public class Parser_TEST
	{
		static ErrorHandler s_errorHandler = new ErrorHandler();
		
		[Test()]
		public void ParseCode1 ()
		{
            Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
			List<Token> tokens = tokenizer.process(File.OpenText("code1.txt"));
			Parser parser = new Parser(tokens, s_errorHandler);
			parser.process();
		}
		
		[Test()]
		public void ParseCode2 ()
		{
            Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
			List<Token> tokens = tokenizer.process(File.OpenText("code2.txt"));
			Parser parser = new Parser(tokens, s_errorHandler);
			parser.process();
		}
		
		[Test()]
		public void ParseCode3 ()
		{
            Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
			List<Token> tokens = tokenizer.process(File.OpenText("code3.txt"));
			Parser parser = new Parser(tokens, s_errorHandler);
			parser.process();
		}
		
		[Test()]
		public void Lookahead ()
		{
			List<Token> tokens = new List<Token>();
			tokens.Add(new Token(Token.TokenType.NAME, "a"));
			tokens.Add(new Token(Token.TokenType.NUMBER, "45"));
			tokens.Add(new Token(Token.TokenType.OPERATOR, "+"));
			tokens.Add(new Token(Token.TokenType.NEW_LINE, "<NEW_LINE>"));
			tokens.Add(new Token(Token.TokenType.EOF, "<EOF>"));
			
			Parser parser = new Parser(tokens, s_errorHandler);
			Assert.AreEqual(Token.TokenType.NAME,     parser.lookAhead(1).getTokenType());
			Assert.AreEqual(Token.TokenType.NUMBER,   parser.lookAhead(2).getTokenType());
			Assert.AreEqual(Token.TokenType.OPERATOR, parser.lookAhead(3).getTokenType());
			
			parser.consumeCurrentToken();
			parser.consumeCurrentToken();
			
			Assert.AreEqual(Token.TokenType.OPERATOR, parser.lookAheadType(1));
			Assert.AreEqual(Token.TokenType.NEW_LINE, parser.lookAheadType(2));
			Assert.AreEqual(Token.TokenType.EOF, 	  parser.lookAheadType(3));
			
			parser.consumeCurrentToken();
			parser.consumeCurrentToken();
			
			Assert.AreEqual(Token.TokenType.EOF, 	  parser.lookAhead(1).getTokenType());
		}
		
		[Test()]
		public void ParseSimpleExpression ()
		{
			List<Token> tokens = new List<Token>();
			tokens.Add(new Token(Token.TokenType.NAME, "a"));
			tokens.Add(new Token(Token.TokenType.OPERATOR, "+"));
			tokens.Add(new Token(Token.TokenType.NAME, "b"));
		
			ErrorHandler errorHandler = new ErrorHandler();
			Parser parser = new Parser(tokens, errorHandler);
			parser.process();
			
			Assert.AreEqual(0, errorHandler.getErrors().Count);
		}
		
		[Test()]
		public void ParseAssignment ()
		{
			List<Token> tokens = new List<Token>();
			tokens.Add(new Token(Token.TokenType.NAME, "variable"));
			tokens.Add(new Token(Token.TokenType.ASSIGNMENT, "="));
			tokens.Add(new Token(Token.TokenType.NUMBER, "42"));
			
			ErrorHandler errorHandler = new ErrorHandler();
			Parser parser = new Parser(tokens, errorHandler);
			parser.process();
			
			Assert.AreEqual(0, errorHandler.getErrors().Count);
		}
		
		[Test()]
		public void ParseSingleName ()
		{
			List<Token> tokens = new List<Token>();
			tokens.Add(new Token(Token.TokenType.NAME, "erik"));
			
			ErrorHandler errorHandler = new ErrorHandler();
			Parser parser = new Parser(tokens, errorHandler);
			parser.process();
			
			Assert.AreEqual(0, errorHandler.getErrors().Count);
		}
		
		[Test()]
		public void ParseLongerExpression ()
		{
			List<Token> tokens = new List<Token>();
			
			tokens.Add(new Token(Token.TokenType.NUMBER, "2"));
			tokens.Add(new Token(Token.TokenType.OPERATOR, "*"));
			tokens.Add(new Token(Token.TokenType.NUMBER, "3"));
			tokens.Add(new Token(Token.TokenType.OPERATOR, "+"));
			tokens.Add(new Token(Token.TokenType.NUMBER, "4"));
			tokens.Add(new Token(Token.TokenType.OPERATOR, "*"));
			tokens.Add(new Token(Token.TokenType.NUMBER, "5"));
			
			ErrorHandler errorHandler = new ErrorHandler();
			Parser parser = new Parser(tokens, errorHandler);
			parser.process();
			
			Assert.AreEqual(0, errorHandler.getErrors().Count);
		}
		
		[Test()]
		public void ParseFunctionCallWithParameters ()
		{
			List<Token> tokens = new List<Token>();
			
			tokens.Add(new Token(Token.TokenType.NAME, "foo"));
			tokens.Add(new Token(Token.TokenType.PARANTHESIS_LEFT, "("));
			tokens.Add(new Token(Token.TokenType.NAME, "a"));
			tokens.Add(new Token(Token.TokenType.COMMA, ","));
			tokens.Add(new Token(Token.TokenType.NAME, "bar"));
			tokens.Add(new Token(Token.TokenType.PARANTHESIS_LEFT, "("));
			tokens.Add(new Token(Token.TokenType.PARANTHESIS_RIGHT, ")"));
			tokens.Add(new Token(Token.TokenType.COMMA, ","));
			tokens.Add(new Token(Token.TokenType.NUMBER, "400"));
			tokens.Add(new Token(Token.TokenType.PARANTHESIS_RIGHT, ")"));
			
			ErrorHandler errorHandler = new ErrorHandler();
			Parser parser = new Parser(tokens, errorHandler);
			parser.process();
			
			Assert.AreEqual(0, errorHandler.getErrors().Count);
		}
		
		[Test()]
		public void ParseIfElseBlock ()
		{
			List<Token> tokens = new List<Token>();
			
			tokens.Add(new Token(Token.TokenType.IF, "if"));
			tokens.Add(new Token(Token.TokenType.PARANTHESIS_LEFT, "("));
			tokens.Add(new Token(Token.TokenType.NAME, "m_hasGotCandy"));
			tokens.Add(new Token(Token.TokenType.PARANTHESIS_RIGHT, ")"));
			tokens.Add(new Token(Token.TokenType.NEW_LINE, "\n"));
			tokens.Add(new Token(Token.TokenType.NAME, "boringFunction"));
			tokens.Add(new Token(Token.TokenType.PARANTHESIS_LEFT, "("));
			tokens.Add(new Token(Token.TokenType.PARANTHESIS_RIGHT, ")"));
			tokens.Add(new Token(Token.TokenType.NEW_LINE, "\n"));
			tokens.Add(new Token(Token.TokenType.BLOCK_END, "end"));
			tokens.Add(new Token(Token.TokenType.ELSE, "else"));
			tokens.Add(new Token(Token.TokenType.NEW_LINE, "\n"));
			tokens.Add(new Token(Token.TokenType.BLOCK_END, "end"));
			
			ErrorHandler errorHandler = new ErrorHandler();
			Parser parser = new Parser(tokens, errorHandler);
			parser.process();
			
			Assert.AreEqual(0, errorHandler.getErrors().Count);
		}
		
		[Test()]
		public void ParseFunctionDeclaration ()
		{
			List<Token> tokens = new List<Token>();
			
			tokens.Add(new Token(Token.TokenType.BUILT_IN_TYPE_NAME, "float"));
			tokens.Add(new Token(Token.TokenType.NAME, "foo"));
			tokens.Add(new Token(Token.TokenType.PARANTHESIS_LEFT, "("));
			tokens.Add(new Token(Token.TokenType.PARANTHESIS_RIGHT, ")"));
			tokens.Add(new Token(Token.TokenType.BLOCK_END, "end"));
			
			ErrorHandler errorHandler = new ErrorHandler();
			Parser parser = new Parser(tokens, errorHandler);
			parser.process();
			
			Console.WriteLine("Tree: " + parser.getAST().getTreeAsString());
            errorHandler.printErrorsToConsole();
			
			Assert.AreEqual(0, errorHandler.getErrors().Count);
		}
		
		[Test()]
		public void ParseSomeExpressionsWithFunctions ()
		{
            Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
			List<Token> tokens = tokenizer.process(File.OpenText("code15.txt"));
			
			ErrorHandler errorHandler = new ErrorHandler();
			Parser parser = new Parser(tokens, errorHandler);
			parser.process();
			
			//Console.WriteLine("Tree: " + parser.getAST().getTreeAsString());
			
			Assert.AreEqual(0, errorHandler.getErrors().Count);
		}





        ////////////// AST-CREATION ///////////////

        [Test()]
        public void CreatingSimpleTree()
        {
            Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
            List<Token> tokens = tokenizer.process(File.OpenText("code4.txt"));

            Parser parser = new Parser(tokens, s_errorHandler);
            parser.process();

            //Console.WriteLine("Tree: " + parser.getAST().toStringTree());
			ASTPainter p = new ASTPainter();
			p.PaintAST(parser.getAST());

            AST root = parser.getAST();
            Assert.AreEqual(Token.TokenType.PROGRAM_ROOT, root.getTokenType());

            AST statementList = root.getChild(0);
            Assert.AreEqual(Token.TokenType.STATEMENT_LIST, statementList.getTokenType());

            AST multiplicationTree = statementList.getChild(1);
            Assert.AreEqual(Token.TokenType.OPERATOR, multiplicationTree.getTokenType());

            AST operand1 = multiplicationTree.getChild(0);
            AST operand2 = multiplicationTree.getChild(1);
            Assert.AreEqual("a", operand1.getTokenString());
            Assert.AreEqual("b", operand2.getTokenString());
        }

        [Test()]
        public void OperationOrder()
        {
            Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
            List<Token> tokens = tokenizer.process(File.OpenText("code6.txt"));

            Parser parser = new Parser(tokens, s_errorHandler);
            parser.process();

            //Console.WriteLine("Tree: " + parser.getAST().toStringTree());

            Assert.AreEqual("(<STATEMENT_LIST> <GLOBAL_VARIABLE_DEFINITIONS_LIST> (+ (* a b) (+ (* c d) e)))",
                parser.getAST().getChild(0).getTreeAsString());
        }

        [Test()]
        public void ParenthesisBasics()
        {
            Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
            List<Token> tokens = tokenizer.process(File.OpenText("code7.txt"));

            Parser parser = new Parser(tokens, s_errorHandler);
            parser.process();

            //Console.WriteLine("Tree: " + parser.getAST().toStringTree());

            Assert.AreEqual("(<STATEMENT_LIST> <GLOBAL_VARIABLE_DEFINITIONS_LIST> (* a (/ (+ b c) d)))", parser.getAST().getChild(0).getTreeAsString());
        }

        [Test()]
        public void ComplexExpressions()
        {
            Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
            List<Token> tokens = tokenizer.process(File.OpenText("code8.txt"));

            Parser parser = new Parser(tokens, s_errorHandler);
            parser.process();

            //Console.WriteLine("Tree: " + parser.getAST().getTreeAsString());
        }

		[Test()]
		public void Backtrack ()
		{
			StringReader programString = new StringReader(
				"a b c d e f g h"
			);

			Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
			List<Token> tokens = tokenizer.process(programString);

			tokens.ForEach (t => Console.WriteLine(t.getTokenType().ToString() + ", " + t.getTokenString()));

			Assert.AreEqual(9, tokens.Count);

			Parser parser = new Parser(tokens, s_errorHandler);

			Assert.AreEqual ("a", parser.lookAhead (1).getTokenString ());
			parser.consumeCurrentToken ();
			Assert.AreEqual ("b", parser.lookAhead (1).getTokenString ());
			parser.consumeCurrentToken ();
			parser.consumeCurrentToken ();
			parser.consumeCurrentToken ();
			Assert.AreEqual ("e", parser.lookAhead (1).getTokenString ());
			var savePoint = parser.lookAhead(1);
			Assert.AreEqual ("e", savePoint.getTokenString());
			parser.consumeCurrentToken ();
			parser.consumeCurrentToken ();
			Assert.AreEqual ("g", parser.lookAhead (1).getTokenString ());
			parser.backtrackToToken (savePoint);
			Assert.AreEqual ("e", parser.lookAhead (1).getTokenString ());
		}
	}
}


using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace ProgrammingLanguageNr1
{
	[TestFixture()]
	public class ExpressionEvaluator_TEST
	{
		static ErrorHandler s_errorHandler = new ErrorHandler();
		
		[Test()]
		public void EvaluateSingleNumber ()
		{
			AST tree = new AST(new Token(Token.TokenType.NUMBER, "42"));
			ExpressionEvaluator e = new ExpressionEvaluator(tree);
			Assert.AreEqual(42, e.getValue());
		}
		
		[Test()]
		public void BasicEvaluation ()
		{
            Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
			List<Token> tokens = tokenizer.process(File.OpenText("code9.txt"));			
			
			Parser parser = new Parser(tokens, s_errorHandler);
			parser.process();
			
			AST root = parser.getAST();
			AST expressionTree = root.getChild(0).getChild(0);
			
			ExpressionEvaluator e1 = new ExpressionEvaluator(expressionTree);
			Assert.AreEqual(26, e1.getValue());	
		}
		
		[Test()]
		public void BooleanAND ()
		{
			AST root = new AST(new Token(Token.TokenType.OPERATOR, "&&"));
			AST lhs = new AST(new Token(Token.TokenType.NUMBER, "1"));
			AST rhs = new AST(new Token(Token.TokenType.NUMBER, "0"));
			root.addChild(lhs);
			root.addChild(rhs);			
			ExpressionEvaluator e = new ExpressionEvaluator(root);
			Assert.AreEqual(0, e.getValue());
		}
		
		[Test()]
		public void BooleanOR ()
		{
			AST root = new AST(new Token(Token.TokenType.OPERATOR, "||"));
			AST lhs = new AST(new Token(Token.TokenType.NUMBER, "1"));
			AST rhs = new AST(new Token(Token.TokenType.NUMBER, "0"));
			root.addChild(lhs);
			root.addChild(rhs);			
			ExpressionEvaluator e = new ExpressionEvaluator(root);
			Assert.AreEqual(1, e.getValue());
		}
		
		[Test()]
		public void BooleanEQUALS ()
		{
			AST root = new AST(new Token(Token.TokenType.OPERATOR, "=="));
			AST lhs = new AST(new Token(Token.TokenType.NUMBER, "4500"));
			AST rhs = new AST(new Token(Token.TokenType.NUMBER, "4500"));
			root.addChild(lhs);
			root.addChild(rhs);			
			ExpressionEvaluator e = new ExpressionEvaluator(root);
			Assert.AreEqual(1, e.getValue());
		}
		
		[Test()]
		public void BooleanNOTEQUALS ()
		{
			AST root = new AST(new Token(Token.TokenType.OPERATOR, "!="));
			AST lhs = new AST(new Token(Token.TokenType.NUMBER, "4500"));
			AST rhs = new AST(new Token(Token.TokenType.NUMBER, "4500"));
			root.addChild(lhs);
			root.addChild(rhs);			
			ExpressionEvaluator e = new ExpressionEvaluator(root);
			Assert.AreEqual(0, e.getValue());
		}
		
		[Test()]
		public void BooleanLESSOREQUALS ()
		{
			AST root = new AST(new Token(Token.TokenType.OPERATOR, "<="));
			AST lhs = new AST(new Token(Token.TokenType.NUMBER, "3"));
			AST rhs = new AST(new Token(Token.TokenType.NUMBER, "3"));
			root.addChild(lhs);
			root.addChild(rhs);			
			ExpressionEvaluator e = new ExpressionEvaluator(root);
			Assert.AreEqual(1, e.getValue());
		}
		
		[Test()]
		public void BooleanLESSOREQUALS2 ()
		{
			AST root = new AST(new Token(Token.TokenType.OPERATOR, "<="));
			AST lhs = new AST(new Token(Token.TokenType.NUMBER, "4"));
			AST rhs = new AST(new Token(Token.TokenType.NUMBER, "3"));
			root.addChild(lhs);
			root.addChild(rhs);			
			ExpressionEvaluator e = new ExpressionEvaluator(root);
			Assert.AreEqual(0, e.getValue());
		}
		
		[Test()]
		public void BooleanGREATEROREQUALS ()
		{
			AST root = new AST(new Token(Token.TokenType.OPERATOR, ">="));
			AST lhs = new AST(new Token(Token.TokenType.NUMBER, "3"));
			AST rhs = new AST(new Token(Token.TokenType.NUMBER, "3"));
			root.addChild(lhs);
			root.addChild(rhs);			
			ExpressionEvaluator e = new ExpressionEvaluator(root);
			Assert.AreEqual(1, e.getValue());
		}
		
		[Test()]
		public void BooleanGREATEROREQUALS2 ()
		{
			AST root = new AST(new Token(Token.TokenType.OPERATOR, ">="));
			AST lhs = new AST(new Token(Token.TokenType.NUMBER, "4"));
			AST rhs = new AST(new Token(Token.TokenType.NUMBER, "3"));
			root.addChild(lhs);
			root.addChild(rhs);			
			ExpressionEvaluator e = new ExpressionEvaluator(root);
			Assert.AreEqual(1, e.getValue());
		}
		
		[Test()]
		public void HandleNegativeNumbers ()
		{
            Tokenizer tokenizer = new Tokenizer(s_errorHandler, true);
			List<Token> tokens = tokenizer.process(File.OpenText("code10.txt"));			
			
			Parser parser = new Parser(tokens, s_errorHandler);
			parser.process();
			
			AST root = parser.getAST();
			AST expressionTree = root.getChild(0).getChild(0);
			
			ExpressionEvaluator e = new ExpressionEvaluator(expressionTree);
			Assert.AreEqual(-5, e.getValue());
		}
	}
}


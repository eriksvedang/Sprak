using System;
using NUnit.Framework;

namespace ProgrammingLanguageNr1.tests
{
	[TestFixture()]
	public class AST_TEST
	{
		[Test()]
		public void SimpleAST ()
		{			
			Token plus = new Token(Token.TokenType.OPERATOR, "+");
			Token one = new Token(Token.TokenType.NUMBER, "1");
			Token two = new Token(Token.TokenType.NUMBER, "2");
			
			AST root = new AST(plus);
			root.addChild(new AST(one));
			root.addChild(new AST(two));
			
			AST list = new AST(null);
			list.addChild(new AST(one));
			list.addChild(new AST(two));

            Console.WriteLine(root.getTreeAsString());
			
			Assert.AreEqual("(+ 1 2)", root.getTreeAsString());
		}
		
		[Test()]
		public void ReplaceASTNode ()
		{	
			AST parent = new AST(new Token(Token.TokenType.NAME, "parent"));
			AST child1 = new AST(new Token(Token.TokenType.NAME, "child1"));
			AST child2 = new AST(new Token(Token.TokenType.NAME, "child2"));
			AST child2a = new AST(new Token(Token.TokenType.NAME, "child2a"));
			AST child2b = new AST(new Token(Token.TokenType.NAME, "child2b"));
			
			parent.addChild(child1);
			parent.addChild(child2);
			child2.addChild(child2a);
			child2.addChild(child2b);
			
			//AST child3 = new AST(new Token(Token.TokenType.NAME, "child3"));
			
			Console.WriteLine(parent.findParent(child2b).getTokenString());
			
			ASTPainter p = new ASTPainter();
			p.PaintAST(parent);
			
			//Assert.Fail();
		}
	}
}


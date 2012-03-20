using System;

namespace ProgrammingLanguageNr1
{
	
	// NOTE: The ExpressionEvaluator is just made for testing purposes
	
	public class ExpressionEvaluator
	{
		public ExpressionEvaluator (AST expressionTree)
		{
			m_expressionTree = expressionTree;			
		}
		
		public float getValue() {
			return evaluate(m_expressionTree);
		}
		
		private float evaluate(AST tree) {
			float returnValue = 0;
			if(tree.getTokenType() == Token.TokenType.NUMBER) {
				returnValue = (float)System.Convert.ToDouble(tree.getTokenString());
			}
			else if(tree.getTokenType() == Token.TokenType.OPERATOR) {
				if(tree.getTokenString() == "+") {
					returnValue = evaluate(tree.getChild(0)) + evaluate(tree.getChild(1));
				}
				else if(tree.getTokenString() == "-") {
					returnValue = evaluate(tree.getChild(0)) - evaluate(tree.getChild(1));
				}
				else if(tree.getTokenString() == "*") {
					returnValue = evaluate(tree.getChild(0)) * evaluate(tree.getChild(1));
				}
				else if(tree.getTokenString() == "/") {
					returnValue = evaluate(tree.getChild(0)) / evaluate(tree.getChild(1));
				}
				else if(tree.getTokenString() == "<") {
					returnValue = (evaluate(tree.getChild(0)) < evaluate(tree.getChild(1))) ? 1 : 0;
				}
				else if(tree.getTokenString() == ">") {
					returnValue = (evaluate(tree.getChild(0)) > evaluate(tree.getChild(1))) ? 1 : 0;
				}
				else if(tree.getTokenString() == "<=") {
					returnValue = (evaluate(tree.getChild(0)) <= evaluate(tree.getChild(1))) ? 1 : 0;
				}
				else if(tree.getTokenString() == ">=") {
					returnValue = (evaluate(tree.getChild(0)) >= evaluate(tree.getChild(1))) ? 1 : 0;
				}
				else if(tree.getTokenString() == "&&") {
					returnValue = (evaluate(tree.getChild(0)) != 0 && evaluate(tree.getChild(1)) != 0) ? 1 : 0;
				}
				else if(tree.getTokenString() == "||") {
					returnValue = (evaluate(tree.getChild(0)) != 0 || evaluate(tree.getChild(1)) != 0) ? 1 : 0;
				}
				else if(tree.getTokenString() == "!=") {
					returnValue = (evaluate(tree.getChild(0)) != evaluate(tree.getChild(1))) ? 1 : 0;
				}
				else if(tree.getTokenString() == "==") {
					returnValue = (evaluate(tree.getChild(0)) == evaluate(tree.getChild(1))) ? 1 : 0;
				}
				else {
					throw new InvalidOperationException("ExpressionEvaluator can't handle operators with string " + tree.getTokenString());
				}
			}
			else {
				throw new InvalidOperationException("ExpressionEvaluator can't handle tokens of type " + tree.getTokenType());
			}
			return returnValue;
		}
		
		AST m_expressionTree;
	}
}


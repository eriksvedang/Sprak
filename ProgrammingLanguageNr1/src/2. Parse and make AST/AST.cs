using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ProgrammingLanguageNr1
{
	public class AST
	{
		public AST () {
		}
		
		public AST (Token token)
		{
			m_token = token;
		}
		
		public Token.TokenType getTokenType() { 
			if (m_token != null) {
				return m_token.getTokenType(); 
			} else {
				return Token.TokenType.NO_TOKEN_TYPE;
			}
		}
		
		public bool isNil() { return m_token == null; }
		
		public void addChild(AST childTree) {

			if (childTree == null) {
				//throw new Exception ("Child tree is null");
				if(childTree == null) throw new Error("Failed to understand source code", Error.ErrorType.SYNTAX, -1, -1);
			}
			
			allocateListIfNecessary();
			m_children.Add(childTree);
		}
		
		public void addChildFirst(AST childTree) {
			Debug.Assert(childTree != null);
			
			allocateListIfNecessary();
			m_children.Insert(0, childTree);
		}
		
		public void addChildAtPosition(AST childTree, int pos) {
			Debug.Assert(childTree != null);
			
			allocateListIfNecessary();
			m_children.Insert(pos, childTree);
		}
		
		private void allocateListIfNecessary() {
			if (m_children == null) {
				m_children = new List<AST>();
			}
		}
		
		public void addChild(Token token) {
			AST childTree = new AST(token);
			addChild(childTree);
		}
		
		public AST findParent(AST ofThisChild) 
		{
			Console.WriteLine("Going into " + getTokenString());
			
			if(ofThisChild == null) { return null; }
			if(m_children == null) { return null; }
			
			int i = m_children.IndexOf(ofThisChild);
			if(i >= 0) {
				Console.WriteLine("Found " + ofThisChild.getTokenString());
				return this;
			}
			else {
				foreach(AST child in m_children) {
					AST parent = child.findParent(ofThisChild);
					if(parent != null) {
						return parent;
					}
				}
			}
			return null;
		}
		
		public int getIndexOfChild(AST child) {
			if(m_children != null) {
				return m_children.IndexOf(child);
			}
			else {
				return -1;
			}
		}
		
		public AST getChild(int n) {
			allocateListIfNecessary();
			Debug.Assert(n >= 0);
			Debug.Assert(n < m_children.Count);
			AST child = m_children[n];
			Debug.Assert(child != null);
			return child;
		}
		
		public void removeChild(int index) {
			m_children.RemoveAt(index);
		}

		public List<AST> getChildren() {
			allocateListIfNecessary();
			return m_children;
		}
		
		public string getTokenString() {
			return m_token != null ? m_token.getTokenString() : "nil";
		}
		
		public Token getToken() {
			return m_token;
		}

        public override string ToString()
        {
            return getTokenString();
        }
		
		public string getTreeAsString() {
			if (m_children == null || m_children.Count == 0) {
				return getTokenString();
			}
			StringBuilder stringTree = new StringBuilder();
			if ( !isNil() ) {
				stringTree.Append("(");
				stringTree.Append(getTokenString());
				stringTree.Append(" ");
			}
			int i = 0;
			foreach (AST childTree in m_children) {
				if(i > 0) { stringTree.Append(" "); }
				i++;
				stringTree.Append(childTree.getTreeAsString());
			}
			if ( !isNil() ) {
				stringTree.Append(")");
			}
			return stringTree.ToString();
		}
		
		public int Executions {
			get {
				return this.m_executions;
			}
			set {
				m_executions = value;
			}
		}		
		
		Token m_token;
		List<AST> m_children;
		int m_executions;
	}
}


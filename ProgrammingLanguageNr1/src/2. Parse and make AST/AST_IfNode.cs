using System;

namespace ProgrammingLanguageNr1
{
	public class AST_IfNode : AST
	{
		public AST_IfNode (Token token ) : base(token)
		{
		}
		
		public void setScope(Scope scope) { m_scope = scope; }
		public Scope getScope() { return m_scope; }
        
		Scope m_scope;
	}
}


using System;

namespace ProgrammingLanguageNr1
{
	public class AST_IfNode : AST
	{
		public AST_IfNode (Token token ) : base(token)
		{
		}
		
		public void setScope(Scope scope) { m_scope = scope; }
		public Scope getScope() { 
#if DEBUG
			if (m_scope == null) {
				throw new Exception ("m_scope is null for IfNode at line " + getToken ().LineNr);
			}
#endif
			return m_scope; 
		}
        
		Scope m_scope;
	}
}


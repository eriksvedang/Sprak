using System;

namespace ProgrammingLanguageNr1
{
	public class AST_LoopBlockNode : AST
	{
		public AST_LoopBlockNode (Token token) : base(token)
		{
		}

		public override void ClearMemorySpaces ()
		{
			base.ClearMemorySpaces ();
			if(m_scope != null) {
				m_scope.ClearMemorySpaces();
			}
		}
		
		public void setScope(Scope scope) { m_scope = scope; }
		public Scope getScope() { return m_scope; }
        
		Scope m_scope;
	}
}


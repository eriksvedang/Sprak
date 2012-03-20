using System;

namespace ProgrammingLanguageNr1
{
	public class AST_LoopNode : AST
	{
		public AST_LoopNode (Token token ) : base(token)
		{
		}
		
		public void setScope(Scope scope) { m_scope = scope; }
		public Scope getScope() { return m_scope; }
        
		//public void setForeachArray(ReturnValue foreachArray) { m_foreachArray = foreachArray; }
		//public ReturnValue getForeachArray() { return m_foreachArray; }			
		
		Scope m_scope;
		//ReturnValue m_foreachArray;
	}
}


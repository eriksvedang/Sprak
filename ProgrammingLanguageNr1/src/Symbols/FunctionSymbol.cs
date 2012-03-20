using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ProgrammingLanguageNr1
{
    public class FunctionSymbol : Scope, Symbol
	{
		public FunctionSymbol (Scope enclosingScope, string name, ReturnValueType type, AST functionDefinitionNode) 
			: base(Scope.ScopeType.FUNCTION_SCOPE, name, enclosingScope)
		{
            Debug.Assert(enclosingScope != null);
			Debug.Assert(functionDefinitionNode != null);
			
			m_enclosingScope = enclosingScope;
			m_functionDefinitionNode = functionDefinitionNode;
            m_returnValueType = type;
		}

        public ReturnValueType getReturnValueType()
        {
            return m_returnValueType;
        }
		
		public AST getFunctionDefinitionNode() { return m_functionDefinitionNode; }

		private AST m_functionDefinitionNode;
        private ReturnValueType m_returnValueType;
    }
}


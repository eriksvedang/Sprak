using System;
using System.Collections.Generic;
using System.Text;

namespace ProgrammingLanguageNr1
{
    public class AST_FunctionCall : AST
    {
        public AST_FunctionCall(Token token)
            : base(token)
        {

        }

        AST_FunctionDefinitionNode m_functionDefinitionRef;

        public AST_FunctionDefinitionNode FunctionDefinitionRef
        {
            get { return m_functionDefinitionRef; }
            set { m_functionDefinitionRef = value; }
        }
    }
}

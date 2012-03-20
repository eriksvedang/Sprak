using System;
using System.Collections.Generic;
using System.Text;

namespace ProgrammingLanguageNr1
{
    public class AST_Assignment : AST
    {
        public AST_Assignment(Token token, string variableName)
            : base(token)
        {
            m_variableName = variableName;
        }

        string m_variableName;

        public string VariableName
        {
            get { return m_variableName; }
        }

        public override string ToString()
        {
            return "Assign to " + m_variableName + "";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ProgrammingLanguageNr1
{
    public class AST_VariableDeclaration : AST
    {
        public AST_VariableDeclaration(Token token, ReturnValueType type, string name)
            : base(token)
        {
            m_type = type;
            m_name = name;
        }

        string m_name;
		ReturnValueType m_type;

		public ReturnValueType Type
        {
            get { return m_type; }
            set { m_type = value; }
        }        

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public override string ToString()
        {
                return base.ToString() + " " + m_name + " of type " + m_type;
        }
    }
}

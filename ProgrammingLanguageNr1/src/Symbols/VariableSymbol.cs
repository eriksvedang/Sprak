using System;

namespace ProgrammingLanguageNr1
{
	public class VariableSymbol : Symbol
	{
		public VariableSymbol (string name, ReturnValueType type)
		{
            m_name = name;
            m_returnValueType = type;
		}

        public string getName()
        {
            return m_name;
        }

        public ReturnValueType getReturnValueType()
        {
            return m_returnValueType;
        }

        public override string ToString()
        {
            return getReturnValueType() + " " + getName();
        }

        string m_name;
        ReturnValueType m_returnValueType;
    }
}

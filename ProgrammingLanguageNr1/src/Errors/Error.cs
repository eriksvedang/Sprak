using System;

namespace ProgrammingLanguageNr1
{
	public class Error : Exception
	{
		public enum ErrorType {
			UNDEFINED,
			SYNTAX,
			LOGIC,
            SCOPE,
			RUNTIME
		}

        public Error(string pMessage) : base(pMessage)
        {
            m_type = ErrorType.UNDEFINED;
            m_lineNr = -1;
            m_linePosition = -1;
        }
		
		public Error (string pMessage, ErrorType type, int lineNr, int linePosition) : base(pMessage)
		{
			m_type = type;
			m_lineNr = lineNr;
			m_linePosition = linePosition;
		}
		
		public string getMessage() { return Message; }
		public int getLineNr() { return m_lineNr; }
		public int getLinePosition() { return m_linePosition; }
		public ErrorType getErrorType() { return m_type; }
		
		int m_lineNr, m_linePosition;
		ErrorType m_type;
	}
}


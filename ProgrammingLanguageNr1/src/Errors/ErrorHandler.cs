using System;
using System.Collections.Generic;

namespace ProgrammingLanguageNr1
{
	public class ErrorHandler
	{		
		public ErrorHandler ()
		{
		}
		
		public void errorOccured(Error e) {
			m_errors.Add(e);
		}
			
		public void errorOccured(string message, Error.ErrorType errorType, int lineNr, int linePosition) {
			m_errors.Add(new Error(message, errorType, lineNr, linePosition));
		}
		
		public void errorOccured(string message, Error.ErrorType errorType) {
			m_errors.Add(new Error(message, errorType, 0, 0));
		}
		
		public void printErrorsToConsole() {
			
			if(getErrors().Count == 0) {
				Console.WriteLine("NO ERRORS");
			}
			else {
				Console.WriteLine("ERROR MESSAGES: ");
				foreach (Error e in getErrors()) {
					Console.WriteLine(
						e.getErrorType() + 
						" ERROR: " + 
						e.getMessage()
						+ " at line " + 
						e.getLineNr() + 
						" and position " + 
						e.getLinePosition());
				}
			}
		}

        public void Reset()
        {
            m_errors.Clear();
        }

		public List<Error> getErrors() { return m_errors; }
		
		List<Error> m_errors = new List<Error>();
	}
}


using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ProgrammingLanguageNr1
{
	public class Tokenizer
	{
        public Tokenizer(ErrorHandler errorHandler, bool stripOutComments)
		{
			m_errorHandler = errorHandler;
            m_stripOutComments = stripOutComments;
		}

		public List<Token> process(TextReader textReader) {
			Debug.Assert(textReader != null);

            m_tokens = new List<Token>();
            m_textReader = textReader;
			m_endOfFile = false;
            
            readNextChar();            
            m_currentLine = 1;
			m_currentPosition = 0;
            m_currentTokenStartPosition = 0;

			Token t;
			
			do {
				t = readNextToken();
				t.LineNr = m_currentLine;
				t.LinePosition = m_currentTokenStartPosition;
				m_currentTokenStartPosition = m_currentPosition;
				
				m_tokens.Add(t);

                //Console.WriteLine(t.LineNr + ": " + t.getTokenType().ToString() + " " + t.getTokenString());
				
			} while(t.getTokenType() != Token.TokenType.EOF);
			
			m_textReader.Close();
            m_textReader.Dispose();
			
			return m_tokens;
		}
		
		private Token readNextToken() {
			
			while (!m_endOfFile) {
				
				switch(m_currentChar) 
				{
	                case '\0':
	                    m_endOfFile = true;
	                    continue;
	
					case ' ': case '\t': case ';':
						readNextChar();
						continue;
						
					case '#':
	                    if (m_stripOutComments)
	                    {
	                        stripComment();
	                        continue;
	                    }
	                    else
	                    {
	                        return COMMENT();
	                    }
						
					case '\n':
						return NEW_LINE();
						
					case '+': case '-': case '*': case '/': case '<': 
					case '>': case '=': case '!': case '&': case '|':
						return OPERATOR();
						
					case '(':
						return PARANTHESIS_LEFT();
						
					case ')':
						return PARANTHESIS_RIGHT();
					
					case '[':
						return BRACKET_LEFT();
					
					case ']':
						return BRACKET_RIGHT();
						
					case '\"':
						return QUOTED_STRING();
						
					case ',':
						return COMMA();
						
					default:
						if( isLETTER() ) {
							return NAME();
						}
						else if ( isDIGIT() ) {
							return NUMBER(false);
						}
	                    else
	                    {
	                        m_errorHandler.errorOccured(
	                            "Can't understand this character: \'" +
	                            m_currentChar +
	                            "\'",
	                            Error.ErrorType.SYNTAX,
	                            m_currentLine,
	                            m_currentPosition);
	                        // Try to recover:
	                        readNextChar();
	                        continue;
	                    }
				}
				
			}
			
			return new Token(Token.TokenType.EOF, "<EOF>");
		}

        private void stripComment() 
        {
            while (m_currentChar != '\n' && m_currentChar != '\0')
            {
                readNextChar();
            }
            return;
        }
		
		private Token COMMENT() {
            StringBuilder tokenString = new StringBuilder();
			
			while(m_currentChar != '\n' && m_currentChar != '\0') {
				tokenString.Append(m_currentChar);
				readNextChar();
			}
            return new Token(Token.TokenType.COMMENT, tokenString.ToString());
		}
		
		private Token COMMA() {
			readNextChar();
			return new Token(Token.TokenType.COMMA, ",");
		}

		private Token NEW_LINE() {
			while(m_currentChar == '\n') { // make several new-lines into a single one
				m_currentLine++;
				m_currentPosition = 0;
				readNextChar();
			}
			return new Token(Token.TokenType.NEW_LINE, "<NEW_LINE>");
		}
		
		private Token OPERATOR ()
		{
			StringBuilder tokenString = new StringBuilder ();
			tokenString.Append (m_currentChar);
			
			char firstChar = m_currentChar;
			readNextChar ();
			
			if (firstChar == '-' && isDIGIT()) {
				// Negative number!
				return NUMBER(true);
			}
			else if ( (firstChar == '<' || firstChar == '>') && m_currentChar == '=') {
				tokenString.Append ('=');
				readNextChar();
			}
			else if ( firstChar == '=') {
				if( m_currentChar == '=' ) {
					tokenString.Append ('=');
					readNextChar();
				} else {
					return ASSIGNMENT();
				}				
			}
			else if ( firstChar == '!' && m_currentChar == '=') {
				tokenString.Append ('=');
				readNextChar();
			}
			else if ( firstChar == '&' && m_currentChar == '&') {
				tokenString.Append ('&');
				readNextChar();
			}
			else if ( firstChar == '|' && m_currentChar == '|') {
				tokenString.Append ('|');
				readNextChar();
			}
			else if( firstChar == '+' && m_currentChar == '+') {
				tokenString.Append('+');
				readNextChar();
			}
			else if( firstChar == '-' && m_currentChar == '-') {
				tokenString.Append('-');
				readNextChar();
			}
			else if( firstChar == '+' && m_currentChar == '=') {
				tokenString.Append('=');
				readNextChar();
			}
			
			return new Token(Token.TokenType.OPERATOR, tokenString.ToString());
		}
		
		private Token PARANTHESIS_LEFT() {
			readNextChar();
			return new Token(Token.TokenType.PARANTHESIS_LEFT, "(");
		}
		
		private Token PARANTHESIS_RIGHT() {
			readNextChar();
			return new Token(Token.TokenType.PARANTHESIS_RIGHT, ")");
		}
		
		private Token BRACKET_LEFT() {
			readNextChar();
			return new Token(Token.TokenType.BRACKET_LEFT, "[");
		}
		
		private Token BRACKET_RIGHT() {
			readNextChar();
			return new Token(Token.TokenType.BRACKET_RIGHT, "]");
		}
		
		private Token QUOTED_STRING() {
			StringBuilder tokenString = new StringBuilder();
			readNextChar();
            while (m_currentChar != '\"' && m_currentChar != '\n' && m_currentChar != '\0')
            {
				tokenString.Append(m_currentChar);
				readNextChar();
			} 
			readNextChar();
			return new Token(Token.TokenType.QUOTED_STRING, tokenString.ToString());
		}
		
		private Token ASSIGNMENT() {
			return new Token(Token.TokenType.ASSIGNMENT, "=");
		}
		
		private Token NAME() {
			StringBuilder tokenString = new StringBuilder();
			do {
				tokenString.Append(m_currentChar);
				readNextChar();
			} while( isLETTER() || isDIGIT() );
			
			Token.TokenType tokenType = Token.TokenType.NAME;
			
            // Keywords
			if(tokenString.ToString() == "if") 
            {
				tokenType = Token.TokenType.IF;
			}
			else if(tokenString.ToString() == "else") 
            {
				tokenType = Token.TokenType.ELSE;
			}
			else if(tokenString.ToString() == "return") 
            {
				tokenType = Token.TokenType.RETURN;
			}
            else if (tokenString.ToString() == "void")
            {
                tokenType = Token.TokenType.BUILT_IN_TYPE_NAME;
            }
            else if (tokenString.ToString() == "number")
            {
                tokenType = Token.TokenType.BUILT_IN_TYPE_NAME;
            }
            else if (tokenString.ToString() == "string")
            {
                tokenType = Token.TokenType.BUILT_IN_TYPE_NAME;
            }
			else if (tokenString.ToString() == "bool")
            {
                tokenType = Token.TokenType.BUILT_IN_TYPE_NAME;
            }
			else if (tokenString.ToString() == "array")
            {
                tokenType = Token.TokenType.BUILT_IN_TYPE_NAME;
            }
			else if (tokenString.ToString() == "var")
            {
                tokenType = Token.TokenType.BUILT_IN_TYPE_NAME;
            }
            else if (tokenString.ToString() == "loop")
            {
                tokenType = Token.TokenType.LOOP;
            }
            else if (tokenString.ToString() == "break")
            {
                tokenType = Token.TokenType.BREAK;
            }
			else if (tokenString.ToString() == "from")
            {
                tokenType = Token.TokenType.FROM;
            }
			else if (tokenString.ToString() == "to")
            {
                tokenType = Token.TokenType.TO;
            }
			else if (tokenString.ToString() == "end")
            {
                tokenType = Token.TokenType.BLOCK_END;
            }
			else if (tokenString.ToString() == "true" || tokenString.ToString() == "false")
            {
                tokenType = Token.TokenType.BOOLEAN_VALUE;
            }

			return new Token(tokenType, tokenString.ToString());
		}
		
		private Token NUMBER(bool negative) {
			StringBuilder tokenString = new StringBuilder();
			if(negative) { tokenString.Append("-"); }
			bool period = false;
			do {
				if (m_currentChar == '.' && !period) {
					tokenString.Append(".");
					period = true;
					readNextChar();
				} else if (m_currentChar == '.') {
					m_errorHandler.errorOccured("Can't have several period signs in a number!", Error.ErrorType.SYNTAX, m_currentLine, m_currentPosition);
					readNextChar();
					break;
				}
				tokenString.Append(m_currentChar);
				readNextChar();
			} while( isDIGIT() || m_currentChar == '.' );
			
			return new Token(Token.TokenType.NUMBER, tokenString.ToString());
		}
		
		private bool isLETTER() {
			
			foreach(char letter in s_letters) {
				if(m_currentChar == letter) return true;
			}
			return false;
		}
		
		private bool isDIGIT() {
			
			foreach(char digit in s_digits) {
				if(m_currentChar == digit) return true;
			}
			return false;
		}
		
		void readNextChar() {
			
		    int c = m_textReader.Read();
		    if (c > 0) {
		        m_currentChar = (char)c;
				m_currentPosition++;
			}
		    else {
				m_currentChar = '\0';
				m_endOfFile = true;
			}
		}
		
		List<Token> m_tokens;
		TextReader m_textReader;
		
		bool m_endOfFile;
		char m_currentChar;
		int m_currentLine;
		int m_currentPosition;
		int m_currentTokenStartPosition;
        bool m_stripOutComments;

		static string s_letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_@!?";
		static string s_digits = "1234567890";
		
		ErrorHandler m_errorHandler;
	}
}

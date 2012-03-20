using System;
namespace ProgrammingLanguageNr1
{
	public class AST_ArrayEndSignal : AST
	{
		public AST_ArrayEndSignal(Token token) : base(token) 
		{
			
		}
		
		public int ArraySize {
			get {
				return this.m_arraySize;
			}
			set {
				m_arraySize = value;
			}
		}

		
		private int m_arraySize;
	}
}


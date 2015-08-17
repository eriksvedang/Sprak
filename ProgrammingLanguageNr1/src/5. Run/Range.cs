using System;

namespace ProgrammingLanguageNr1
{
	public struct Range
	{
		public Range (int pStart, int pEnd, int pStep) : this()
		{
			this.start = pStart;
			this.end = pEnd;
			this.step = pStep;
		}

		public float start {get;set;}
		public float end {get;set;}
		public float step {get;set;}

		public override string ToString ()
		{
			return string.Format ("(from {0} to {1})", start, end);
		}

		static Range NONE = new Range(0, 0, 0);
	}
}


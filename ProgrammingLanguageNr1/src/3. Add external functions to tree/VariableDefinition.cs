using System;

namespace ProgrammingLanguageNr1
{
    [Serializable]
	public class VariableDefinition
	{
        public VariableDefinition() { }

		public VariableDefinition (string pVariableName, object pInitValue)
        {
            variableName = pVariableName;
			initValue = pInitValue;
        }

        public string variableName;
		public object initValue;
	}
}

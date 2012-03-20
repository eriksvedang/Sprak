using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace ProgrammingLanguageNr1.tests
{
	[TestFixture()]
	public class GlobalScope_TEST
	{	
		[Test()]
		public void DefineFloatVariable ()
		{
            Scope scope = new Scope(Scope.ScopeType.MAIN_SCOPE, "global scope");
			VariableSymbol variableSymbol = new VariableSymbol("x", ReturnValueType.NUMBER);
            scope.define(variableSymbol);
            Assert.IsNotNull(scope.resolve("x"));
		}	
	}
}


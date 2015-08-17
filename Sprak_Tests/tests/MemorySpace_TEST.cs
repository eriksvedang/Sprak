using System;
using NUnit.Framework;

namespace ProgrammingLanguageNr1.tests
{
	[TestFixture()]
	public class MemorySpace_TEST
	{
		[Test()]
		public void SaveVariableToMemorySpace ()
		{
			MemorySpaceNodeListCache cache = new MemorySpaceNodeListCache();
			
            AST root = new AST();
			MemorySpace memorySpace = new MemorySpace("globals", root, new Scope(Scope.ScopeType.MAIN_SCOPE, "blah"), cache);
			object val = 5.9f;
			memorySpace.setValue("x", val);
			Assert.AreEqual(typeof(float), memorySpace.getValue("x").GetType());
			Assert.AreEqual(5.9f, (float)memorySpace.getValue("x"));
		}
	}
}


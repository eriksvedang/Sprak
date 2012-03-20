using System;
using NUnit.Framework;
using System.IO;

namespace ProgrammingLanguageNr1.tests
{
	[TestFixture()]
	public class ScopeBuilder_TEST
	{
		static ErrorHandler s_errorHandler = new ErrorHandler();

		[Test()]
		public void DefineVariableFromCode ()
		{
            Tokenizer t = new Tokenizer(s_errorHandler, true);
			Parser p = new Parser(t.process(File.OpenText("code11.txt")), s_errorHandler);
			p.process();

            ScopeBuilder scopeBuilder = new ScopeBuilder(p.getAST(), s_errorHandler);
			scopeBuilder.process();
			Scope globalScope = scopeBuilder.getGlobalScope();
			
			Assert.IsNotNull(globalScope.resolve("x"));
		}
		
		[Test()]
		public void DefineFunction ()
		{
            Tokenizer t = new Tokenizer(s_errorHandler, true);
			Parser p = new Parser(t.process(File.OpenText("code12.txt")), s_errorHandler);
			p.process();

            ScopeBuilder scopeBuilder = new ScopeBuilder(p.getAST(), s_errorHandler);
			scopeBuilder.process();
            Scope globalScope = scopeBuilder.getGlobalScope();
			
			Assert.IsNotNull(globalScope.resolve("foo"));
		}
		
		[Test()]
		public void DeclareAndReferenceFunctionsAndVariables ()
		{
            Tokenizer t = new Tokenizer(s_errorHandler, true);
			Parser p = new Parser(t.process(File.OpenText("code13.txt")), s_errorHandler);
			p.process();

            ScopeBuilder scopeBuilder = new ScopeBuilder(p.getAST(), s_errorHandler);
			scopeBuilder.process();
		}
		
		[Test()]
		public void DeclareVariableInSubscopes ()
		{
            Tokenizer t = new Tokenizer(s_errorHandler, true);
			Parser p = new Parser(t.process(File.OpenText("code16.txt")), s_errorHandler);
			p.process();

            ScopeBuilder scopeBuilder = new ScopeBuilder(p.getAST(), s_errorHandler);
			scopeBuilder.process();
		}
		
		[Test()]
		public void ForgettingEndStatement ()
		{
            Tokenizer t = new Tokenizer(s_errorHandler, true);
			ErrorHandler e = new ErrorHandler();
			Parser p = new Parser(t.process(File.OpenText("code68.txt")), e);
			p.process();

            ScopeBuilder scopeBuilder = new ScopeBuilder(p.getAST(), e);
			scopeBuilder.process();
			
			e.printErrorsToConsole();
			
			Assert.AreEqual(1, e.getErrors().Count);
		}
		
		
	}
}


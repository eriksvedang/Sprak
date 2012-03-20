using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;

namespace ProgrammingLanguageNr1
{
	[TestFixture()]
	public class ErrorHandler_TEST
	{
		[Test()]
		public void BasicErrorHandlerUsage ()
		{
			ErrorHandler errorHandler = new ErrorHandler();
			errorHandler.errorOccured("Test error 1.", Error.ErrorType.UNDEFINED);
			errorHandler.errorOccured("Test error 2.", Error.ErrorType.SYNTAX, 10, 20);
			
			Assert.AreEqual(2, errorHandler.getErrors().Count);
		}
		
		[Test()]
		public void UnrecognizedChar ()
		{
			TextReader reader = File.OpenText("code17.txt");
			ErrorHandler errorHandler = new ErrorHandler();
            Tokenizer tokenizer = new Tokenizer(errorHandler, true);
			tokenizer.process(File.OpenText("code17.txt"));
			List<Error> errors = errorHandler.getErrors();
			Assert.AreEqual(3, errors.Count);
		}
		
		[Test()]
		public void TooManyTokensInStatements ()
		{
			ErrorHandler errorHandler = new ErrorHandler();
            Tokenizer tokenizer = new Tokenizer(errorHandler, true);
			List<Token> tokens = tokenizer.process(File.OpenText("code18.txt"));
			Parser parser = new Parser(tokens, errorHandler);
			parser.process();
			errorHandler.printErrorsToConsole();
			
			Assert.AreEqual(3, errorHandler.getErrors().Count);
		}
		
		[Test()]
		public void FunctionCallWithMissedParanthesis ()
		{
			TextReader programString = new StringReader("f(");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Something is wrong with the argument list",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void FunctionCallWithMissedParanthesis2 ()
		{
			TextReader programString = new StringReader("f(a");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Ending parenthesis is missing in function call",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void FunctionCallWithMissedCommaInArgumentList ()
		{
			TextReader programString = new StringReader("f(a b)");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Comma is missing in argument list",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void IfStatementWithMissedParenthesis ()
		{
			TextReader programString = new StringReader("if(a > b { }");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("If statement isn't complete",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void IfStatementWithStrangeConditional ()
		{
			TextReader programString = new StringReader("if(a, b) { }");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("If statement isn't complete",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void IfStatementWithMissingEndBlock ()
		{
			TextReader programString = new StringReader("if(a == b) { print(42) ");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("If statement isn't complete",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
        //[Test()]
        //public void FunctionWithNoExpressionInReturn ()
        //{
        //    TextReader programString = File.OpenText("code31.txt");
        //    SprakProgram program = new SprakProgram(programString);
        //    program.getErrorHandler().printErrorsToConsole();
			
        //    Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
        //    Assert.AreEqual("No expression in return statement",
        //        program.getErrorHandler().getErrors()[0].getMessage());
        //}
		
		[Test()]
		public void AssignmentWithNoExpression ()
		{
			TextReader programString = File.OpenText("code32.txt");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Missing expression in assignment",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void AssignmentWithNoExpression2 ()
		{
			TextReader programString = File.OpenText("code33.txt");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Missing expression in assignment",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void UsingReservedWordAsVariableName ()
		{
			TextReader programString = File.OpenText("code34.txt");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Can't figure out statement type of token BUILT_IN_TYPE with string float",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		
		[Test()]
		public void CallingUndefinedFunction ()
		{
			TextReader programString = new StringReader("f()");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			//program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Can't find function with name f",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void WrongNumberOfArgumentsToFunction ()
		{
			TextReader programString = File.OpenText("code30.txt");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.run();
			//program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Wrong number of arguments to function",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void FunctionDefinitionWithoutBody ()
		{
			TextReader programString = File.OpenText("code37.txt");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.run();
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Missing curly bracket in beginning of function definition",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void FunctionDefinitionWithoutEndingBracket ()
		{
			TextReader programString = File.OpenText("code38.txt");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			program.run();
			program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Trying to define a function inside a function (are you missing a curly bracket?)",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void DefiningVariableWithUnknownType ()
		{
			TextReader programString = new StringReader("superinteger a");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			//program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Can't find type with name superinteger",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
		
		[Test()]
		public void DeclareVariableTwice ()
		{
			TextReader programString = File.OpenText("code39.txt");
            DefaultSprakRunner program = new DefaultSprakRunner(programString);
			//program.getErrorHandler().printErrorsToConsole();
			
			Assert.AreEqual(1, program.getErrorHandler().getErrors().Count);
			Assert.AreEqual("Trying to redefine symbol with name a",
				program.getErrorHandler().getErrors()[0].getMessage());
		}
	}
}


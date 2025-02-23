using System;
using System.Text.RegularExpressions;
using Xunit;
namespace UnitTests;

public class FindFunctionBodyTests
{
	private readonly Regex methodRegex = new Regex(@"^([\s\t]*)(\/\*[\*]*\/\s*)*([\s\t]*)(template\s*\<(?:[\w\s,<>]+\>\s*))*((?!if\b|else\b|switch\b|case\b|lock\b|using\b|while\b|new\b|catch\b|for\b|foreach\b|public\b|private\b|protected\b|[\s*])(?:[\w:*_&<>,]+?\s+){1,6})([\w:*~_&<>+\-%\/%|=]+\s*)(\(([\w:*_&<>\[\]?\=,\(\)\s]*)\))+[^{;]*?(?:^[^\r\n{]*;?[\s]+){0,10}\{([ \t]*)(\r?\n)", RegexOptions.Compiled);

	[Fact]
	public void TestFindFunctionBody_NoPreprocessorDirectives()
	{
		string input = @"void function() 
		{ 
			return 1; 
		}";

		Match match = methodRegex.Match(input);
		var (start, end) = Utilities.FindFunctionBody(input, match);
		Assert.Equal(20, start);
		Assert.Equal(42, end);
	}

	[Fact]
	public void TestFindFunctionBody2_NoPreprocessorDirectives()
	{
		string input = @"void function() 
		{ 
			return 1; 
		}
		void dummy() 
		{ 
			return 1; 
		}";
		Match match = methodRegex.Match(input);
		var (start, end) = Utilities.FindFunctionBody(input, match);
		Assert.Equal(20, start);
		Assert.Equal(42, end);
	}

	[Fact]
	public void TestFindFunctionBody_WithBlockComment()
	{
		string input = @"void function() 
		{ 
			return 1; 
			/*} 
			void function() 
			{ 
			return 1;*/ 
		}
		void dummy() 
		{ 
			return 1; 
		}";
		Match match = methodRegex.Match(input);
		var (start, end) = Utilities.FindFunctionBody(input, match);
		Assert.Equal(20, start);
		Assert.Equal(96, end);
	}

	[Fact]
	public void TestFindFunctionBody0_WithConditionalDirectives()
	{
		string input = @"int function()
		{
			#ifdef AAA
			}
			#elif BBB
			}
			#else
			#endif
		return 1;
	}
	void dummy() 
	{ 
		return 1; 
	}";
		
		Match match = methodRegex.Match(input);
		var (start, end) = Utilities.FindFunctionBody(input, match);
		Assert.Equal(18, start);
		Assert.Equal(98, end);
	}

	[Fact]
	public void TestFindFunctionBody1_WithConditionalDirectives()
	{
		string input = @"int function()
		{
			#ifdef AAA
			}
			#elif BBB
			}
			#else
				#ifdef CCC
				}
				#endif
			#endif
		return 1;
	}
	void dummy() 
	{ 
		return 1; 
	}";
		
		Match match = methodRegex.Match(input);
		var (start, end) = Utilities.FindFunctionBody(input, match);
		Assert.Equal(18, start);
		Assert.Equal(133, end);
	}

	[Fact]
	public void TestFindFunctionBody2_WithConditionalDirectives()
	{
		string input = @"int function()
		{
			/*}
				void function()
				{
					#ifdef
					{
					return 1;
			*/

			#ifdef AAA
			}
			#elif BBB
			}
			#else
				#ifdef CCC
				}
				#endif
			{ <---
			#endif

		--->}
		return 1;
	}
	void dummy() 
	{ 
		return 1; 
	}";
		
		Match match = methodRegex.Match(input);
		var (start, end) = Utilities.FindFunctionBody(input, match);
		Assert.Equal(18, start);
		Assert.Equal(237, end);
	}

	[Fact]
	public void TestFindFunctionBody3_WithConditionalDirectives()
	{
		string input = @"int function()
		{
			/*}
				void function()
				{
					#ifdef
					{
					return 1;
			*/

			#ifdef AAA
				}
			#elif BBB
				}
			#else
				#ifdef CCC
					}
				#endif
				{ <---
			#endif

			#ifdef AAA
				{
			#endif

			#ifdef AAA
				}
			#elif BBB
				}
			#else
			--->}
			#endif

		return 1;
	}
	void dummy() 
	{ 
		return 1; 
	}";
		
		Match match = methodRegex.Match(input);
		var (start, end) = Utilities.FindFunctionBody(input, match);
		Assert.Equal(18, start);
		Assert.Equal(343, end);
	}

	[Fact]
	public void TestFindFunctionBody4_WithConditionalDirectives()
	{
		string input = @"int function()
		{
			/*}
				void function()
				{
					#ifdef
					{
					return 1;
			*/

			#ifdef AAA
				}
			#elif BBB
				}
			#else
				#ifdef CCC
					}
				#else
				{ <---
				#endif
			#endif

			#ifdef AAA
				{
			#endif

			#ifdef AAA
				}
			#elif BBB
				}
			#else
				#ifndef AAA
				--->}
				#else
				}}}
				#endif
			#endif

		return 1;
	}
	void dummy() 
	{ 
		return 1; 
	}";
		
		Match match = methodRegex.Match(input);
		var (start, end) = Utilities.FindFunctionBody(input, match);
		Assert.Equal(18, start);
		Assert.Equal(404, end);
	}

	[Fact]
	public void TestFindFunctionBody5_WithConditionalDirectives()
	{
		string input = @"int function()
		{
			/*}
				void function()
				{
					#ifdef
					{
					return 1;
			*/

			#ifdef AAA
				}
			#elif BBB
				}
			#else
				#ifdef CCC
					}
				#else
				{ <---
				#endif
			#endif

			#ifdef AAA
				{
			#endif

			#ifdef AAA
				}
			#elif BBB
				}
			#else
				#ifndef AAA
				#else
				}}}
				#endif
				--->}
			#endif

		return 1;
	}
	void dummy() 
	{ 
		return 1; 
	}";
		
		Match match = methodRegex.Match(input);
		var (start, end) = Utilities.FindFunctionBody(input, match);
		Assert.Equal(18, start);
		Assert.Equal(404, end);
	}











}

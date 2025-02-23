namespace UnitTests;

using System.Text.RegularExpressions;

public class RegexTest
{	    
	class TEST_RESULT
	{    
		public string input  = string.Empty;
		public string result = string.Empty;
	};

	private static readonly TEST_RESULT[] _Split_Function_Test =
	{
		new TEST_RESULT { input = "DirectoryInfo dir",                                   result = "dir"        },
		new TEST_RESULT { input = "std::string str",                                     result = "str"        },
		new TEST_RESULT { input = "const std::string str",                               result = "str"        },
		new TEST_RESULT { input = "std::shared:ptr<int> shr_ptr",                        result = "shr_ptr"    },
		new TEST_RESULT { input = "const std::shared:ptr<int> shr_ptr",                  result = "shr_ptr"    },
		new TEST_RESULT { input = "std::array<int, 4> std_arr",                          result = "std_arr"    },
		new TEST_RESULT { input = "const std::array<int, 4> std_arr",                    result = "std_arr"    },
		new TEST_RESULT { input = "int &(*func_ptr)(int a, int::int<int> b)",            result = "*func_ptr"  },
		new TEST_RESULT { input = "std::string str = \"\"",                              result = "str"        },
		new TEST_RESULT { input = "std::shared:ptr<int> shr_ptr= NULL",                  result = "shr_ptr"    },
		new TEST_RESULT { input = "std::array<int, 4> std_arr = {1, 2, 3, 4}",           result = "std_arr"    },
		new TEST_RESULT { input = "int &(*func_ptr)(int a, int::int<int> b) = NULL",     result = "*func_ptr"  },
		new TEST_RESULT { input = "int var = sizeof(*ptr)",                              result = "var"        },
		new TEST_RESULT { input = "int var = AAA::BBB::CCC",                             result = "var"        },
		new TEST_RESULT { input = "int var = AAA::BBB::CCC[D]",                          result = "var"        },
	};	                          

	[Fact]
	public void SplitFunctionParameters()
	{	
	    foreach (var test in _Split_Function_Test)
		{
			var match = Regex.Match(test.input, RegexBuilder.SplitFunctionParameters());
			if (match.Groups[1].Success)
			{
				Assert.Equal(test.result, match.Groups[1].Value);
			}
			else if (match.Groups[2].Success)
			{
				Assert.Equal(test.result, match.Groups[2].Value);
			}
			else
			{
				Assert.True(false);
			}
		}
	}


}
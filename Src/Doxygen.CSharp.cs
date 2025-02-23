using System.Text;
using System.Text.RegularExpressions;

class DoxygenCSharp
{
	/// @brief Recognize a constructor/destructor
	private static readonly string _Constructor_Regex = RegexBuilder.Make(new RegexString("^") +
	                                                                      new RegexCapturePadding() +
	                                                                      new RegexCommentBar() +
	                                                                      new RegexCapturePadding() +
	                                                                      new RegexCSAccessModifier() +
	                                                                      new RegexTemplate() +
	                                                                      new RegexCSConstructorExcludeKeywork() +
	                                                                      new RegexCSCaptureConstructorName() +
	                                                                      new RegexCaptureParameters() +
	                                                                      new RegexCaptureConstructorInit() +
	                                                                      new RegexCaptureUntilBodyBegin());

	/// @brief Recognize a function
	private static readonly string _Function_Regex = RegexBuilder.Make(new RegexString("^") +
	                                                                   new RegexCapturePadding() +
	                                                                   new RegexCommentBar() +
	                                                                   new RegexCapturePadding() +
	                                                                   new RegexCSAccessModifier() +
	                                                                   new RegexTemplate() +
	                                                                   new RegexCSMethodExcludeKeywork() +
	                                                                   new RegexCSCaptureMethodName() +
	                                                                   new RegexCaptureParameters() +
	                                                                   new RegexCaptureUntilBodyBegin());

	/// @brief Ollama client 
	private OllamaClient _Ollama_Client;

	/// @brief Initializes a new instance of the DoxygenCSharp class.
	///        This constructor initializes a new instance of the DoxygenCSharp class with the specified OllamaClient.
	///
	/// @param ollama_client The OllamaClient to be used by this instance.
	public DoxygenCSharp(OllamaClient ollama_client)
	{
		_Ollama_Client = ollama_client;
	}

	/// @brief Verifies and generates Doxygen comments for a C# file.
	///        This method processes a specified C# file to verify existing Doxygen comments and generate new ones if necessary.
	///        It handles constructors and functions within the file, checking for specific patterns and generating appropriate Doxygen blocks.
	///
	/// @param file_name The path to the C# file to be processed.
	/// @param ollama A flag indicating whether to use an external service (Ollama) to generate Doxygen comments.
	/// @param rebuild A flag indicating whether to rebuild existing Doxygen comments.
	/// @param dry_mode A flag indicating whether to perform a dry run without writing changes to the file.
	/// @param with_context A flag indicating whether to set context using Ollama before processing the file.
	public void VerifyAndGenerate(string file_name, bool ollama, bool rebuild, bool dry_mode, bool with_context)
	{
		bool done;
		bool write_to_file;
		string input;
		string class_name;
		BUFFER output;
		BUFFER doxygen_block;
		List<string> error_log;
		List<string> warning_log;
		string buffer;

		done = false;

		doxygen_block.data = "";
		doxygen_block.lines_number = 0;

		class_name = "";
		output.data = "";
		output.lines_number = 0;

		input = File.ReadAllText(file_name, Encoding.Default);
		if ((input.Contains(@"/*" +  "NO DOXYPATCH" + "*/", StringComparison.CurrentCulture)) ||
		    (input.Contains(@"//" +  "NO DOXYPATCH", StringComparison.CurrentCulture)))
		{
			Log.Instance.AppendWarning("Disabled");
			return;
		}

#if DEBUG
		Log.Instance.AppendError(_Constructor_Regex);
		Log.Instance.AppendError(_Function_Regex);
#endif

		if (with_context)
		{
			bool ok = _Ollama_Client.SetContext(input).GetAwaiter().GetResult();
			if (!ok)
			{
				Log.Instance.AppendWarning("Unable to set context for " + file_name);
			}
		}

		// Read the entire file
		write_to_file = false;
		do
		{
			var generic_empty_match = Regex.Match(input, RegexBuilder.GenericEmptyLineRegex());
			if (generic_empty_match.Success)
			{
				if (doxygen_block.lines_number > 0)
				{
					// Saving of a empty line, to compare after with a body function
					Utilities.AddDataToBuffer(ref doxygen_block, generic_empty_match);
					if (rebuild)
					{
						// If rebuild, this block shall be written
						Utilities.AddDataToBuffer(ref output, doxygen_block.data);
						doxygen_block.data = "";
						doxygen_block.lines_number = 0;
					}
				}
				else
				{
					Utilities.AddDataToBuffer(ref output, generic_empty_match);
				}

				// Deletion of matched lines
				input = input[generic_empty_match.Value.Length..];
			}
			else
			{

				var class_name_match = new Regex(RegexBuilder.CSClassStructNamespaceRegex(), RegexOptions.None, TimeSpan.FromSeconds(5)).Match(input);
				var doxygen_generic_match = new Regex(RegexBuilder.DoxygenGenericRegex(), RegexOptions.None, TimeSpan.FromSeconds(5)).Match(input);
				var constructor_match = new Regex(_Constructor_Regex, RegexOptions.None, TimeSpan.FromSeconds(10)).Match(input);
				var function_match = new Regex(_Function_Regex, RegexOptions.None, TimeSpan.FromSeconds(10)).Match(input);

				if (class_name_match.Success)
				{
					// Set current class name
					class_name = class_name_match.Groups[1].Value;

					// Saving of matching line
					Utilities.AddDataToBuffer(ref output, class_name_match);

					// Deletion of matched lines
					input = input[class_name_match.Value.Length..];
				}
				else if (doxygen_generic_match.Success)
				{
					// Saving of a generic Doxygen line, to compare after with a body function
					Utilities.AddDataToBuffer(ref doxygen_block, doxygen_generic_match);

					// Deletion of matched lines
					input = input[doxygen_generic_match.Value.Length..];
				}
				else if (ConstructorMatch(constructor_match))
				{
					string body;
					int body_begin;
					int body_end;

					string return_value;
					string method_name;

					body = "";
					body_begin = 0;
					body_end = 0;

					try
					{
						(body_begin, body_end) = Utilities.FindFunctionBody(input, constructor_match);
						body = input[body_begin..body_end];
					}
					catch
					{
					}
#if DEBUG
					Log.Instance.AppendEvent(class_name + " - " + constructor_match.Value);
#endif
					if (rebuild)
					{
						doxygen_block.data = "";
					}

					// Comparing of Doxygen saved comment block with function name
					// This function should generate a empty skeleton doxygen_block in case of nothing match (function without block comment),
					// or generate warning in case Doxygen skeleton is present but not correct
					if (AnalyzeDoxygenCommentBlock(constructor_match, ref doxygen_block.data, out error_log, out warning_log, out return_value, out method_name))
					{
						if (ollama)
						{
							try
							{
								var ollama_doxygen_comment_block = _Ollama_Client.GenerateDoxygen(constructor_match.Groups[0].Value, class_name, "C#", body).GetAwaiter().GetResult();
								doxygen_block.data = Utilities.MergeDoxygenHeader(doxygen_block.data, ollama_doxygen_comment_block);

#if DEBUG
					Log.Instance.AppendEvent("-------------------------------------------------");
					Log.Instance.AppendEvent(ollama_doxygen_comment_block);
					Log.Instance.AppendEvent("-------------------------------------------------");
#endif

								warning_log.Clear();
								error_log.Clear();

								warning_log.Add(method_name + " - generated by ollama");
							}
							catch
							{

							}
						}

						if (dry_mode)
						{
							doxygen_block.data = "";
							doxygen_block.lines_number = 0;
						}

						write_to_file = !dry_mode;
					}

					foreach (string l in warning_log)
					{
						Log.Instance.AppendWarning(l, file_name, (output.lines_number + 1));
					}

					foreach (string l in error_log)
					{
						Log.Instance.AppendError(l, file_name, (output.lines_number + 1));
					}

					// Adding to output Doxygen block data generated from function AnalyzeDoxygenCommentBlock.
					Utilities.AddDataToBuffer(ref output, doxygen_block.data);
					doxygen_block.data = "";
					doxygen_block.lines_number = 0;

					// Adding method body
					Utilities.AddDataToBuffer(ref output, input[..body_end]);
					input = input[body_end..];
				}
				else if (function_match.Success)
				{
					string body;
					int body_begin;
					int body_end;

					string return_value;
					string method_name;

					body = "";
					body_begin = 0;
					body_end = 0;

					try
					{
						(body_begin, body_end) = Utilities.FindFunctionBody(input, function_match);
						body = input[body_begin..body_end];
					}
					catch
					{
					}

					if (rebuild)
					{
						doxygen_block.data = "";
					}

					// Comparing of Doxygen saved comment block with function name
					// This function should generate a empty skeleton doxygen_block in case of nothing match (function without block comment),
					// or generate warning in case Doxygen skeleton is present but not correct
					if (AnalyzeDoxygenCommentBlock(function_match, ref doxygen_block.data, out error_log, out warning_log, out return_value, out method_name))
					{
						if (ollama)
						{
							try
							{
								var ollama_doxygen_comment_block = _Ollama_Client.GenerateDoxygen(function_match.Groups[0].Value, class_name, "C#", body).GetAwaiter().GetResult();
								doxygen_block.data = Utilities.MergeDoxygenHeader(doxygen_block.data, ollama_doxygen_comment_block);

#if DEBUG
					Log.Instance.AppendEvent("-------------------------------------------------");
					Log.Instance.AppendEvent(ollama_doxygen_comment_block);
					Log.Instance.AppendEvent("-------------------------------------------------");
#endif

								warning_log.Clear();
								error_log.Clear();

								warning_log.Add(method_name + " - generated by ollama");
							}
							catch
							{

							}
						}

						if (dry_mode)
						{
							doxygen_block.data = "";
							doxygen_block.lines_number = 0;
						}

						write_to_file = !dry_mode;
					}

					foreach (string l in warning_log)
					{
						Log.Instance.AppendWarning(l, file_name, (output.lines_number + 1));
					}

					foreach (string l in error_log)
					{
						Log.Instance.AppendError(l, file_name, (output.lines_number + 1));
					}

					// Adding to output Doxygen block data generated from function AnalyzeDoxygenCommentBlock.
					Utilities.AddDataToBuffer(ref output, doxygen_block.data);
					doxygen_block.data = "";
					doxygen_block.lines_number = 0;

					Utilities.AddDataToBuffer(ref output, input[..body_end]);
					input = input[body_end..];
				}
				else
				{
					var comment_block_match = new Regex(RegexBuilder.CommentBlockRegex(), RegexOptions.None, TimeSpan.FromSeconds(5)).Match(input);
					var generic_inline_match = new Regex(RegexBuilder.GenericInlineCommentRegex(), RegexOptions.None, TimeSpan.FromSeconds(5)).Match(input);
					var generic_block_match = new Regex(RegexBuilder.GenericBlockCommentRegex(), RegexOptions.None, TimeSpan.FromSeconds(5)).Match(input);
					var generic_line_match = new Regex(RegexBuilder.GenericLineRegex(), RegexOptions.None, TimeSpan.FromSeconds(5)).Match(input);

					if (comment_block_match.Success)
					{
						if (doxygen_block.lines_number > 0)
						{
							// Save block comments to check
							Utilities.AddDataToBuffer(ref doxygen_block, comment_block_match);
						}
						else
						{
							Utilities.AddDataToBuffer(ref output, comment_block_match);
						}

						// Deletion of matched lines
						input = input[comment_block_match.Value.Length..];
					}
					else if (generic_inline_match.Success)
					{
						if (doxygen_block.lines_number > 0)
						{
							// Save block comments to check
							Utilities.AddDataToBuffer(ref doxygen_block, generic_inline_match);
						}
						else
						{
							Utilities.AddDataToBuffer(ref output, generic_inline_match);
						}

						// Deletion of matched lines
						input = input[generic_inline_match.Value.Length..];
					}
					else if (generic_block_match.Success)
					{
						if (doxygen_block.lines_number > 0)
						{
							// Saving of a generic block of code, to compare after with a body function
							Utilities.AddDataToBuffer(ref doxygen_block, generic_block_match);
						}
						else
						{
							Utilities.AddDataToBuffer(ref output, generic_block_match);
						}

						// Deletion of matched lines
						input = input[generic_block_match.Value.Length..];
					}
					else if (generic_line_match.Success)
					{
						// Line of come found
						// Flushing of Doxygen block cache to output file
						if (doxygen_block.lines_number > 0)
						{
							Utilities.AddDataToBuffer(ref output, doxygen_block.data);
							doxygen_block.data = "";
							doxygen_block.lines_number = 0;
						}

						// Adding line of code found to output file
						Utilities.AddDataToBuffer(ref output, generic_line_match);

						// Deletion of matched lines
						input = input[generic_line_match.Value.Length..];
					}
					else
					{
						// Adding last lines found and let's code procedure
						output.data += input;
						done = true;
					}
				}
			}
		}
		while (!done);

		// Rewriting output file, taking a copy of old file
		if (write_to_file)
		{
			buffer = file_name + ".doxy.bak";

			if (File.Exists(buffer))
			{
				File.Delete(buffer);
			}
			File.Move(file_name, buffer);

			File.WriteAllText(file_name, output.data, Encoding.Default);
		}
	}

	/// @brief Analyzes a Doxygen comment block and extracts relevant information.
	///        This method processes the provided Doxygen comment block to extract class,
	///        brief description, parameters, and return value. It also checks for missing or
	///        incorrect tags and logs any errors or warnings accordingly.
	///
	/// @param input_match The match object containing the function code.
	/// @param doxygen_comments A reference to a string that holds the Doxygen comments.
	/// @param error_log An output list of strings to store error messages.
	/// @param warning_log An output list of strings to store warning messages.
	/// @param return_value An output string to store the function's return value.
	/// @param name
	/// @retval A boolean indicating whether the Doxygen comments were modified and need to be written back to a file.
	private bool AnalyzeDoxygenCommentBlock(Match input_match, ref string doxygen_comments, out List<string> error_log, out List<string> warning_log,
	                                        out string return_value, out string name)
	{
		bool done;
		bool write_to_file;
		Match doxygen_class_match;
		Match doxygen_brief_match;
		Match doxygen_param_match;
		Match doxygen_retval_match;
		Match generic_line_match;
		Regex array_brackets_remover;
		Regex constructor_init_formatter;

		string buffer;
		string non_doxygen_data;
		List<Match> doxygen_class;
		List<Match> doxygen_brief;
		List<Match> doxygen_param;
		List<Match> doxygen_retval_param;
		bool doxygen_retval;

		string function_align;
		string function_name;
		List<string> function_parameters;
		string function_return_value;
		string function_parametrs_body;
		bool function_return;

		return_value = string.Empty;
		name = string.Empty;

		error_log = new List<string>();
		warning_log = new List<string>();

		buffer = doxygen_comments;
		non_doxygen_data = "";
		doxygen_class = new List<Match>();
		doxygen_brief = new List<Match>();
		doxygen_param = new List<Match>();
		doxygen_retval_param = new List<Match>();
		doxygen_retval = false;

		array_brackets_remover = new Regex(@"\[(.*?)\]");
		constructor_init_formatter = new Regex(@"(\)\s*:)|(\)\s*noexcept\s*:)");

		done = false;
		// comments data extraction
		do
		{
			doxygen_class_match = Regex.Match(buffer, RegexBuilder.DoxygenClassRegex());
			doxygen_brief_match = Regex.Match(buffer, RegexBuilder.DoxygenBriefRegex());
			doxygen_param_match = Regex.Match(buffer, RegexBuilder.DoxygenParamRegex());
			doxygen_retval_match = Regex.Match(buffer, RegexBuilder.DoxygenRetvalRegex());
			generic_line_match = Regex.Match(buffer, RegexBuilder.GenericLineRegex());

			if (doxygen_class_match.Success)
			{
				doxygen_class.Add(doxygen_class_match);
				buffer = buffer[doxygen_class_match.Value.Length..];
			}
			else if (doxygen_brief_match.Success)
			{
				doxygen_brief.Add(doxygen_brief_match);
				buffer = buffer[doxygen_brief_match.Value.Length..];
			}
			else if (doxygen_param_match.Success)
			{
				doxygen_param.Add(doxygen_param_match);
				buffer = buffer[doxygen_param_match.Value.Length..];
			}
			else if (doxygen_retval_match.Success)
			{
				doxygen_retval_param.Add(doxygen_retval_match);
				doxygen_retval = true;
				buffer = buffer[doxygen_retval_match.Value.Length..];
			}
			else if (generic_line_match.Success)
			{
				non_doxygen_data += generic_line_match.Value;
				buffer = buffer[generic_line_match.Value.Length..];
			}
			else
			{
				done = true;
			}
		}
		while (!done);

		// Function data extraction
		function_align = input_match.Groups[1].Value;

		string code = input_match.Value;

		string function_modifiers;

		Parser.Do(code, out function_modifiers, out function_return_value, out function_name, out function_parametrs_body);

		return_value = function_return_value;
		name         = function_name;

		function_return_value = function_return_value.Replace("*", "").Replace("&", "").Replace("[]", "");
		function_return_value = array_brackets_remover.Replace(function_return_value, "");

		function_name = function_name.Trim().Replace("*", "").Replace("&", "").Replace("[]", "");
		function_name = array_brackets_remover.Replace(function_name, "");

		if (function_parametrs_body.StartsWith("("))
		{
			function_parametrs_body =  function_parametrs_body[1..];
		}
		if (function_parametrs_body.EndsWith(")"))
		{
			function_parametrs_body =  function_parametrs_body[..^1];
		}

		function_parametrs_body = function_parametrs_body.Trim();

		function_parameters = new List<string>();
		if (function_parametrs_body != "void")
		{
			var parameters = Utilities.SplitParameters(function_parametrs_body);
			foreach (var par in parameters)
			{
				var regex = Regex.Match(par, RegexBuilder.SplitFunctionParameters());
				if (regex.Groups[1].Success)
				{
					buffer = regex.Groups[1].Value.Replace("*", "").Replace("&", "").Replace("[]", "");
					function_parameters.Add(buffer);
				}
				else if (regex.Groups[2].Success)
				{
					buffer = regex.Groups[2].Value.Replace("*", "").Replace("&", "").Replace("[]", "");
					function_parameters.Add(buffer);
				}
			}
		}

		function_return = ((function_return_value.Length != 0) && (!function_return_value.Contains("void") || (input_match.Groups[4].Value.StartsWith("*"))));

		write_to_file = false;

		// Analyzing Doxygen comments
		if ((doxygen_brief.Count == 0) && (doxygen_param.Count == 0) && !doxygen_retval)
		{
			// No Doxygen header found: adding new empty skeleton
			error_log.Add(function_name + " - empty doxygen header added");

			doxygen_comments = function_align + "/// @brief\r\n" + function_align + "///\r\n";
			foreach (string p in function_parameters)
			{
				doxygen_comments += function_align + "/// @param " + p + "\r\n";
			}

			if (function_return)
			{
				doxygen_comments += function_align + "/// @retval\r\n";
			}

			doxygen_comments += non_doxygen_data;

			write_to_file = true;
		}
		else
		{
			// Doxygen header found: checking parameters
			if (doxygen_brief.Count == 0)
			{
				error_log.Add(function_name + " - @brief not found, add it manually");
			}
			else if (doxygen_brief.Count == 1)
			{
				if (doxygen_brief[0].Groups[1].Value.Trim() == "")
				{
					warning_log.Add(function_name + " - empty @brief detected, fix it");
				}
			}
			else
			{
				foreach (Match match in doxygen_brief)
				{
					if (match.Groups[1].Value.Trim() == "")
					{
						warning_log.Add(function_name + " - empty @brief detected, fix it");
					}
				}
			}

			if (doxygen_class.Count == 1)
			{
				if (doxygen_class[0].Groups[1].Value.Trim() == "")
				{
					warning_log.Add(function_name + " - empty @class detected, fix it");
				}
			}
			else if (doxygen_class.Count > 1)
			{
				error_log.Add(function_name + " - more than 1 @class detected, fix it");
			}

			foreach (string p in function_parameters)
			{
				if (!Utilities.ParamListContains(p, doxygen_param))
				{
					error_log.Add(function_name + " - @param '" + p + "' not found, add it manually");
				}
			}

			foreach (Match p in doxygen_param)
			{
				if (Utilities.ParamListContains(p, function_parameters))
				{
					if (p.Groups[4].Value.Trim() == "")
					{
						warning_log.Add(function_name + " - empty @param " + p.Groups[2].Value + " detected, fix it");
					}
				}
				else
				{
					error_log.Add(function_name + " - @param '" + p.Groups[2].Value + "' not present, remove it manually");
				}
			}

			if (function_return)
			{
				if (doxygen_retval_param.Count == 0)
				{
					error_log.Add(function_name + " - @retval not found, add it manually");
				}
				else if (doxygen_retval_param.Count == 1)
				{
					if (doxygen_retval_param[0].Groups[1].Value.Trim() == "")
					{
						warning_log.Add(function_name + " - empty @retval detected, fix it");
					}
				}
				else
				{
					error_log.Add(function_name + " - more than 1 @retval detected, fix it");
				}
			}
			else
			{
				if (doxygen_retval_param.Count > 0)
				{
					error_log.Add(function_name + " - @retval present but function doesn't return, remove it manually");
				}
			}
		}

		return (write_to_file);
	}

	/// @brief Checks if the constructor match is valid.
	///        This method evaluates whether a given Match object represents a successful match and checks if the third group of the match is null or empty.
	///
	/// @param match The Match object to evaluate.
	/// @retval True if the match is successful and the third group is null or empty, otherwise false.
	private bool ConstructorMatch(Match match)
	{
		bool result;

		result = false;

		if (match.Success)
		{
			if ((match.Groups[3].Value == null) || (match.Groups[3].Value == ""))
			{
				result = true;
			}
		}

		return (result);
	}
}

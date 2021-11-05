using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text.RegularExpressions;
using DirWalkerNameSpace;

namespace DoxygenManagerNameSpace
{
	class DoxygenCCPP
	{
		/// @brief Recognize a doxygen brief tag
		private static readonly string _DoxygenBriefRegex = @"^\/\/\/[\s]*@brief([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))";
		/// @brief Recognize a doxygen param tag
		private static readonly string _DoxygenParamRegex = @"^\/\/\/[\s]*@param[\s]*([^\s]+)([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))";
		/// @brief Recognize a doxygen retval tag
		private static readonly string _DoxygenRetvalRegex = @"^\/\/\/[\s]*@retval([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))";
		/// @brief Recognize a generic doxygen tag
		private static readonly string _DoxygenGenericRegex = @"^\/\/\/([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))";

		/// @brief Recognize a generic empty line
		private static readonly string _GenericEmptyLineRegex = @"^\s*(\r\n|\r|\n)";
		/// @brief Recognize a generic inline comment
		private static readonly string _GenericInlineCommentRegex = @"^\s*\/\/([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))";
		/// @brief Recognize a generic block comment
		private static readonly string _GenericBlockCommentRegex = @"^\s*\/\*([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))";
		/// @brief Recognize a generic line of code
		private static readonly string _GenericLineRegex = @"^([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))";

		/// @brief Recognize a function
		private static readonly string _FunctionRegex =
		    @"^(\/\*[\/\*]*(\r\n|\r|\n))?((?!if\b|else\b|while\b|[\s*])(?:[\w:*~_&<>]+?\s+){1,6})([\w:*~_&]+\s*)\(([^);]*)\)[^{;]*?(?:^[^\r\n{]*;?[\s]+){0,10}\{(\r\n|\r|\n)";

		/// @brief Buffer containing data and lines number
		private struct BUFFER
		{
			public string data; ///< string containing data
			public int lines_number; ///< number of lines inserted
		};

		/// @brief Constructor
		public DoxygenCCPP()
		{
		}

		/// @brief Verify and Generate Doxygen header
		///
		/// @param file_name file name to analyze
		public void VerifyAndGenerate(string file_name)
		{
			bool done;
			bool write_to_file;
			string input;
			Match doxygen_generic_match;
			Match function_match;
			Match generic_inline_match;
			Match generic_block_match;
			Match generic_empty_match;
			Match generic_line_match;
			BUFFER output;
			BUFFER doxygen_block;
			List<string> error_log;
			List<string> warning_log;
			string buffer;

			done = false;

			doxygen_block.data = "";
			doxygen_block.lines_number = 0;

			output.data = "";
			output.lines_number = 0;

			// Read the entire file
			input = File.ReadAllText(file_name, Encoding.Default);

			write_to_file = false;
			do
			{
				doxygen_generic_match = Regex.Match(input, _DoxygenGenericRegex);

				function_match = Regex.Match(input, _FunctionRegex);

				generic_inline_match = Regex.Match(input, _GenericInlineCommentRegex);
				generic_block_match = Regex.Match(input, _GenericBlockCommentRegex);
				generic_empty_match = Regex.Match(input, _GenericEmptyLineRegex);
				generic_line_match = Regex.Match(input, _GenericLineRegex);

				if (generic_empty_match.Success)
				{
					if (doxygen_block.lines_number > 0)
					{
						// Saving of a empty line, to compare after with a body funtion
						AddDataToBuffer(ref doxygen_block, generic_empty_match);
					}
					else
					{
						AddDataToBuffer(ref output, generic_empty_match);
					}

					// Deletion of matched lines
					input = input.Substring(generic_empty_match.Value.Length);
				}
				else if (doxygen_generic_match.Success)
				{
					// Saving of a generic Doxygen line, to compare after with a body funtion
					AddDataToBuffer(ref doxygen_block, doxygen_generic_match);

					// Deletion of matched lines
					input = input.Substring(doxygen_generic_match.Value.Length);
				}
				else if (function_match.Success)
				{
					// Comparsion of Doxygen saved comment block with function name
					// This function should generate a empty skeleton doxygen_block in case of nothing match (function without block comment),
					// or generate warning in case Doxygen skeleton is present but not correct
					if (AnalyzeDoxygenCommentBlock(function_match, ref doxygen_block.data, out error_log, out warning_log))
					{
						write_to_file = true;
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
					AddDataToBuffer(ref output, doxygen_block.data);
					doxygen_block.data = "";
					doxygen_block.lines_number = 0;

					// Adding function header
					AddDataToBuffer(ref output, function_match);

					// Deletion of matched lines
					input = input.Substring(function_match.Value.Length);
				}
				else if (generic_inline_match.Success)
				{
					if (doxygen_block.lines_number > 0)
					{
						// Si salva il blocco di commenti da confrontare in seguito
						// con una eventuale funzione
						AddDataToBuffer(ref doxygen_block, generic_inline_match);
					}
					else
					{
						AddDataToBuffer(ref output, generic_inline_match);
					}

					// Deletion of matched lines
					input = input.Substring(generic_inline_match.Value.Length);
				}
				else if (generic_block_match.Success)
				{
					if (doxygen_block.lines_number > 0)
					{
						// Saving of a generic block of code, to compare after with a body funtion
						AddDataToBuffer(ref doxygen_block, generic_block_match);
					}
					else
					{
						AddDataToBuffer(ref output, generic_block_match);
					}

					// Deletion of matched lines
					input = input.Substring(generic_block_match.Value.Length);
				}
				else if (generic_line_match.Success)
				{
					// Line of come found
					// Flusing of Doxygen block cache to output file
					if (doxygen_block.lines_number > 0)
					{
						AddDataToBuffer(ref output, doxygen_block.data);
						doxygen_block.data = "";
						doxygen_block.lines_number = 0;
					}

					// Adding line of code found to output file
					AddDataToBuffer(ref output, generic_line_match);

					// Deletion of matched lines
					input = input.Substring(generic_line_match.Value.Length);
				}
				else
				{
					// Adding last lines found and let's clode procedure
					output.data += input;
					done = true;
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

		/// @brief Verify and Generate Doxygen header
		///
		/// @param function_match regex matching of function
		/// @param doxygen_comments comment found before function
		/// @param error_log list of errors
		/// @param warning_log list of warning
		/// @retval true write comments to file (no Doxygen header found), false nothing to write (Doxygen header found)
		private bool AnalyzeDoxygenCommentBlock(Match function_match, ref string doxygen_comments, out List<string> error_log, out List<string> warning_log)
		{
			int id;
			bool done;
			bool write_to_file;
			Match doxygen_brief_match;
			Match doxygen_param_match;
			Match doxygen_retval_match;
			Match generic_line_match;
			Regex array_brackets_remover;

			string buffer;
			string non_doxygen_data;
			List<Match> doxygen_brief;
			List<Match> doxygen_param;
			List<Match> doxygen_retval_param;
			bool doxygen_retval;

			string function_name;
			List<string> parameters;
			List<string> function_parameters;
			string function_return_parameter;
			bool function_return;

			int begin_parameters_index;
			int end_parameters_index;

			error_log = new List<string>();
			warning_log = new List<string>();

			buffer = doxygen_comments;
			non_doxygen_data = "";
			doxygen_brief = new List<Match>();
			doxygen_param = new List<Match>();
			doxygen_retval_param = new List<Match>();
			doxygen_retval = false;

			array_brackets_remover = new Regex(@"\[(.*?)\]");

			done = false;
			// comments data extraction
			do
			{
				doxygen_brief_match = Regex.Match(buffer, _DoxygenBriefRegex);
				doxygen_param_match = Regex.Match(buffer, _DoxygenParamRegex);
				doxygen_retval_match = Regex.Match(buffer, _DoxygenRetvalRegex);
				generic_line_match = Regex.Match(buffer, _GenericLineRegex);

				if (doxygen_brief_match.Success)
				{
					doxygen_brief.Add(doxygen_brief_match);
					buffer = buffer.Substring(doxygen_brief_match.Value.Length);
				}
				else if (doxygen_param_match.Success)
				{
					doxygen_param.Add(doxygen_param_match);
					buffer = buffer.Substring(doxygen_param_match.Value.Length);
				}
				else if (doxygen_retval_match.Success)
				{
					doxygen_retval_param.Add(doxygen_retval_match);
					doxygen_retval = true;
					buffer = buffer.Substring(doxygen_retval_match.Value.Length);
				}
				else if (generic_line_match.Success)
				{
					non_doxygen_data += generic_line_match.Value;
					buffer = buffer.Substring(generic_line_match.Value.Length);
				}
				else
				{
					done = true;
				}
			}
			while (!done);

			// Function data extraction
			function_return_parameter = function_match.Groups[3].Value.Replace("*", "").Replace("&", "").Replace("[]", "");
			function_return_parameter = array_brackets_remover.Replace(function_return_parameter, "");

			function_name = function_match.Groups[4].Value.Trim().Replace("*", "").Replace("&", "").Replace("[]", "");
			function_name = array_brackets_remover.Replace(function_name, "");

			buffer = function_match.Value;
			begin_parameters_index = buffer.IndexOf(function_name) + function_name.Length;
			buffer = buffer.Substring(begin_parameters_index);

			begin_parameters_index = buffer.IndexOf('(');
			end_parameters_index = buffer.LastIndexOf(')');
			buffer = buffer.Substring(begin_parameters_index + 1, end_parameters_index - begin_parameters_index - 1);

			// Function parameters extraction
			parameters = buffer.Trim().Split(',').ToList();
			for (id = 0; id < parameters.Count; id++)
			{
				parameters[id] = parameters[id].Trim();
			}

			function_parameters = new List<string>();
			foreach (string p in parameters)
			{
				List<string> fp;

				try
				{
					if (p.Contains('(') && p.Contains(')'))
					{
						begin_parameters_index = p.IndexOf('(');
						end_parameters_index = p.IndexOf(')');
						buffer = p.Substring(begin_parameters_index + 1, end_parameters_index - begin_parameters_index - 1);
						buffer = buffer.Replace("*", "").Replace("&", "").Replace("[]", "");
						buffer = array_brackets_remover.Replace(buffer, "");

						function_parameters.Add(buffer);
					}
					else
					{
						fp = new List<string>(p.Split(' '));
						if (fp.Count > 1)
						{
							buffer = fp[fp.Count - 1].Replace("*", "").Replace("&", "").Replace("[]", "");
							buffer = array_brackets_remover.Replace(buffer, "");

							function_parameters.Add(buffer);
						}
					}
				}
				catch
				{
				}
			}
			function_return = ((function_match.Groups[3].Value.Length != 0) && (!function_match.Groups[3].Value.Contains("void") || (function_match.Groups[4].Value.StartsWith("*"))));

			write_to_file = false;

			// Analyzing Doxygen commets
			if ((doxygen_brief.Count == 0) && (doxygen_param.Count == 0) && !doxygen_retval)
			{
				// No Doxygen header found: adding new empty skeleton
				error_log.Add(function_name + " - empty doxygen header added");

				doxygen_comments = "/// @brief\r\n///\r\n";
				foreach (string p in function_parameters)
				{
					doxygen_comments += "/// @param " + p + "\r\n";
				}

				if (function_return)
				{
					doxygen_comments += "/// @retval\r\n";
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
					error_log.Add(function_name + " - more than 1 @brief detected, fix it");
				}

				foreach (string p in function_parameters)
				{
					if (!ParamListContains(p, doxygen_param))
					{
						error_log.Add(function_name + " - @param '" + p + "' not found, add it manually");
					}
				}

				foreach (Match p in doxygen_param)
				{
					if (ParamListContains(p, function_parameters))
					{
						if (p.Groups[2].Value.Trim() == "")
						{
							warning_log.Add(function_name + " - empty @param " + p.Groups[1].Value + " detected, fix it");
						}
					}
					else
					{
						error_log.Add(function_name + " - @param '" + p.Groups[1].Value + "' not present, remove it manually");
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

		/// @brief Counts number of lines in a Regex Match
		///
		/// @retval number of lines
		private int CountLines(Match match)
		{
			Regex regex;

			regex = new Regex(@"(\r\n|\r|\n)");

			return (regex.Matches(match.Value).Count);
		}

		/// @brief Counts number of lines in a string
		///
		/// @retval number of lines
		private int CountLines(string str)
		{
			Regex regex;

			regex = new Regex(@"(\r\n|\r|\n)");

			return (regex.Matches(str).Count);
		}

		/// @brief Concatenate a string to the end of a Regex Match
		///
		private void AddDataToBuffer(ref BUFFER buffer, Match match)
		{
			buffer.data += match.Value;
			buffer.lines_number += CountLines(match);
		}

		/// @brief Concatenate a string to the end of a string
		///
		private void AddDataToBuffer(ref BUFFER buffer, string data)
		{
			buffer.data += data;
			buffer.lines_number += CountLines(data);
		}

		/// @brief Verify if a pattern (param of a function) is contained in a list of Regex Match
		///
		/// @retval true found, false not found
		private bool ParamListContains(string pattern, List<Match> list)
		{
			pattern = pattern.Trim();
			foreach (Match s in list)
			{
				if (s.Groups[1].Value.Trim() == pattern)
				{
					return (true);
				}
			}

			return (false);
		}

		/// @brief Verify if a pattern (param of a function) is contained in a list of string
		///
		/// @retval true found, false not found
		private bool ParamListContains(Match pattern, List<string> list)
		{
			string p;

			p = pattern.Groups[1].Value.Trim();
			foreach (string s in list)
			{
				if (p == s.Trim())
				{
					return (true);
				}
			}

			return (false);
		}

	}
}

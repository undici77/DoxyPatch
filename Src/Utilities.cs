using System.Text.RegularExpressions;
using System.Security.Cryptography;

public class Utilities
{

	/// @brief Merges a reference Doxygen header with a generated comment block.
	///        This function takes two strings: one representing the existing Doxygen header and another representing the generated comment block.
	///        It processes the generated comment block to replace certain keywords, remove specific patterns, and align comments properly before merging them into the original Doxygen header.
	///
	/// @param ref_doxygen The reference Doxygen header as a string.
	/// @param generated_comment_block The generated comment block as a string.
	/// @retval The merged Doxygen header with the processed generated comment block included.
	public static string MergeDoxygenHeader(string ref_doxygen, string generated_comment_block)
	{
		string[] doxygen_lines = ref_doxygen.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
		string[] ollama_query_result_lines = generated_comment_block.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

		List<string> filtered_query_result_lines = new List<string>();
		for (int i = 0; i < ollama_query_result_lines.Length; i++)
		{
			var result_lines = ollama_query_result_lines[i];
			result_lines = result_lines.Replace("@return", "@retval");
			result_lines = result_lines.Replace("@result", "@retval");
			result_lines = result_lines.Replace("[in]", "");
			result_lines = result_lines.Replace("[out]", "");
			result_lines = result_lines.Replace("[in, out]", "");
			result_lines = result_lines.Replace("[out, in]", "");
			result_lines = result_lines.Replace("[in,out]", "");
			result_lines = result_lines.Replace("[out,in]", "");
			result_lines = result_lines.IndexOf('@') >= 0 ? "/// " + result_lines.Substring(result_lines.IndexOf('@')) : result_lines;
			result_lines = Regex.Replace(result_lines.Trim(), @"\s+", " ");
			if (result_lines.StartsWith("*"))
			{
				result_lines = result_lines.Substring(1);
			}
			result_lines = result_lines.Trim();
			if (result_lines != string.Empty)
			{
				filtered_query_result_lines.Add(result_lines);
			}
		}

		// Create a new array to hold the merged lines
		string[] merged_lines = new string[doxygen_lines.Length];

		// Iterate through each line in the source
		for (int i = 0; i < doxygen_lines.Length; i++)
		{
			string source_line = doxygen_lines[i];
			bool match_found = false;

			if (Regex.IsMatch(source_line, @"^\s*///\s+@"))
			{
				var unpadded_source_line = source_line.TrimStart();
				var pad = source_line[..^unpadded_source_line.Length];
				for (int ii = 0; ii < filtered_query_result_lines.Count; ii++)
				{
					var result_lines = filtered_query_result_lines[ii];
					if (!Regex.IsMatch(result_lines, @"^\s*///\s*$"))
					{
						if (result_lines.StartsWith(unpadded_source_line + " "))
						{
							if (match_found)
							{
								result_lines = result_lines.Replace(unpadded_source_line, "/// ");

								merged_lines[i] += Environment.NewLine + pad + AlignString(result_lines, unpadded_source_line);
							}
							else
							{
								merged_lines[i] = result_lines.Replace(unpadded_source_line, source_line);
								match_found = true;
							}
						}
						else if (match_found)
						{
							if (Regex.IsMatch(result_lines, @"^\s*///(?!\s*@)\s+"))
							{
								merged_lines[i] += Environment.NewLine + pad + AlignString(result_lines, unpadded_source_line);
							}
							else
							{
								break;
							}
						}
					}
				}
			}

			if (!match_found)
			{
				merged_lines[i] = source_line;
			}
		}

		return (string.Join(Environment.NewLine, merged_lines) + Environment.NewLine);
	}

	/// @brief Merges comments from generated code into the corresponding positions in the source code.
	///        This function takes two strings representing source code and generated code. It extracts blocks of comments
	///        followed by their respective code from the generated code and inserts these comments into the source code at
	///        the appropriate locations where matching code is found.
	///
	/// @param source_code The original source code as a string.
	/// @param generated_code The generated code containing comments that need to be merged into the source code.
	/// @retval A new string representing the source code with the comments from the generated code inserted appropriately.
	public static string MergeComments(string source_code, string generated_code)
	{
		var source_lines = source_code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
		var generated_lines = generated_code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

		// A generated_block is sum of comments just before and relative code
		List<(List<string> comments, List<string> code_block)> generated_block_list = new List<(List<string>, List<string>)>();
		List<string> current_comments = new List<string>();
		List<string> current_code = new List<string>();

		// Extracting blocks of comments->code from generated
		foreach (var line in generated_lines)
		{
			if (line.TrimStart().StartsWith("//"))
			{
				if (current_code.Count > 0)
				{
					generated_block_list.Add((new List<string>(current_comments), new List<string>(current_code)));
					current_comments.Clear();
					current_code.Clear();
				}
				current_comments.Add(line);
			}
			else if (!string.IsNullOrWhiteSpace(line))
			{
				current_code.Add(line);
			}
		}

		if (current_code.Count > 0 || current_comments.Count > 0)
		{
			generated_block_list.Add((new List<string>(current_comments), new List<string>(current_code)));
		}

		int result_lines_id = 0;
		int last_match_id = 0;

		// Search a match of generated_block is result 
		foreach (var (comments, code_block) in generated_block_list)
		{
			bool match_found = false;

			while (result_lines_id < source_lines.Count && !match_found)
			{
				if (MergeCommentsIsMatch(source_lines, result_lines_id, code_block))
				{
					source_lines.InsertRange(result_lines_id, comments);
					last_match_id = result_lines_id + code_block.Count + comments.Count;
					result_lines_id = last_match_id;

					match_found = true;
				}
				else
				{
					result_lines_id++;
				}
			}

			if (!match_found)
			{
				result_lines_id = last_match_id;
			}
		}

		return (string.Join(Environment.NewLine, source_lines));
	}

	/// @brief Checks if a block of code matches the lines starting from a specified index.
	///        This function compares each line in the provided code block with the corresponding
	///        lines in the given list of strings, starting from the specified index. It trims both sides
	///        of the lines before comparison to ensure that only the content is compared.
	///
	/// @param lines The list of string lines to compare against.
	/// @param start The starting index in the 'lines' list where comparison begins.
	/// @param code_block The block of code (as a list of strings) to match with the lines.
	/// @retval True if all lines in the code block match the corresponding lines in 'lines', false otherwise.
	private static bool MergeCommentsIsMatch(List<string> lines, int start, List<string> code_block)
	{
		if ((start + code_block.Count) > lines.Count)
		{
			return false;
		}

		for (int i = 0; i < code_block.Count; i++)
		{
			if (lines[start + i].Trim() != code_block[i].Trim())
				return false;
		}
		return (true);
	}

	/// @brief Aligns a given string to match the length of a reference string.
	///        This function aligns the provided input string by adding spaces after its prefix ("/// ")
	///        so that the total length matches the length of the reference string plus one.
	///        If the reference string's length is less than or equal to the prefix length, it returns the original input.
	///
	/// @param input The string to be aligned. It should start with "/// ".
	/// @param reference The reference string whose length determines the alignment.
	/// @retval The aligned string with spaces added after the prefix to match the specified length.
	public static string AlignString(string input, string reference)
	{
		const string input_start = "/// ";

		int reference_length = reference.Length + 1;

		if (reference_length <= input_start.Length)
		{
			return input;
		}

		string trimmed_input = input[input_start.Length..].TrimStart();

		string padding = new string(' ', reference_length - input_start.Length);

		return input_start + padding + trimmed_input;
	}

	/// @brief Counts the number of lines in a given Match object.
	///        This function calculates the number of line breaks present in the value of a Match object,
	///        using a regular expression to identify different types of newline characters.
	///
	/// @param match The Match object whose value will be analyzed for line breaks.
	/// @retval The count of line breaks found in the Match object's value.
	public static int CountLines(Match match)
	{
		Regex regex;

		regex = new Regex(@"(\r\n|\r|\n)");

		return (regex.Matches(match.Value).Count);
	}

	/// @brief Counts the number of lines in a given string.
	///        This function calculates the number of lines in the provided string by matching line break characters.
	///        It considers '\r\n', '\r', and '\n' as line breaks.
	///
	/// @param str The input string whose lines are to be counted.
	/// @retval The number of lines in the input string.
	public static int CountLines(string str)
	{
		Regex regex;

		regex = new Regex(@"(\r\n|\r|\n)");

		return (regex.Matches(str).Count);
	}

	/// @brief Adds data from a match to the buffer and updates the line count.
	///        This method appends the value of a given match to the data in the buffer
	///        and increments the buffer's line number by the number of lines in the match.
	///
	/// @param buffer The buffer to which data will be added. This parameter is passed by reference.
	/// @param match The match whose value will be added to the buffer.
	public static void AddDataToBuffer(ref BUFFER buffer, Match match)
	{
		buffer.data += match.Value;
		buffer.lines_number += CountLines(match);
	}

	/// @brief Adds data to a buffer and updates the line count.
	///        This function appends the provided string data to the existing data in the buffer
	///        and increments the buffer's line count by the number of lines in the new data.
	///
	/// @param buffer The buffer to which the data will be added. This parameter is passed by reference.
	/// @param data The string data to add to the buffer.
	public static void AddDataToBuffer(ref BUFFER buffer, string data)
	{
		buffer.data += data;
		buffer.lines_number += CountLines(data);
	}

	/// @brief Checks if a given pattern exists in the second group of any match within a list.
	///        This function trims the provided pattern and iterates through each match in the list,
	///        comparing the trimmed value of the second group of each match to the pattern.
	///
	/// @param pattern The string pattern to search for, which will be trimmed before comparison.
	/// @param list A list of Match objects where each match's second group is checked against the pattern.
	/// @retval True if the pattern matches any second group in the list; otherwise, false.
	public static bool ParamListContains(string pattern, List<Match> list)
	{
		pattern = pattern.Trim();
		foreach (Match s in list)
		{
			if (s.Groups[2].Value.Trim() == pattern)
			{
				return (true);
			}
		}

		return (false);
	}

	/// @brief Checks if a trimmed value from a Match object's second group is contained within a list of strings.
	///        This function retrieves the value from the specified group of a Match object, trims it,
	///        and checks for its presence in a provided list of strings after trimming each element of the list.
	///
	/// @param pattern The Match object containing groups of matched strings.
	/// @param list The list of strings to search within.
	/// @retval True if the trimmed value from the second group is found in the list; otherwise, false.
	public static bool ParamListContains(Match pattern, List<string> list)
	{
		string p;

		p = pattern.Groups[2].Value.Trim();
		foreach (string s in list)
		{
			if (p == s.Trim())
			{
				return (true);
			}
		}

		return (false);
	}

	/// @brief Finds the start and end indices of a method's body in a given string.
	///        This function locates the opening and closing braces of a method's body within a provided string,
	///        taking into account nested structures, strings, characters, comments, and preprocessor directives.
	///
	/// @param input The string containing the source code to search through.
	/// @param input_match A Match object representing the method signature for which the body is sought.
	/// @retval A tuple containing the start index of the opening brace '{' and the end index of the closing brace '}'.
	public static (int start, int end) FindFunctionBody(string input, Match input_match)
	{
		string method = input_match.Value;
		int start_id = method.LastIndexOf('{');
		int end_id = start_id;

		if (start_id != -1)
		{
			int brackets_counter = 0;
			bool in_string = false;
			bool in_char = false;
			bool in_multiline_comment = false;
			bool in_singleline_comment = false;
			bool inside_else_block = false;
			Stack<bool> preprocessor_stack = new Stack<bool>();

			do
			{
				var current_char = input[end_id];
				var prev_char = (end_id > 0) ? input[end_id - 1] : '\0';
				var next_char = (end_id + 1 < input.Length) ? input[end_id + 1] : '\0';

				// Handle multiline comments /* */
				if (!in_string && !in_char && !in_singleline_comment)
				{
					if (!in_multiline_comment && current_char == '/' && next_char == '*')
					{
						in_multiline_comment = true;
						end_id++; // Skip '*'
					}
					else if (in_multiline_comment && current_char == '*' && next_char == '/')
					{
						in_multiline_comment = false;
						end_id++; // Skip '/'
					}
				}

				// Handle single-line comments //
				if (!in_string && !in_char && !in_multiline_comment)
				{
					if (!in_singleline_comment && current_char == '/' && next_char == '/')
					{
						in_singleline_comment = true;
						end_id++; // Skip second '/'
					}
					else if (in_singleline_comment && (current_char == '\n' || current_char == '\r'))
					{
						in_singleline_comment = false; // End of line comment
					}
				}

				// Handle strings and characters
				if (!in_multiline_comment && !in_singleline_comment)
				{
					if (!in_char && (current_char == '"') && (prev_char != '\\'))
					{
						in_string = !in_string;
					}
					else if (!in_string && (current_char == '\'') && (prev_char != '\\'))
					{
						in_char = !in_char;
					}
				}

				// Handle preprocessor directives
				if (!in_string && !in_char && !in_multiline_comment && !in_singleline_comment)
				{
					if (end_id + 6 < input.Length && input.Substring(end_id, 7) == "#ifdef ")
					{
						preprocessor_stack.Push(false);
						inside_else_block = false;
						end_id += 6; // Skip "#ifdef "
					}
					else if (end_id + 7 < input.Length && input.Substring(end_id, 8) == "#ifndef ")
					{
						preprocessor_stack.Push(true);
						inside_else_block = false;
						end_id += 7; // Skip "#ifndef "
					}
					else if (end_id + 11 < input.Length && input.Substring(end_id, 12) == "#if defined ")
					{
						preprocessor_stack.Push(false);
						inside_else_block = false;
						end_id += 11; // Skip "#if defined "
					}
					else if (end_id + 12 < input.Length && input.Substring(end_id, 13) == "#if !defined ")
					{
						preprocessor_stack.Push(true);
						inside_else_block = false;
						end_id += 12; // Skip "#if !defined "
					}
					else if (end_id + 5 < input.Length && input.Substring(end_id, 5) == "#else")
					{
						if (preprocessor_stack.Count > 0)
						{
							bool is_active = preprocessor_stack.Pop();
							preprocessor_stack.Push(!is_active);
							inside_else_block = !is_active;
						}
						end_id += 5; // Skip "#else"
					}
					else if (end_id + 6 < input.Length && input.Substring(end_id, 6) == "#endif")
					{
						if (preprocessor_stack.Count > 0)
						{
							preprocessor_stack.Pop();
						}
						inside_else_block = false;
						end_id += 6; // Skip "#endif"
					}
				}

				// Count brackets outside of strings, characters, and comments
				if (((preprocessor_stack.Count == 0) || preprocessor_stack.Peek() || inside_else_block) && 
				     (!in_string && !in_char && !in_multiline_comment && !in_singleline_comment))
				{
					if (current_char == '{')
					{
						brackets_counter++;
					}
					else if (current_char == '}')
					{
						brackets_counter--;
					}
				}

				end_id++;
			}
			while ((end_id < input.Length) && (brackets_counter != 0));
		}

		if (end_id > input.Length)
		{
			end_id = input.Length;
		}

		return (start_id, end_id);
	}

	/// @brief Splits a string into segments based on commas outside of nested parentheses, angle brackets, and braces.
	///        This function processes the input string to split it into segments wherever a comma is found,
	///        but only when not inside any level of nested parentheses '()', angle brackets '<>', or braces '{}'.
	///        It trims whitespace from each segment before adding it to the result list.
	///
	/// @param input The string to be split into segments.
	/// @retval A List<string> containing the segments extracted from the input string.
	public static List<string> SplitParameters(string input)
	{
		var result = new List<string>();
		
		var current_segment = string.Empty;
		int nested_level = 0;

		for (int i = 0; i < input.Length; i++)
		{
			char current_char = input[i];

			if (current_char == '(' || current_char == '<' || current_char == '{')
			{
				nested_level++;
			}
			else if (current_char == ')' || current_char == '>' || current_char == '}')
			{
				nested_level--;
			}

			if ((nested_level == 0) && (current_char == ','))
			{
				result.Add(current_segment.Trim());
				current_segment = string.Empty;
			}
			else
			{
				current_segment += current_char;
			}
		}

		if (!string.IsNullOrWhiteSpace(current_segment))
		{
			result.Add(current_segment.Trim());
		}

		return result;
	}

    /// @brief Retrieves a list of model files with their SHA-256 hashes from the specified folder and its subfolders.
    ///        This function searches for all files without an extension in the given directory and calculates the SHA-256 hash for each file.
    ///
    /// @param folder_path The path to the folder where the search will be performed.
    /// @retval A list of tuples, each containing the file path and its corresponding SHA-256 hash.
    public static List<(string path, string sha_256)> GetModelFiles(string folder_path)
    {
        if (!Directory.Exists(folder_path))
        {
            throw new DirectoryNotFoundException($"The directory {folder_path} does not exist.");
        }

        var result = new List<(string path, string sha_256)>();

        // Search for all files without an extension in the folder and subfolders
        var files_list = Directory.GetFiles(folder_path, "*", SearchOption.AllDirectories)
                             .Where(file => !Path.HasExtension(file))
                             .ToList();

        foreach (var path in files_list)
        {
            var sha_256 = CalculateSHA256(path);
            result.Add((path, sha_256));
        }

        return (result);
    }

    /// @brief Calculates the SHA-256 hash of a file.
    ///        This function reads the specified file and computes its SHA-256 hash value.
    ///        The resulting hash is returned as a hexadecimal string in lowercase without dashes.
    ///
    /// @param filePath The path to the file for which the SHA-256 hash will be calculated.
    /// @retval A string representing the SHA-256 hash of the file in lowercase hexadecimal format.
    private static string CalculateSHA256(string filePath)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                return (BitConverter.ToString(hashBytes).Replace("-", String.Empty).ToLowerInvariant());
            }
        }
    }
}

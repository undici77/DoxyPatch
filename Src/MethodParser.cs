using System.Text.RegularExpressions;
using System.Text;

public class MethodParser
{
	/// @brief Processes angle brackets in a given code string.
	///        This function processes angle brackets ('<' and '>') from the current position backwards,
	///        appending them to the result StringBuilder. It stops when the brackets are balanced or the start of the string is reached.
	///
	/// @param code The input string containing code with angle brackets.
	/// @param i Reference to the current index in the code string, updated during processing.
	/// @param result Reference to a StringBuilder where processed characters are prepended.
	/// @retval True if angle brackets were processed successfully, false otherwise.
	public static bool ProcessAngleBrackets(string code, ref int i, ref StringBuilder result)
	{
		if (i > 0)
		{
			char current_char = code[i];
			if ((current_char != '<') && (current_char != '>'))
			{
				return (false);
			}

			int brackets_counter = (current_char == '>') ? 1 : -1;
			result.Insert(0, current_char);
			i--;

			while ((i >= 0) && (brackets_counter != 0))
			{
				current_char = code[i];

				result.Insert(0, current_char);

				if ( current_char == '>')
				{
					brackets_counter++;
				}
				else if (current_char == '<')
				{
					brackets_counter--;
				}

				i--;
			}

			while ((i >= 0) && (code[i] == ' '))
			{
				i--;
			}

			return (true);
		}

		return (false);
	}

	/// @brief Processes round brackets in a given code string.
	///        This function processes the round brackets starting from a specified index and updates the result with the bracketed content.
	///        It ensures that the brackets are balanced and correctly nested.
	///
	/// @param code The input code string containing the brackets to be processed.
	/// @param i A reference to the current index in the code string, which is updated during processing.
	/// @param result A reference to a StringBuilder object where the bracketed content will be inserted.
	/// @retval True if the round brackets are successfully processed and balanced; otherwise, false.
	public static bool ProcessRoundBrackets(string code, ref int i, ref StringBuilder result)
	{
		if (i > 0)
		{
			char current_char = code[i];
			if ((current_char != '(') && (current_char != ')'))
			{
				return (false);
			}

			int brackets_counter = (current_char == ')') ? 1 : -1;
			result.Insert(0, current_char);
			i--;

			while ((i >= 0) && (brackets_counter != 0))
			{
				current_char = code[i];

				result.Insert(0, current_char);

				if (current_char == ')')
				{
					brackets_counter++;
				}
				else if ( current_char == '(')
				{
					brackets_counter--;
				}

				i--;
			}

			return (true);
		}

		return (false);
	}

	/// @brief Processes the scope resolution operator in a given code string.
	///        This function checks for the presence of a valid scope resolution operator (::)
	///        at the current position indicated by index i, considering spaces around it. If found,
	///        it inserts "::" at the beginning of the result StringBuilder and updates the index i.
	///
	/// @param code The input code string to process.
	/// @param i Reference to the current index in the code string being processed.
	/// @param result Reference to a StringBuilder where the scope resolution operator will be inserted if found.
	/// @retval True if a valid scope resolution operator is found and processed, false otherwise.
	public static bool ProcessScopeResolutionOperator(string code, ref int i, ref StringBuilder result)
	{
		int id = i;

		if (id > 0)
		{
			if ((code[id] != ' ') && (code[id] != ':'))
			{
				return (false);
			}

			while ((id >= 0) && (code[id] == ' '))
			{
				id--;
			}

			if (id <= 0)
			{
				return (false);
			}

			if (code[id] != ':')
			{
				return (false);
			}

			id--;

			if (code[id] != ':')
			{
				return (false);
			}

			id--;

			while ((id >= 0) && (code[id] == ' '))
			{
				id--;
			}

			result.Insert(0, "::");

			i = id;

			return (true);

		}

		return (false);
	}

	/// @brief Processes and modifies a given C# code string.
	///        This method processes a provided C# code string by trimming it, handling angle brackets,
	///        round brackets, and the scope resolution operator from the end of the string. It uses helper methods
	///        to process these specific characters and constructs the result in reverse order until a space is encountered
	///        or all specified characters are processed.
	///
	/// @param code The reference to the C# code string that needs to be processed.
	/// @retval The modified string after processing, excluding trailing angle brackets, round brackets,
	///         and scope resolution operators from the end. If the input string is null or empty, it returns an empty string.
	public static string Do(ref string code)
	{
		if (string.IsNullOrEmpty(code))
		{
			return string.Empty;
		}

		code = code.Trim();

		StringBuilder result = new StringBuilder();
		bool angle_brackets_processed = false;
		bool round_brackets_processed = false;
		bool scope_resolution_operator_processed = false;
		bool done = false;

		int i = code.Length - 1;
		while ((i >= 0) && !done)
		{
			char current_char = code[i];

			if (!angle_brackets_processed && ProcessAngleBrackets(code, ref i, ref result))
			{
				angle_brackets_processed = true;
			}
			else if (!round_brackets_processed && ProcessRoundBrackets(code, ref i, ref result))
			{
				round_brackets_processed = true;
			}
			else if (!scope_resolution_operator_processed && ProcessScopeResolutionOperator(code, ref i, ref result))
			{
				scope_resolution_operator_processed = true;
			}
			else
			{
				if (current_char == ' ')
				{
					if ((i - 1) >= 0)
					{
						done = true;
					}
				}
				else
				{
					result.Insert(0, current_char);

					i--;
				}
			}
		}

		if (i >= 0)
		{
			code = code[..i];
		}
		else
		{
			code = string.Empty;
		}

		return (result.ToString());
	}

	/// @brief Checks if a character is one of the specified brackets.
	///        This function determines whether the given character is one of the following:
	///        '<', '>', '(', or ')'.
	///
	/// @param c The character to check.
	/// @retval True if the character is one of the specified brackets, otherwise false.
	private static bool IsBrackets(char c)
	{	                       
		return ((c == '<') || (c == '>') || (c == '(') || (c == ')'));
	}
}

public class Parser
{	                                                        
	/// @brief Extracts a template directive from the provided code.
	///        This function searches for and extracts a template directive starting with "template<"
	///        and ending with the matching closing bracket '>'. It updates the input string to remove
	///        the extracted template directive.
	///
	/// @param code The reference to the string containing the code from which the template directive is to be extracted.
	/// @param template The output parameter that will contain the extracted template directive if found.
	/// @retval True if a valid template directive was found and extracted, false otherwise.
	private static bool ExtractTemplateDirective(ref string code, out string template)
	{
		template = string.Empty;
		var match = Regex.Match(code, @"\btemplate\s*<");

		if (!match.Success)
		{
			return false;
		}

		int start_index = match.Index;
		int template_start = match.Index + match.Length;

		int open_brackets = 1;
		int end_index = template_start;

		while ((end_index < code.Length) && (open_brackets > 0))
		{
			if (code[end_index] == '<')
			{
				open_brackets++;
			}
			else if (code[end_index] == '>')
			{
				open_brackets--;
			}

			end_index++;
		}

		if (open_brackets != 0)
		{
			return (false);
		}

		template = code[start_index..end_index].Trim();
		code = code[end_index..];

		return true;
	}

	/// @brief Extracts parameters from a given C# method signature.
	///        This function attempts to extract the parameters section from a provided string that represents a C# method signature.
	///        It modifies the input string to remove the extracted parameters part and outputs the parameters as a separate string.
	///
	/// @param code The reference to the string containing the C# method signature. This will be modified to exclude the parameters section upon successful extraction.
	/// @param parameters The output parameter that will contain the extracted parameters section of the method signature if successful.
	/// @retval Returns true if the parameters were successfully extracted, otherwise returns false.
	private static bool ExtractParameters(ref string code, out string parameters)
	{
		parameters = string.Empty;

		var init_pattern = @"([\w\s\)]:[\w\s_])";
		var match = Regex.Match(code, init_pattern);
		if (match.Success)
		{
			code = code[..(match.Groups[1].Index + 1)];
		}

		int close_brackets_index = code.LastIndexOf(')');
		if (close_brackets_index == -1)
		{
			return false;
		}

		int open_brackets = 0;
		int end_index = close_brackets_index;

		while (end_index >= 0)
		{
			if (code[end_index] == ')')
			{
				open_brackets++;
			}
			else if (code[end_index] == '(')
			{
				open_brackets--;
			}

			if (open_brackets == 0)
			{
				break;
			}

			end_index--;
		}

		if ((open_brackets != 0) || (end_index < 0))
		{
			return (false);
		}

		parameters = code.Substring(end_index, close_brackets_index - end_index + 1).Trim();

		code = code[..end_index];

		return true;
	}

	/// @brief Extracts the method name from a given code snippet.
	///        This function attempts to extract the method name by utilizing the Do method of the MethodParser class.
	///        The extracted method name is returned via the out parameter and the function always returns true.
	///
	/// @param code A reference to the string containing the code snippet from which the method name will be extracted.
	/// @param method_name An output parameter that will hold the extracted method name.
	/// @retval Always returns true, indicating the operation was successful.
	private static bool ExtractMethodName(ref string code, out string method_name)
	{
		method_name = MethodParser.Do(ref code);
		return (true);
	}

	/// @brief Extracts the return type and modifiers from a given C# method signature.
	///        This function uses regular expressions to parse the provided code string and separate the return type and any preceding modifiers.
	///        The parsed return type and modifiers are then output via out parameters, and the original code string is cleared if successful.
	///
	/// @param code A reference to the string containing the C# method signature from which to extract the return type and modifiers.
	/// @param return_type An out parameter that will hold the extracted return type of the method.
	/// @param modifiers
	/// @retval True if the extraction was successful, false otherwise.
	private static bool ExtractReturnTypeAndModifiers(ref string code, out string return_type, out string modifiers)
	{
		return_type = string.Empty;
		modifiers = string.Empty;

		var return_type_pattern = @"(?:explicit\b|static\b|public\b|private\b|protected\b|constexpr\b|virtual\b|[\s*])*([\w<>:*&\(\),\s]*)";
		var match = Regex.Match(code, return_type_pattern);
		if (match.Success)
		{
			return_type = match.Groups[1].Value.Trim();
			modifiers = code[..match.Groups[1].Index].Trim();
			code = "";

			return (true);
		}

		return (false);
	}

	/// @brief Parses a method signature to extract modifiers, return type, method name, and parameters.
	///        This function processes a given code string representing a method signature,
	///        removing comments and extracting the method's modifiers, return type, name, and parameters.
	///
	/// @param code The input string containing the method signature.
	/// @param modifiers Output parameter for the method's access modifiers (e.g., public, private).
	/// @param return_type Output parameter for the method's return type.
	/// @param method_name Output parameter for the method's name.
	/// @param parameters
	/// @retval True if parsing is successful, false otherwise.
	public static bool Do(string code, out string modifiers, out string return_type, out string method_name, out string parameters)
	{
		modifiers = string.Empty;
		return_type = string.Empty;
		method_name = string.Empty;
		parameters = string.Empty;

		string comment_bar_pattern = @"(\/\*[*]*[^\/]+\/)";

		code = Regex.Replace(code, comment_bar_pattern, "").Replace("\r", "").Replace("\n", "").Replace("\t", "");

		string template;

		try
		{
			ExtractTemplateDirective(ref code, out template);
			ExtractParameters(ref code, out parameters);
			ExtractMethodName(ref code, out method_name);
			ExtractReturnTypeAndModifiers(ref code, out return_type, out modifiers);

			return (true);
		}
		catch
		{
			modifiers = string.Empty;
			return_type = string.Empty;
			method_name = string.Empty;
			parameters = string.Empty;
		}

		return (false);
	}

}
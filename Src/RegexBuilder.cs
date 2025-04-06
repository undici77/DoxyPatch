using System.Text;
using System.Text.RegularExpressions;

public class RegexObject
{
	protected string _Regex_String = "";

	/// @brief Returns the regex string.
	///        This function returns the stored regex string.
	///
	/// @retval The stored regex string.
	public string String()
	{
		return (_Regex_String);
	}

	/// @brief Concatenates two RegexObject instances.
	///        This function concatenates the regex string of the 'right' object to that of the 'left' object.
	///
	/// @param left The first RegexObject whose regex string will be modified by appending the second's regex string.
	/// @param right The second RegexObject whose regex string will be appended to the first's regex string.
	/// @retval A new RegexObject instance with the concatenated regex string from both objects.
	public static RegexObject operator +(RegexObject left, RegexObject right)
	{
		left._Regex_String += right.String();

		return (left);
	}
}

class RegexString : RegexObject
{
	/// @brief Initializes a new instance of the RegexString class with the specified regular expression string.
	///        This constructor takes a string representing a regular expression and assigns it to the internal field _Regex_String.
	///
	/// @param regex_string The regular expression string used for pattern matching.
	public RegexString(string regex_string)
	{
		_Regex_String = regex_string;
	}
}

class RegexStringGroup : RegexObject
{
	/// @brief Initializes a new instance of the RegexStringGroup class with a specified regular expression string.
	///        This constructor takes a regular expression string, wraps it in parentheses to form a group,
	///        and assigns it to the private field _Regex_String.
	///
	/// @param regex_string The regular expression pattern to be grouped.
	public RegexStringGroup(string regex_string)
	{
		_Regex_String = "(" + regex_string + ")";
	}
}

public class RegexIgnorePadding : RegexObject
{
    /// @brief Initializes a new instance of the RegexIgnorePadding class.
    ///        This constructor initializes the regex pattern to match zero or more whitespace characters (spaces and tabs).
    ///
    public RegexIgnorePadding()
    {
        _Regex_String = @"(?:[\s\t]*)?";
    }
}

public class RegexCapturePadding : RegexObject
{
    /// @brief Initializes a new instance of the RegexCapturePadding class.
    ///        This constructor initializes the regex pattern to match any whitespace or tab characters.
    ///
    public RegexCapturePadding()
    {
        _Regex_String = @"([\s\t]*)";
    }
}

public class RegexCommentBar : RegexObject
{
    /// @brief Initializes a new instance of the RegexCommentBar class.
    ///        This constructor initializes the _Regex_String field with a regular expression pattern that matches zero or more occurrences of C-style comment blocks.
    ///
    public RegexCommentBar()
    {
        _Regex_String = @"(\/\*[\*]*\/\s*)*";
    }
}

public class RegexHPPConstructorAccessModifier : RegexObject
{
    /// @brief Initializes a new instance of the RegexHPPConstructorAccessModifier class.
    ///        This constructor sets up a regular expression pattern that matches certain access modifiers and keywords in C++ constructors.
    ///
    public RegexHPPConstructorAccessModifier()
    {
        _Regex_String = @"(?:explicit\b|constexpr\b|virtual\b|[\s*])*";
    }
}

public class RegexCSAccessModifier : RegexObject
{
    /// @brief Initializes a new instance of the RegexCSAccessModifier class.
    ///        This constructor sets up a regular expression pattern that matches C# access modifiers.
    ///        The pattern optionally matches any of the specified access modifiers followed by whitespace.
    ///
    public RegexCSAccessModifier()
    {
        _Regex_String = @"(?:public|private|protected|internal|protected internal|private protected)?\s*";
    }
}

public class RegexTemplate : RegexObject
{
    /// @brief Initializes a new instance of the RegexTemplate class.
    ///        This constructor sets up a regular expression pattern that matches template declarations with optional generic type parameters.
    ///
    public RegexTemplate()
    {
        _Regex_String = @"(template\s*\<(?:[\w\s,<>]+\>\s*))*";
    }
}

public class RegexCPPConstructorExcludeKeywork : RegexObject
{
    /// @brief Initializes a new instance of the RegexCPPConstructorExcludeKeywork class.
    ///        This constructor sets up a regular expression pattern that excludes specific C# keywords.
    ///        The pattern ensures that certain keywords like 'if', 'else', 'switch', etc., are not matched.
    ///
    public RegexCPPConstructorExcludeKeywork()
    {
        _Regex_String =
            @"((?!if\b|else\b|switch\b|case\b|lock\b|using\b|while\b|new\b|catch\b|sizeof\b|for\b|foreach\b|public\b|private\b|protected\b|[\s*]))";
    }
}

public class RegexCPPMethodExcludeKeywork : RegexObject
{
    /// @brief Initializes a new instance of the RegexCPPMethodExcludeKeywork class.
    ///        This constructor sets up a regular expression pattern that excludes certain C# keywords and constructs a regex string accordingly.
    ///
    public RegexCPPMethodExcludeKeywork()
    {
        _Regex_String =
            @"((?!if\b|else\b|switch\b|case\b|lock\b|using\b|while\b|new\b|catch\b|sizeof\b|for\b|foreach\b|public\b|private\b|protected\b|[\s*])(?:[\w:*_&<>,]+?\s+){1,6})";
    }
}

public class RegexHPPConstructorExcludeKeywork : RegexObject
{
    /// @brief Initializes a new instance of the RegexHPPConstructorExcludeKeywork class.
    ///        This constructor sets up a regular expression pattern that excludes specific C# keywords and whitespace characters.
    ///
    public RegexHPPConstructorExcludeKeywork()
    {
        _Regex_String =
            @"((?!if\b|else\b|switch\b|case\b|lock\b|using\b|while\b|new\b|catch\b|sizeof\b|for\b|foreach\b|public\b|private\b|protected\b|[\s*]))";
    }
}

public class RegexHPPMethodExcludeKeywork : RegexObject
{
    /// @brief Initializes a new instance of the RegexHPPMethodExcludeKeywork class.
    ///        This constructor sets up a regular expression pattern that matches method signatures in C# code,
    ///        excluding certain keywords such as 'if', 'else', 'switch', etc. The pattern is designed to match
    ///        method signatures with up to six parameters, ensuring that none of the specified keywords are present.
    ///
    public RegexHPPMethodExcludeKeywork()
    {
        _Regex_String =
            @"((?!if\b|else\b|switch\b|case\b|lock\b|using\b|while\b|new\b|catch\b|sizeof\b|for\b|foreach\b|public\b|private\b|protected\b|[\s*])(?:[\w:*_&<>,]+?\s+){1,6})";
    }
}

public class RegexCSConstructorExcludeKeywork : RegexObject
{
    /// @brief Initializes a new instance of the RegexCSConstructorExcludeKeywork class.
    ///        This constructor sets up a regular expression pattern that excludes specific C# keywords and whitespace characters.
    ///
    public RegexCSConstructorExcludeKeywork()
    {
        _Regex_String =
            @"((?!if\b|else\b|switch\b|case\b|lock\b|using\b|while\b|new\b|catch\b|sizeof\b|for\b|foreach\b|public\b|private\b|protected\b|[\s*]))";
    }
}

public class RegexCSMethodExcludeKeywork : RegexObject
{
    /// @brief Initializes a new instance of the RegexCSMethodExcludeKeywork class.
    ///        This constructor sets up a regular expression pattern that excludes certain C# keywords and constructs a regex string accordingly.
    ///
    public RegexCSMethodExcludeKeywork()
    {
        _Regex_String =
            @"((?!if\b|else\b|switch\b|case\b|lock\b|using\b|while\b|new\b|catch\b|sizeof\b|for\b|foreach\b|public\b|private\b|protected\b|[\s*])(?:[\w:*_&<>,]+?\s+){1,6})";
    }
}

public class RegexCPPCaptureConstructorName : RegexObject
{
    /// @brief Initializes a new instance of the RegexCPPCaptureConstructorName class.
    ///        This constructor sets up a regular expression pattern to match C++ constructor declarations.
    ///
    public RegexCPPCaptureConstructorName()
    {
        _Regex_String = @"(?!__attribute__)([\w:*_&<>,]+)\:\:([\w~_]+\s*)";
    }
}

public class RegexHPPCaptureConstructorName : RegexObject
{
    /// @brief Initializes a new instance of the RegexHPPCaptureConstructorName class.
    ///        This constructor sets up the regular expression pattern used for capturing constructor names in HPP files.
    ///
    public RegexHPPCaptureConstructorName()
    {
        _Regex_String = @"(?!__attribute__)([\w:*~_&<>,]+\s*)";
    }
}

public class RegexCPPCaptureMethodName : RegexObject
{
    /// @brief Initializes a new instance of the RegexCPPCaptureMethodName class.
    ///        This constructor sets up the regular expression pattern used to capture method names in C++ code.
    ///        The pattern is designed to match typical C++ method name signatures, including various characters and symbols commonly found in identifiers.
    ///
    public RegexCPPCaptureMethodName()
    {
        _Regex_String = @"(?!__attribute__)([\w:*~_&<>+\-%\/%|=]+\s*)";
    }
}

public class RegexHPPCaptureMethodName : RegexObject
{
    /// @brief Initializes a new instance of the RegexHPPCaptureMethodName class.
    ///        This constructor sets up the regular expression pattern used for capturing method names in HPP files.
    ///
    public RegexHPPCaptureMethodName()
    {
        _Regex_String = @"(?!__attribute__)([\w:*~_&<>+\-%\/%|=]+\s*)";
    }
}

public class RegexCSCaptureConstructorName : RegexObject
{
    /// @brief Initializes a new instance of the RegexCSCaptureConstructorName class.
    ///        This constructor sets up the regular expression pattern used for capturing specific patterns in strings.
    ///
    public RegexCSCaptureConstructorName()
    {
        _Regex_String = @"([\w:*~_&<>+\-%\/%|=]+\s*)";
    }
}

public class RegexCSCaptureMethodName : RegexObject
{
    /// @brief Initializes a new instance of the RegexCSCaptureMethodName class.
    ///        This constructor sets up the regular expression pattern used to capture method names in C# code.
    ///        The pattern is designed to match sequences of word characters, colons, asterisks, tildes, underscores,
    ///        ampersands, angle brackets, plus signs, hyphens, percent signs, slashes, pipes, and equals signs,
    ///        followed by any amount of whitespace.
    ///
    public RegexCSCaptureMethodName()
    {
        _Regex_String = @"([\w:*~_&<>+\-%\/%|=]+\s*)";
    }
}

public class RegexCaptureParameters : RegexObject
{
    /// @brief Initializes a new instance of the RegexCaptureParameters class.
    ///        This constructor sets up the default regular expression pattern used for capturing parameters.
    ///
    public RegexCaptureParameters()
    {
        _Regex_String = @"(\(([\w:*_&<>\[\]?\=,\(\)\s]*)\))+";
    }
}

public class RegexCaptureConstructorInit : RegexObject
{
    /// @brief Initializes a new instance of the RegexCaptureConstructorInit class.
    ///        This constructor initializes the _Regex_String field with a specific regular expression pattern.
    ///        The pattern is designed to match optional whitespace, followed by a colon and more optional whitespace,
    ///        then a sequence of word characters, spaces, commas, curly braces, parentheses, dots, angle brackets,
    ///        asterisks, ampersands, colons, underscores, hyphens, and double quotes. It also matches an opening
    ///        parenthesis followed by any number of characters except closing parenthesis.
    ///
    public RegexCaptureConstructorInit()
    {
        _Regex_String = @"(\s*:\s*((\w+\s*[\(\{]+[^\)\}]*[\)\}]+\,\s*)*(\w+\s*[\(\{]+[^\)\}]*[\)\}]+)+)?)?";
    }
}

public class RegexCaptureUntilBodyBegin : RegexObject
{
    /// @brief Initializes a new instance of the RegexCaptureUntilBodyBegin class.
    ///        This constructor sets up a regular expression pattern that matches a sequence of characters
    ///        not including '{' or ';' followed by an optional sequence of lines ending with semicolons,
    ///        and finally captures the opening brace '{' along with any leading whitespace and newline character.
    ///
    public RegexCaptureUntilBodyBegin()
    {
        _Regex_String = @"[^{;]*?(?:^[^\r\n{]*;?[\s]+){0,1}\{([ \t]*)(\r?\n)";
    }
}

public class RegexBuilder
{
	/// @brief Returns a regex pattern for matching C-style comment blocks.
	///        This function returns a regular expression that matches C-style comment blocks,
	///        which start with '/*' and end with '*/', including any content in between.
	///
	/// @retval A string representing the regex pattern for C-style comment blocks.
	public static string CommentBlockRegex()
	{
		return (@"^\s*\/\*([\s\S]*?)\*\/");
	}

	/// @brief Generates a regular expression pattern for matching C++ class, struct, or namespace declarations.
	///        This function returns a regex pattern that can be used to identify and extract the names of classes,
	///        structs, and namespaces in C++ code. The pattern accounts for optional template specifications and
	///        inheritance clauses.
	///
	/// @retval A string containing the regular expression pattern.
	public static string CPPClassStructNamespaceRegex()
	{
		return (@"^\s*(?:template\s*<[^>]+>\s*)?(?:class|struct|namespace)\s+([a-zA-Z_][a-zA-Z0-9_]*)\b(?:\s*:\s*(?:virtual|public|private|protected)?\s*[^\{]*)?\s*\{");
	}

	/// @brief Generates a regular expression pattern for matching C# class, struct, or namespace declarations.
	///        This function returns a regex pattern that matches the declaration of classes, structs, and namespaces in C#,
	///        optionally preceded by access modifiers and other keywords. The pattern captures the name of the declared entity.
	///
	/// @retval A string containing the regular expression pattern for matching C# class, struct, or namespace declarations.
	public static string CSClassStructNamespaceRegex()
	{
		return (@"^\s*(?:public|private|protected|internal|static|abstract|sealed|partial|\s+)?\s*(?:class|struct|namespace)\s+([a-zA-Z_][a-zA-Z0-9_]*)\b(?:\s*:\s*[a-zA-Z_][a-zA-Z0-9_]*(?:\s*,\s*[a-zA-Z_][a-zA-Z0-9_]*)*)?\s*\{");
	}

	/// @brief Returns a regex pattern for matching Doxygen class comments.
	///
	/// @retval A regex pattern string for matching Doxygen class comments.
	public static string DoxygenClassRegex()
	{
		return (@"^[\s\t]*\/\/\/[\s\t]*@class([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))");
	}
	/// @brief Constructs a regex pattern to match Doxygen brief comments.
	///
	/// @retval A string representing the constructed regex pattern.
	public static string DoxygenBriefRegex()
	{
		return (@"^[\s\t]*\/\/\/[\s\t]*@brief([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))");
	}

	/// @brief Returns a regex pattern for matching Doxygen @param tags.
	///
	/// @retval A string representing the regex pattern for matching Doxygen @param tags.
	public static string DoxygenParamRegex()
	{
		return (@"^[\s\t]*\/\/\/[\s\t]*@param[\s]*(\[[\w]*\])*[\s]*([^\s\[]+)*(\[[\w]*\])*([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))");
	}

	/// @brief Returns a regex pattern for matching Doxygen @retval tags.
	///
	/// @retval tag in documentation comments. The regex accounts for optional whitespace characters before and after the tag, and it supports multi-line continuation using backslashes.
	public static string DoxygenRetvalRegex()
	{
		return (@"^[\s\t]*\/\/\/[\s\t]*@retval([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))");
	}

	/// @brief Generates a generic regex pattern for matching Doxygen comments.
	///        This function returns a regular expression string that matches lines starting with '///',
	///        optionally followed by any characters, including escaped newlines.
	///
	/// @retval A string representing the regex pattern for matching Doxygen comments.
	public static string DoxygenGenericRegex()
	{
		return (@"^[\s\t]*\/\/\/([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))");
	}

	/// @brief Generates a regex pattern for matching empty lines.
	///        This function returns a regular expression that matches lines which contain only whitespace characters,
	///        including spaces and tabs, followed by any of the newline sequences: CRLF (\r\n), CR (\r), or LF (\n).
	///
	/// @retval A string representing the regex pattern for matching empty lines.
	public static string GenericEmptyLineRegex()
	{
		return (@"^[\s\t]*(\r\n|\r|\n)");
	}

	/// @brief Returns a regular expression pattern for matching generic inline comments.
	///        This function returns a string representing a regular expression that matches lines containing only an inline comment in C#.
	///        The regex accounts for optional leading whitespace and tab characters, followed by '//' and any subsequent characters until the end of the line.
	///        It also considers escaped newline sequences within the comment.
	///
	/// @retval A string representing the regular expression pattern for matching generic inline comments.
	public static string GenericInlineCommentRegex()
	{
		return (@"^[\s\t]*\/\/([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))");
	}

	/// @brief Generates a regex pattern for matching generic block comments.
	///        This function returns a regular expression string that matches the start of a generic block comment,
	///        which begins with '/*' and continues until the end of the line, accounting for escaped newlines.
	///
	/// @retval A string representing the regex pattern for matching generic block comments.
	public static string GenericBlockCommentRegex()
	{
		return (@"^[\s\t]*\/\*([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))");
	}

	/// @brief Generates a regular expression pattern for matching generic lines.
	///        This function returns a regex pattern that matches a line of text, including any escaped newline characters.
	///        The pattern accounts for different types of newline characters (\r\n, \r, or \n).
	///
	/// @retval A string representing the regular expression pattern for matching generic lines.
	public static string GenericLineRegex()
	{
		return (@"^([^\r\n]*(?:\\.[^\r\n]*)*(\r\n|\r|\n))");
	}

	/// @brief Returns a regular expression pattern for splitting function parameters.
	///        This method constructs and returns a regex pattern designed to match and split
	///        function parameters in a specific format. The pattern accounts for various syntax
	///        elements such as parameter names, optional array declarations, default values,
	///        and nested parentheses.
	///
	/// @retval A string containing the regular expression pattern used for splitting function parameters.
	public static string SplitFunctionParameters()
	{
		return (@"\s*([\w]+)(?:\s*)(?:\[[\w_]*\])?(?:\s*)(?:\=\s*[\s\*\:""\(\)\{\}\[\]\,\w]*)?$|(?:[\s\&\*\:\w]*[\(]+([^)]+)\)\s*[\(]+[^)]+\)[\,]*)\s*(?:\=\s*[\s\*\:""\(\)\{\}\[\]\,\w]*)?");
	}

	/// @brief Constructs a regular expression string from a RegexObject.
	///        This function takes a RegexObject and returns its string representation.
	///
	/// @param regex_object The RegexObject to be converted to a string.
	/// @retval A string representing the regular expression defined in the RegexObject.
	public static String Make(RegexObject regex_object)
	{
		return (regex_object.String());
	}

}

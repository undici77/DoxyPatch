using DirWalkerNameSpace;
public struct BUFFER
{
	public string data; ///< string containing data
	public int lines_number; ///< number of lines inserted
};

class Doxygen
{
	private static Doxygen _Instance = new Doxygen();
	private static Thread? _Thread;
	private struct PROCEDURE_ARGS
	{
		public string path; ///< path of a direvtory or file name of a file
		public List<string>? extensions; ///< list of extantion files in case of direcotry or null in case of file
		public List<string>? dir_to_ignore; ///< list of directory to ignore
		public bool recursive; ///<	search recursively in sub directory
		public bool ollama; ///< fill signature field with ollama
		public bool rebuild; ///< rebuild existing Doxygen comments
		public bool dry_mode; ///< query analyze
		public bool with_context; ///< run specific model with context management
		public int delay; ///< delay between files processing to cooldown GPU/CPU.
	};
	private static DoxygenCCPP? _Doxygen_CCPP = null;
	private static DoxygenCSharp? _Doxygen_CSharp = null;
	private static DoxygenHPP? _Doxygen_HPP = null;
	public static Doxygen Instance
	{
		get
		{
			return (_Instance);
		}
	}

	/// @brief Initializes a new instance of the Doxygen class.
	///        This constructor does not take any parameters and is used to create an instance of the Doxygen class.
	///
	private Doxygen()
	{
	}

	/// @brief Initializes and starts a procedure with specified parameters.
	///        This method sets up the necessary arguments for a procedure, checks if a thread is already running,
	///        initializes Doxygen objects for different languages, and starts a new thread to execute the procedure.
	///
	/// @param path The file or directory path to process.
	/// @param extensions A list of file extensions to consider. Can be null.
	/// @param dir_to_ignore A list of directories to ignore during processing. Can be null.
	/// @param recursive Indicates whether to process subdirectories recursively.
	/// @param ollama Indicates whether to use Ollama for processing.
	/// @param rebuild Indicates whether to force a rebuild.
	/// @param dry_mode Indicates whether to run in dry mode (no actual changes).
	/// @param with_context Indicates whether to include context in the output.
	/// @param delay The delay in seconds before starting the procedure.
	/// @param ollama_client An instance of OllamaClient for communication with Ollama services.
	public void Procedure(string path, List<string>? extensions, List<string>? dir_to_ignore, bool recursive, bool ollama, bool rebuild, bool dry_mode, bool with_context, int delay, OllamaClient ollama_client)
	{
		PROCEDURE_ARGS args;

		if (_Thread != null)
		{
			return;
		}

		args.path = path;
		args.extensions = extensions;
		args.recursive = recursive;
		args.ollama = ollama;
		args.rebuild = rebuild;
		args.dry_mode = dry_mode;
		args.with_context = with_context;
		args.dir_to_ignore = dir_to_ignore;
		args.delay = delay * 1000;

		_Doxygen_CCPP = new DoxygenCCPP(ollama_client);
		_Doxygen_CSharp = new DoxygenCSharp(ollama_client);
		_Doxygen_HPP = new DoxygenHPP(ollama_client);

		_Thread = new Thread(ThreadProcedure);
		_Thread.Start(args);
		_Thread.Join();
	}

	/// @brief Processes a directory or file to generate Doxygen skeletons.
	///        This method processes either a directory (recursively if specified) or a single file,
	///        verifying and generating Doxygen skeleton files based on the provided arguments.
	///
	/// @param parameter An object containing PROCEDURE_ARGS which includes path, recursive flag,
	///                  extensions filter, delay for cooldown, and other options.
	private void ThreadProcedure(object? parameter)
	{
		DirectoryInfo directory_info;
		List<FileInfo> folder_to_analyze;
		DirWalker? dir_walker;

		Log.Instance.Clear();

		if (parameter == null)
		{
			return;
		}

		PROCEDURE_ARGS args = (PROCEDURE_ARGS)parameter;

		folder_to_analyze = new List<FileInfo>();

		if (Directory.Exists(args.path))
		{
			// Verifying if path is a direcotry
			directory_info = new DirectoryInfo(args.path);

			dir_walker = new DirWalker();
			if (args.recursive)
			{
				dir_walker.RecursiveFileList(directory_info, "*", folder_to_analyze, args.dir_to_ignore);
			}
			else
			{
				dir_walker.FileList(directory_info, "*", folder_to_analyze);
			}

			dir_walker = null;
		}
		else if (File.Exists(args.path))
		{
			// Verifying if path is a file
			folder_to_analyze.Add(new FileInfo(args.path));
		}
		else
		{
			// Error
			_Thread = null;
			return;
		}

		bool cooldown;

		cooldown = false;
		foreach (FileInfo fi in folder_to_analyze)
		{
			if ((args.extensions == null) || (args.extensions.Contains(fi.Extension)))
			{
				// Wait some seconds to cooldown CPU/GPU
				if ((args.delay > 0) && cooldown)
				{
					Log.Instance.AppendEvent("Cooling down GPU/CPU");
					Thread.Sleep(args.delay);
					cooldown = false;
				}

				// Verify and Generate a file with Doxygen skeleton or generating warning
				if (VerifyAndGenerate(fi.FullName, args.ollama, args.rebuild, args.dry_mode, args.with_context))
				{
					cooldown = true;
				}
			}
		}

		_Thread = null;
	}

	/// @brief Verifies and generates documentation for a specified file based on its extension.
	///        This method checks the file extension and delegates the verification and generation process to the appropriate handler.
	///        It supports C, C++, C#, and header files. If the file type is not supported, it logs an event in debug mode and returns false.
	///
	/// @param file_name The name of the file to be verified and processed.
	/// @param ollama A boolean flag indicating whether to use Ollama for processing.
	/// @param rebuild A boolean flag indicating whether to force a rebuild of the documentation.
	/// @param dry_mode A boolean flag indicating whether to run in dry mode (no actual generation).
	/// @param with_context A boolean flag indicating whether to include context information in the generated documentation.
	/// @retval True if the file was successfully processed, false otherwise.
	private bool VerifyAndGenerate(string file_name, bool ollama, bool rebuild, bool dry_mode, bool with_context)
	{
		bool result;
		string extension;

		result = true;
		extension = Path.GetExtension(file_name).ToLower();
		switch (extension)
		{
			case ".c":
			case ".cpp":
				if (_Doxygen_CCPP != null)
				{
					Log.Instance.AppendEvent("Analyzing " + file_name);
					_Doxygen_CCPP.VerifyAndGenerate(file_name, ollama, rebuild, dry_mode, with_context);
				}
				break;

			case ".cs":
#if DEBUG
			case ".csp":
#endif
				if (_Doxygen_CSharp != null)
				{
					Log.Instance.AppendEvent("Analyzing " + file_name);
					_Doxygen_CSharp.VerifyAndGenerate(file_name, ollama, rebuild, dry_mode, with_context);
				}
				break;

			case ".hpp":
			case ".h":
				if (_Doxygen_HPP != null)
				{
					Log.Instance.AppendEvent("Analyzing " + file_name);
					_Doxygen_HPP.VerifyAndGenerate(file_name, ollama, rebuild, dry_mode, with_context);
				}
				break;

			default:
#if DEBUG
				Log.Instance.AppendEvent("Unable to analyze " + file_name);
#endif
				result = false;
				break;
		}

		return (result);
	}
}

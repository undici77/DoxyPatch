using System.Diagnostics;
using System.Reflection;
using System;
using CommandLine;

namespace DoxyPatch
{
	[Verb("doxypatch", HelpText = "Automate Doxygen documentation generation for specified files or directories.")]
	public class Options
	{
		[Value(0, MetaName = "[file or directory]", Required = true, HelpText = "Specify the file or directory path to process.")]
		public string? FileOrDirectory { get; set; }

		[Option('h', "help", Default = false, HelpText = "Show this help message and exit.")]
		public bool ShowHelp { get; set; }

		[Option('r', "recursive", Default = false, HelpText = "Process directories recursively, including all subdirectories and files.")]
		public bool Recursive { get; set; }

		[Option('o', "ollama", Default = false, HelpText = "Enable Ollama mode to automatically generate Doxygen fields for the specified files or directory.")]
		public bool Ollama { get; set; }

		[Option('b', "rebuild", Default = false, HelpText = "Rebuild existing Doxygen fields, overwriting any previously generated documentation.")]
		public bool Rebuild { get; set; }

		[Option('c', "with-context", Default = false, HelpText = "Pass the entire source code as context for more accurate documentation generation (experimental feature).")]
		public bool WithContext { get; set; }

		[Option('m', "dry-mode", Default = false, HelpText = "Run doxypatch in dry mode, simulating the documentation generation process without making any actual changes.")]
		public bool DryMode { get; set; }

		[Option('d', "delay", Default = 0, HelpText = "Specify a delay in seconds between processing files to avoid overheating GPU and CPU.")]
		public int Delay { get; set; }
	}

	class Program
	{
		static string? _Version;
		static string? _Name;

		/// @brief Displays help information for the DoxyPatch tool.
		///        This method prints usage instructions, arguments, and options to the console.
		///        It retrieves properties from the Options class, checks for ValueAttribute and OptionAttribute,
		///        and outputs their respective metadata names, long/short names, and help texts.
		///
		static void ShowHelp()
		{
			var options_type = typeof(Options);
			var properties = options_type.GetProperties();

			Console.WriteLine("Usage: DoxyPatch [file or directory] <options>");
			Console.WriteLine();
			Console.WriteLine("Arguments:");
			foreach (var property in properties)
			{
				var valueAttribute = property.GetCustomAttribute<ValueAttribute>();
				if (valueAttribute != null)
				{
					Console.WriteLine($"    {valueAttribute.MetaName}\r\n        {valueAttribute.HelpText}");
				}
			}

			Console.WriteLine();
			Console.WriteLine("Options:");
			foreach (var property in properties)
			{
				var option_attributes = property.GetCustomAttributes<OptionAttribute>().ToList();
				if (option_attributes.Any())
				{
					var first_option = option_attributes.First();
					var short_name = first_option.ShortName;
					var long_name = $"--{first_option.LongName}";

					Console.Write($"    {short_name}{(string.IsNullOrEmpty(short_name) ? "" : ", ")}{long_name}");

					if (option_attributes.Count > 1)
					{
						Console.Write(", ");
						for (int i = 1; i < option_attributes.Count; i++)
						{
							var opt = option_attributes[i];
							short_name = opt.ShortName;
							long_name = $"--{opt.LongName}";
							Console.Write($"{short_name}{(string.IsNullOrEmpty(short_name) ? "" : ", ")}{long_name}");
						}
					}

					Console.WriteLine($"\r\n        {first_option.HelpText}");
				}
			}
		}


		/// @brief Main entry point of the application.
		///        This function initializes various configuration settings from an INI file,
		///        parses command-line arguments, and performs operations based on those arguments.
		///        It handles server address, model names, pre-prompts, and options for processing files or directories.
		///
		/// @param args Command-line arguments passed to the application.
		static void Main(string[] args)
		{
			string server_address;
			string model_name;
			string model_name_with_context;
			string pre_prompt;
			string pre_prompt_with_class;

			var assembly = Assembly.GetExecutingAssembly();
			var version_attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			var product_attribute = assembly.GetCustomAttribute<AssemblyProductAttribute>();

			_Version = version_attribute?.InformationalVersion ?? "Unknown";
			_Name = product_attribute?.Product ?? "Unknown";

			var ini = new IniFile();
			ini.Load(Path.Combine(AppContext.BaseDirectory, _Name + ".ini"));

			server_address = ini.GetKeyValue("Ollama", "Address");
			if (server_address == string.Empty)
			{
				server_address = "http://localhost:11434";
				ini.SetKeyValue("Ollama", "Address", server_address);
			}

			model_name = ini.GetKeyValue("Ollama", "ModelName");
			if (model_name == string.Empty)
			{
				model_name = "doxypatch:latest";
				ini.SetKeyValue("Ollama", "ModelName", model_name);
			}

			model_name_with_context = ini.GetKeyValue("Ollama", "ModelNameWithContext");
			if (model_name_with_context == string.Empty)
			{
				model_name_with_context = "doxypatch-with-context:latest";
				ini.SetKeyValue("Ollama", "ModelNameWithContext", model_name_with_context);
			}

			pre_prompt = ini.GetKeyValue("Ollama", "PrePrompt");
			if (pre_prompt == string.Empty)
			{
				pre_prompt = "Please provide your best effort, in **English**, adhering to the rules for this method written in '{LANG}':";
				ini.SetKeyValue("Ollama", "PrePrompt", pre_prompt);
			}

			pre_prompt_with_class = ini.GetKeyValue("Ollama", "PrePromptWithClass");
			if (pre_prompt_with_class == string.Empty)
			{
				pre_prompt_with_class = "Please provide your best effort, in **English**, adhering to the rules for this '{CLASS}' class method written in '{LANG}':";
				ini.SetKeyValue("Ollama", "PrePromptWithClass", pre_prompt_with_class);
			}

			ini.Save(Path.Combine(AppContext.BaseDirectory, _Name + ".ini"));

			Console.WriteLine(_Name + " - " + _Version);

			OllamaClient ollama_client;
			Options options;
			string option_enabled = "";

			for (int id = 1; id < args.Length; id++)
			{
				option_enabled += args[id] + " ";
			}

			var parser = new CommandLine.Parser(with => { with.AutoVersion = false; });
			var result = parser.ParseArguments<Options>(args);

			if (result.Tag == ParserResultType.Parsed)
			{
				options = ((Parsed<Options>)result).Value;
			}
			else
			{
				ShowHelp();
				return;
			}

			if (options.ShowHelp || (options.FileOrDirectory == null))
			{
				ShowHelp();
				return;
			}

			Console.WriteLine("OllamaServer: " + server_address);

			if (options.WithContext)
			{
				Console.WriteLine("Model:        " + model_name_with_context);
				ollama_client = new OllamaClient(server_address, model_name_with_context, pre_prompt, pre_prompt_with_class);
			}
			else
			{
				Console.WriteLine("Model:        " + model_name);
				ollama_client = new OllamaClient(server_address, model_name, pre_prompt, pre_prompt_with_class);
			}

			Console.WriteLine("Options:      " + option_enabled);

			if (options.Ollama)
			{
				if (!ollama_client.IsServerOnlineAsync().GetAwaiter().GetResult())
				{
					Console.WriteLine("Ollama server offline");

					return;
				}

				bool save = false;
				var models_list = Utilities.GetModelFiles(Path.Combine(AppContext.BaseDirectory, "Models"));
				foreach (var file in models_list)
				{
					var name = Path.GetFileName(file.path);
					var sha_256 = ini.GetKeyValue("Models", name);
					if (sha_256 != file.sha_256)
					{
						Console.WriteLine("Rebuilding ollama " + name + " model: please wait....");
						ini.SetKeyValue("Models", name, file.sha_256);
						ollama_client.RemoveModel(name);
						ollama_client.CreateModel(name, file.path);
						Console.WriteLine("Rebuilding ollama " + name + " model: done!");

						save = true;
					}
				}
				if (save)
				{
					ini.Save(Path.Combine(AppContext.BaseDirectory, _Name + ".ini"));
				}
			}

			Doxygen.Instance.Procedure(options.FileOrDirectory, null, null, options.Recursive, options.Ollama, options.Rebuild, options.DryMode, options.WithContext, options.Delay, ollama_client);
		}
	}
}


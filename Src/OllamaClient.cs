using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System.Diagnostics;

class OllamaClient
{
	private Uri _Uri;
	private OllamaApiClient _Ollama_Client;
	private string _Model;
	private string _Pre_Prompt;
	private string _Pre_Prompt_With_Class;
	private OllamaSharp.Chat _Chat;


	/// @brief Initializes a new instance of the OllamaClient class.
	///        This constructor sets up the client with the specified address, model, and prompts.
	///
	/// @param address The URI of the Ollama API server.
	/// @param model The name of the model to be used by the client.
	/// @param pre_prompt The initial prompt to be sent to the model without class context.
	/// @param pre_prompt_with_class The initial prompt to be sent to the model with class context.
	public OllamaClient(string address, string model, string pre_prompt, string pre_prompt_with_class)
	{
		_Uri = new Uri(address);
		_Ollama_Client = new OllamaApiClient(_Uri);

		_Model = model;
		_Ollama_Client.SelectedModel = model;
		_Pre_Prompt = pre_prompt;
		_Pre_Prompt_With_Class = pre_prompt_with_class;

		_Chat = new Chat(_Ollama_Client);
	}

	/// @brief Checks if the server is online.
	///        This method sends a GET request to the specified URI and checks if the server responds with a successful status code.
	///
	/// @retval True if the server responds with a successful status code, otherwise false.
	public async Task<bool> IsServerOnlineAsync()
	{
		using (var client = new HttpClient())
		{
			try
			{
				HttpResponseMessage response = await client.GetAsync(_Uri);
				return response.IsSuccessStatusCode;
			}
			catch (HttpRequestException)
			{
				// Server is unreachable
				return false;
			}
		}
	}

	/// @brief Generates a Doxygen comment block for a given method.
	///        This function processes the provided method code by removing any existing Doxygen comments,
	///        normalizing whitespace, and constructing a query to generate a new Doxygen comment block.
	///        The generated comment includes brief descriptions, parameter details, and return value information.
	///
	/// @param method The method code for which the Doxygen comment is to be generated.
	/// @param class_name The name of the class containing the method. If empty, it assumes no class context.
	/// @param language The programming language of the method code.
	/// @param body The body of the method code.
	/// @retval A string containing the generated Doxygen comment block for the provided method.
	public async Task<string> GenerateDoxygen(string method, string class_name, string language, string body)
	{
		method = Regex.Replace(method, @"^\s*/\*+.*?\*/\s*\*?\s*[\r\n]*$", "", RegexOptions.Multiline);
		method = method.Replace("\t", "").Replace("\r", "").Replace("\n", "");
		if (method.EndsWith("{"))
		{
			method = method.TrimEnd('{');
		}

		string query;
		if (class_name == string.Empty)
		{
			query = _Pre_Prompt + "\r\nCode:\r\n" + method + body + "\r\n";
			query = query.Replace("{LANG}", language);
		}
		else
		{
			query = _Pre_Prompt_With_Class + "\r\nCode:\r\n" + method + body + "\r\n";
			query = query.Replace("{LANG}", language);
			query = query.Replace("{CLASS}", class_name);
		}

		string result = string.Empty;

		await foreach (var answer_token in _Chat.Send(query))
		{
			result += answer_token;
		}

		return (result);
	}

	/// @brief Generates a Doxygen comment block for a given method.
	///        This function generates a Doxygen comment block based on the provided method name,
	///        language, and body of the code. It calls an overloaded version of itself with an empty string
	///        as the second parameter.
	///
	/// @param method The name of the method for which to generate the Doxygen comment.
	/// @param language The programming language of the method.
	/// @param body The body of the method.
	/// @retval A task that represents the asynchronous operation and returns a string containing the generated Doxygen comment block.
	public async Task<string> GenerateDoxygen(string method, string language, string body)
	{
		return (await GenerateDoxygen(method, "", language, body));
	}

	/// @brief Sets the context for a chat session.
	///        This method constructs a query by enclosing the provided context within specific tags and sends it to a chat service.
	///        It then processes the response tokens to determine if the operation was successful.
	///
	/// @param context The context string to be set for the chat session.
	/// @retval A boolean value indicating whether the context was successfully set ("done" is found in the result).
	public async Task<bool> SetContext(string context)
	{
		string query;

		query = "@@@begin_ctx@@@\r\n\r\n" + context + "\r\n\r\n@@@end_ctx@@@";

		string result = string.Empty;
		await foreach (var answer_token in _Chat.Send(query))
		{
			result += answer_token;
		}

		return (result.IndexOf("done") != -1);
	}

	/// @brief Creates a model using the 'ollama' command
	///        This method starts a process to create a model with the specified name and file using the Ollama command-line tool.
	///
	/// @param model_name The name of the model to be created.
	/// @param file_name The path to the file used for creating the model.
	public void CreateModel(string model_name, string file_name)
	{
		var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "ollama",
				Arguments = $"create {model_name} -f \"{file_name}\"",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};

		process.Start();
		process.WaitForExit();
	}

	/// @brief Removes a specified model using the 'ollama' command.
	///        This function initiates a process to remove a model by its name using the 'ollama rm' command.
	///        It redirects both standard output and standard error, does not use shell execution, and creates no window.
	///
	/// @param model_name The name of the model to be removed.
	public void RemoveModel(string model_name)
	{
		var process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "ollama",
				Arguments = $"rm \"{model_name}\"",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};

		process.Start();
		process.WaitForExit();
	}
}

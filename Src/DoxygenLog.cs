using System;

class Log
{
	private struct LogInfo
	{
		public string file;
		public int    line;
	};

	private static Log? _Instance;

	private object _Lock;
	private List<LogInfo> _Log_Info;

	public static Log Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = new Log();
			}

			return (_Instance);
		}
	}

	/// @brief Initializes a new instance of the Log class.
	///        This constructor initializes a new instance of the Log class, setting up an internal lock object and an empty list to store log information.
	///
	private Log()
	{
		_Lock = new object();

		_Log_Info = new List<LogInfo>();
	}

	/// @brief Clears the log information.
	///        This function clears all entries from the log information storage.
	///        It ensures thread safety by locking the _Lock object during the operation.
	///
	public void Clear()
	{
		lock (_Lock)
		{
			_Log_Info.Clear();
		}
	}

	/// @brief Appends an event message to the log.
	///        This method creates a new LogInfo object, writes the provided message to the console,
	///        and adds the LogInfo object to the _Log_Info list while ensuring thread safety using a lock.
	///
	/// @param message The string message to be logged and displayed on the console.
	public void AppendEvent(string message)
	{
		LogInfo info;

		info = new LogInfo();

		try
		{
			lock (_Lock)
			{
				Console.WriteLine(message);

				info.file = "";
				info.line = 0;
				_Log_Info.Add(info);
			}
		}
		catch
		{
		}
	}

	/// @brief Appends an event to the log.
	///        This method creates a new LogInfo object, sets its properties based on the provided parameters,
	///        and adds it to the _Log_Info list. It also prints the message to the console within a lock statement
	///        to ensure thread safety.
	///
	/// @param message The message to be logged.
	/// @param file The name of the file where the event occurred.
	/// @param line The line number in the file where the event occurred.
	public void AppendEvent(string message, string file, int line)
	{
		LogInfo info;

		info = new LogInfo();

		try
		{
			lock (_Lock)
			{
				Console.WriteLine(message);

				info.file = file;
				info.line = line;
				_Log_Info.Add(info);
			}
		}
		catch
		{
		}
	}

	/// @brief Appends a warning message to the log.
	///        This method formats the provided message as a warning and appends it to the log.
	///        It prefixes the message with "WRN: ", locks a shared resource, writes the message to the console,
	///        initializes a LogInfo object with default file and line values, and adds this object to the log list.
	///
	/// @param message The warning message to append to the log.
	public void AppendWarning(string message)
	{
		LogInfo info;

		info = new LogInfo();

		try
		{
			message = "WRN: " + message;
			lock (_Lock)
			{
				Console.WriteLine(message);

				info.file = "";
				info.line = 0;
				_Log_Info.Add(info);
			}
		}
		catch
		{
		}
	}

	/// @brief Appends a warning message to the log.
	///        This method formats a warning message with the provided file and line number,
	///        then logs it to the console and stores it in the internal log list.
	///
	/// @param message The warning message to append.
	/// @param file The name of the file where the warning occurred.
	/// @param line The line number in the file where the warning occurred.
	public void AppendWarning(string message, string file, int line)
	{
		LogInfo info;

		info = new LogInfo();

		try
		{
			message =  "WRN: " + file + ":" + line + " " + message;
			lock (_Lock)
			{
				Console.WriteLine(message);

				info.file = file;
				info.line = line;
				_Log_Info.Add(info);
			}
		}
		catch
		{
		}
	}

	/// @brief Appends an error message to the log.
	///        This method formats the provided message as an error and appends it to the log.
	///        It prefixes the message with "ERR: ", locks a shared resource, writes the message to the console,
	///        initializes a LogInfo object with default values for file and line, and adds this object to the log list.
	///
	/// @param message The error message to append to the log.
	public void AppendError(string message)
	{
		LogInfo info;

		info = new LogInfo();

		try
		{
			message = "ERR: " + message;
			lock (_Lock)
			{
				Console.WriteLine(message);

				info.file = "";
				info.line = 0;
				_Log_Info.Add(info);
			}
		}
		catch
		{
		}
	}

	/// @brief Appends an error message to the log.
	///        This method formats an error message with the provided file and line number,
	///        then logs it to the console and stores it in a list of log information.
	///
	/// @param message The error message to append.
	/// @param file The name of the file where the error occurred.
	/// @param line The line number in the file where the error occurred.
	public void AppendError(string message, string file, int line)
	{
		LogInfo info;

		info = new LogInfo();

		try
		{
			message =  "ERR: " + file + ":" + line + " " + message;
			lock (_Lock)
			{
				Console.WriteLine(message);

				info.file = file;
				info.line = line;
				_Log_Info.Add(info);
			}
		}
		catch
		{
		}
	}

	/// @brief Retrieves file and line information based on the provided log ID.
	///        This method attempts to fetch the file name and line number associated with a given log ID from the internal log collection.
	///        If the ID is valid, it assigns the corresponding file and line values. Otherwise, or in case of an exception, it sets both
	///        the file and line to default values (empty string for file and 0 for line).
	///
	/// @param id The identifier of the log entry from which to retrieve information.
	/// @param file Output parameter that will hold the name of the file associated with the log ID upon successful retrieval.
	/// @param line
	public void GetInfo(int id, out string file, out int line)
	{
		try
		{
			if (id < _Log_Info.Count)
			{
				file = _Log_Info[id].file;
				line = _Log_Info[id].line;
			}
			else
			{
				file = "";
				line = 0;
			}
		}
		catch
		{
			file = "";
			line = 0;
		}
	}

}

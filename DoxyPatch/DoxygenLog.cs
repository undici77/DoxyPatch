using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DoxygenManagerNameSpace
{
	class Log
	{
		/// @brief Instance of singleton
		private static Log _Instance;

		/// @brief Lock of write to file procedure
		private object _Lock;

		/// @brief Istance of singleton
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

		/// @brief Constructor
		private Log()
		{
			_Lock = new object();
		}

		/// @brief Append event string message to log file
		///
		/// @param message message to append
		public void AppendEvent(string message)
		{
			try
			{
				lock (_Lock)
				{
					Console.WriteLine(message);
				}
			}
			catch
			{
			}
		}

		/// @brief Append event string message/file/line to log file
		///
		/// @param message message to append
		/// @param file file mame to append
		/// @param line line number to to append
		public void AppendEvent(string message, string file, int line)
		{
			try
			{
				lock (_Lock)
				{
					Console.WriteLine(file + "(" + line + "): event " + message);
				}
			}
			catch
			{
			}
		}

		/// @brief Append warning string message/file/line to log file
		///
		/// @param message message to append
		/// @param file file mame to append
		/// @param line line number to to append
		public void AppendWarning(string message, string file, int line)
		{
			try
			{
				lock (_Lock)
				{
					Console.WriteLine(file + "(" + line + "): warning " + message);
				}
			}
			catch
			{
			}
		}

		/// @brief Append error string message/file/line to log file
		///
		/// @param message message to append
		/// @param file file mame to append
		/// @param line line number to to append
		public void AppendError(string message, string file, int line)
		{
			try
			{
				lock (_Lock)
				{
					Console.WriteLine(file + "(" + line + "): error " + message);
				}
			}
			catch
			{
			}
		}
	}
}


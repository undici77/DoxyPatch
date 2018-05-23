using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using DoxygenManagerNameSpace;

namespace DoxyPatch
{
	class Program
	{
		[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
		private static extern uint TimeBeginPeriod(uint uMilliseconds);

		static string _Version;
		static string _Name;
		static string _Path;

		static void PrintHelp()
		{
			Console.WriteLine("DoxyPatch [file or directory]");
		}

		static void Main(string[] args)
		{
			TimeBeginPeriod(1);

			Assembly assembly = Assembly.GetExecutingAssembly();
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

			_Version = fvi.FileVersion;
			_Name = fvi.ProductName;
			_Path = System.IO.Path.GetDirectoryName(fvi.FileName) + "\\";

			Console.WriteLine(_Name + " - " + _Version);

			if (args.Length != 1)
			{
				PrintHelp();
				return;
			}

			Doxygen.Instance.Procedure(args[0], null);

		}
	}
}

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
	class Doxygen
	{
		/// @brief Instance of singleton
		private static Doxygen _Instance;

		/// @brief Thread descriptor
		private static Thread _Thread;

		/// @brief Arguments passed from cmdline
		private struct PROCEDURE_ARGS
		{
			public string path;	///< path of a direvtory or file name of a file
			public List<string> extensions; ///< list of extantion files in case of direcotry or null in case of file
		};


		/// @brief Parser for C and CPP Files
		private static DoxygenCCPP   _Doxygen_CCPP;

		/// @brief Parser for CSharp Files
		private static DoxygenCSharp _Doxygen_CSharp;

		/// @brief Parser for HPP Files
		private static DoxygenHPP _Doxygen_HPP;

		/// @brief Istance of singleton
		public static Doxygen Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Thread         = null;
					_Instance       = new Doxygen();
					_Doxygen_CCPP   = new DoxygenCCPP();
					_Doxygen_CSharp = new DoxygenCSharp();
					_Doxygen_HPP    = new DoxygenHPP();
				}

				return (_Instance);
			}
		}

		/// @brief Constructor
		private Doxygen()
		{
		}

		/// @brief Procedure of starting parsing and generating file in a separeted thread
		///
		/// @param path string containing directory path or file name
		/// @param extensions string containing list of extansions in case of direcotry
		public void Procedure(string path, List<string> extensions)
		{
			PROCEDURE_ARGS args;

			if (_Thread != null)
			{
				return;
			}

			args.path = path;
			args.extensions = extensions;

			_Thread = new Thread(ThreadProcedure);
			_Thread.Start(args);
			_Thread.Join();

		}

		/// @brief Thread parsing and generating file in a separeted thread
		///
		/// @param parameter generic object containing PROCEDURE_ARGS struct
		private void ThreadProcedure(object parameter)
		{
			DirectoryInfo directory_info;
			List<FileInfo> folder_to_analyze;
			DirWalker dir_walker;

			PROCEDURE_ARGS args = (PROCEDURE_ARGS)parameter;

			folder_to_analyze = new List<FileInfo>();

			if (Directory.Exists(args.path))
			{
				// Verifying if path is a direcotry
				directory_info = new DirectoryInfo(args.path);

				dir_walker = new DirWalker();
				dir_walker.FullDirList(directory_info, "*", folder_to_analyze, null);
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

			foreach (FileInfo fi in folder_to_analyze)
			{
				if ((args.extensions == null) || (args.extensions.Contains(fi.Extension)))
				{
					// Verify and Generate a file with Doxygen skeleton or generating warning
					Log.Instance.AppendEvent("Analyzing " + fi.FullName);
					VerifyAndGenerate(fi.FullName);
				}
			}

			_Thread = null;
		}

		/// @brief Verify and Generate Doxygen header
		///
		/// @param file_name file name to analyze
		private void VerifyAndGenerate(string file_name)
		{
			string extension;

			extension = Path.GetExtension(file_name).ToLower();
			switch (extension)
			{
				case ".c":
				case ".cc":
				case ".cpp":
					_Doxygen_CCPP.VerifyAndGenerate(file_name);
					break;

				case ".cs":
					_Doxygen_CSharp.VerifyAndGenerate(file_name);
					break;

				case ".hpp":
					_Doxygen_HPP.VerifyAndGenerate(file_name);
					break;

				default:
					Log.Instance.AppendEvent("Unable to analyze " + file_name);
					break;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

namespace DirWalkerNameSpace
{
	class DirWalker
	{
		public DirWalker()
		{
		}

		/// @brief Get a full directory list
		///
		/// @param dir start root directory
		/// @param search_pattern pattern of files to search
		/// @param files list of files found
		/// @param dir_to_ignore list of direcory to ignore
		public void FullDirList(DirectoryInfo dir, string search_pattern, List<FileInfo> files, List<string> dir_to_ignore)
		{
			try
			{
				foreach (FileInfo f in dir.GetFiles(search_pattern))
				{
					files.Add(f);
				}
			}
			catch
			{
				return;
			}

			foreach (DirectoryInfo d in dir.GetDirectories())
			{
				if ((dir_to_ignore == null) || !dir_to_ignore.Contains(d.Name))
				{
					FullDirList(d, search_pattern, files, dir_to_ignore);
				}
			}
		}
	}
}

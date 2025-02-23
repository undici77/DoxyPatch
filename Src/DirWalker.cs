using System;

namespace DirWalkerNameSpace
{
	class DirWalker
	{
		/// @brief Initializes a new instance of the DirWalker class.
		///        This constructor sets up any necessary initial state or configurations
		///        required by the DirWalker object. Currently, it does not take any parameters
		///        and does not return any value.
		///
		public DirWalker()
		{
		}

		/// @brief Recursively lists all files in a directory that match a specified pattern.
		///        This method traverses the specified directory and its subdirectories to find all files
		///        that match the given search pattern, excluding directories listed in dir_to_ignore.
		///
		/// @param dir The root directory to start the file listing from.
		/// @param search_pattern The pattern to match against file names (e.g., "*.txt").
		/// @param files A list to store the FileInfo objects of matching files.
		/// @param dir_to_ignore
		public void RecursiveFileList(DirectoryInfo dir, string search_pattern, List<FileInfo> files, List<string>? dir_to_ignore)
		{
			if (FileList(dir, search_pattern, files))
			{
				foreach (DirectoryInfo d in dir.GetDirectories())
				{
					if ((dir_to_ignore == null) || !dir_to_ignore.Contains(d.Name))
					{
						RecursiveFileList(d, search_pattern, files, dir_to_ignore);
					}
				}
			}
		}

		/// @brief Populates a list with files from a specified directory that match a search pattern.
		///        This method searches for files within the provided directory that match the given search pattern.
		///        If a file named ".doxypatch_ignore" is found, it returns false without adding any files to the list.
		///        Otherwise, it adds all matching files to the provided list and returns true.
		///
		/// @param dir The DirectoryInfo object representing the directory to search in.
		/// @param search_pattern The pattern to match against file names.
		/// @param files A reference to a List<FileInfo> that will be populated with the found files.
		/// @retval True if no ".doxypatch_ignore" file is found and files are added successfully, false otherwise.
		public bool FileList(DirectoryInfo dir, string search_pattern, List<FileInfo> files)
		{
			List<FileInfo> list;

			list = new List<FileInfo>();

			try
			{
				foreach (FileInfo f in dir.GetFiles(search_pattern))
				{
					if (f.Name == ".doxypatch_ignore")
					{
						return (false);
					}
					else
					{
						list.Add(f);
					}
				}

				files.AddRange(list);
			}
			catch
			{
			}

			return (true);
		}
	}
}

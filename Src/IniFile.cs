using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type
#pragma warning disable CS8603 // Possible null reference return
#pragma warning disable CS8604 // Possible null reference argument
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member

public class IniFile
{
	private ArrayList _Sections;
	private string _File_Name;

	public System.Collections.ICollection SectionsName
	{
		get
		{
			return (_Sections);
		}
	}

	/// @brief Initializes a new instance of the IniFile class.
	///        This constructor initializes a new instance of the IniFile class and creates an empty list to store sections.
	///
	public IniFile()
	{
		_Sections = new ArrayList();
	}

	/// @brief Loads an INI file with default behavior.
	///        This function loads an INI file from the specified path without performing a reload if it's already loaded.
	///
	/// @param file_name The path to the INI file to load.
	public void Load(string file_name)
	{
		Load(file_name, false);
	}

	/// @brief Loads an INI file into the IniFile object.
	///        This method reads and parses an INI file, optionally merging its contents with existing data.
	///        It handles sections, keys, values, comments, and quoted strings within the file.
	///
	/// @param file_name The path to the INI file to be loaded.
	/// @param merge If true, merges the new data with existing data; if false, clears existing data before loading.
	public void Load(string file_name, bool merge)
	{
		IniSection temp_section;
		StreamReader stream_reader;
		Regex regex_comment;
		Regex regex_section;
		Regex regex_key_value;
		Regex regex_key_value_comment;
		Regex regex_key_value_string;
		Regex regex_key_value_string_comment;
		string line;
		Match match;

		Debug.WriteLine(string.Format("Loading: {0}", file_name));

		if (!merge)
		{
			RemoveAllSections();
			_File_Name = "";
		}

		_File_Name = _File_Name + "," + file_name;

		temp_section                   = null;
		stream_reader                  = new StreamReader(new FileStream(file_name, FileMode.OpenOrCreate, FileAccess.Read));
		regex_comment                  = new Regex("^([\\s]*;.*)", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
		regex_section                  = new Regex("^[\\s]*\\[[\\s]*([^\\[\\s]*[^\\s\\]])[\\s]*\\][\\s]*$", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
		regex_key_value                = new Regex("^\\s*([^=\\s]*)[^=]*=(.*)", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
		regex_key_value_comment        = new Regex("^\\s*([^=\\s]*)[^=]*=(.*);(.*)", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
		regex_key_value_string         = new Regex("^\\s*([^=\\s]*)[^=]*=\"(.*)\"", (RegexOptions.Singleline | RegexOptions.IgnoreCase));
		regex_key_value_string_comment = new Regex("^\\s*([^=\\s]*)[^=]*=\"(.*)\";(.*)", (RegexOptions.Singleline | RegexOptions.IgnoreCase));

		while (!stream_reader.EndOfStream)
		{
			line = stream_reader.ReadLine();

			if (line != string.Empty)
			{
				match = null;

				if (regex_comment.Match(line).Success)
				{
					// Comment
					match = regex_comment.Match(line);
					Debug.WriteLine(string.Format("Skipping comment: {0}", match.Groups[0].Value));
				}
				else if (regex_section.Match(line).Success)
				{
					// Section
					match = regex_section.Match(line);
					Debug.WriteLine(string.Format("Section [{0}]", match.Groups[1].Value));
					temp_section = AddSection(match.Groups[1].Value);
				}
				else if (regex_key_value_string_comment.Match(line).Success && (temp_section != null))
				{
					// Key="Value";Comment
					match = regex_key_value_string_comment.Match(line);
					Debug.WriteLine(string.Format("Key {0}={1};{2}", match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
					temp_section.AddKey(match.Groups[1].Value).Value = match.Groups[2].Value;
					temp_section.AddKey(match.Groups[1].Value).Comment = match.Groups[3].Value;
					temp_section.AddKey(match.Groups[1].Value).Quotes = true;
				}
				else if (regex_key_value_string.Match(line).Success && (temp_section != null))
				{
					// Key="Value"
					match = regex_key_value_string.Match(line);
					Debug.WriteLine(string.Format("Key {0}=\"{1}\"", match.Groups[1].Value, match.Groups[2].Value));
					temp_section.AddKey(match.Groups[1].Value).Value = match.Groups[2].Value;
					temp_section.AddKey(match.Groups[1].Value).Quotes = true;
				}
				else if (regex_key_value_comment.Match(line).Success && (temp_section != null))
				{
					// Key=Value;Comment
					match = regex_key_value_comment.Match(line);
					Debug.WriteLine(string.Format("Key {0}={1};{2}", match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
					temp_section.AddKey(match.Groups[1].Value).Value = match.Groups[2].Value;
					temp_section.AddKey(match.Groups[1].Value).Comment = match.Groups[3].Value;
					temp_section.AddKey(match.Groups[1].Value).Quotes = false;
				}
				else if (regex_key_value.Match(line).Success && (temp_section != null))
				{
					// Key=Value
					match = regex_key_value.Match(line);
					Debug.WriteLine(string.Format("Key {0}={1}", match.Groups[1].Value, match.Groups[2].Value));
					temp_section.AddKey(match.Groups[1].Value).Value = match.Groups[2].Value;
					temp_section.AddKey(match.Groups[1].Value).Quotes = false;
				}
				else if (temp_section != null)
				{
					//  Handle Key without value
					Debug.WriteLine(string.Format("Key {0}", line));
					temp_section.AddKey(line);
				}
				else
				{
					Debug.WriteLine(string.Format("unknown type of data: {0}", line));
				}
			}
		}

		stream_reader.Close();

		Debug.WriteLine(string.Format("{0}: loaded", file_name));
	}

	/// @brief Saves the IniFile to a specified file.
	///        This method writes the contents of the IniFile object to a file in INI format. It iterates through each section and key,
	///        writing them to the file with appropriate formatting, including handling quotes and comments for keys.
	///
	/// @param file_name The name of the file where the IniFile will be saved.
	public void Save(string file_name)
	{
		StreamWriter stream_writer;

		Debug.WriteLine(string.Format("Saving: {0}", file_name));

		stream_writer = new StreamWriter(file_name, false);
		foreach (IniSection s in SectionsName)
		{
			Debug.WriteLine(string.Format("Section: [{0}]", s.Name));
			stream_writer.WriteLine(string.Format("[{0}]", s.Name));

			foreach (IniSection.IniKey k in s.Keys)
			{
				if (k.Comment == null)
				{
					if (k.Quotes == true)
					{
						Debug.WriteLine(string.Format("Key: {0}={1}", k.Name, k.Value));
						stream_writer.WriteLine(string.Format("{0}=\"{1}\"", k.Name, k.Value));
					}
					else
					{
						Debug.WriteLine(string.Format("Key: {0}={1}", k.Name, k.Value));
						stream_writer.WriteLine(string.Format("{0}={1}", k.Name, k.Value));
					}

				}
				else
				{
					if (k.Quotes == true)
					{
						Debug.WriteLine(string.Format("Key: {0}=\"{1}\";{2}", k.Name, k.Value, k.Comment));
						stream_writer.WriteLine(string.Format("{0}=\"{1}\";{2}", k.Name, k.Value, k.Comment));
					}
					else
					{
						Debug.WriteLine(string.Format("Key: {0}={1};{2}", k.Name, k.Value, k.Comment));
						stream_writer.WriteLine(string.Format("{0}={1};{2}", k.Name, k.Value, k.Comment));
					}
				}
			}
		}

		stream_writer.Close();

		Debug.WriteLine(string.Format("{0}: saved", file_name));
	}

	/// @brief Adds a section to the IniFile.
	///        This method adds a new section with the specified name if it does not already exist.
	///        If the section already exists, it returns the existing section.
	///
	/// @param section_name The name of the section to add or retrieve.
	/// @retval An IniSection object representing the added or existing section.
	public IniSection AddSection(string section_name)
	{
		int        id;
		IniSection section;

		section_name = section_name.Trim();

		id = _Sections.IndexOf(section_name.Trim());

		if (id == -1)
		{
			section = new IniSection(this, section_name);
			_Sections.Add(section);
		}
		else
		{
			section = (IniSection)_Sections[id];
		}

		return (section);
	}

	/// @brief Removes a section from the IniFile.
	///        This function removes a section specified by its name from the IniFile.
	///        It trims any leading or trailing whitespace from the section name before attempting to remove it.
	///
	/// @param section_name The name of the section to be removed.
	/// @retval True if the section was successfully removed, otherwise false.
	public bool RemoveSection(string section_name)
	{
		section_name = section_name.Trim();
		return (RemoveSection(GetSection(section_name)));
	}

	/// @brief Removes a section from the IniFile.
	///        This method attempts to remove an IniSection from the IniFile's collection of sections based on its name.
	///        If the section is successfully removed, it returns true. Otherwise, it handles exceptions and returns false.
	///
	/// @param section The IniSection object to be removed.
	/// @retval True if the section was successfully removed, otherwise false.
	public bool RemoveSection(IniSection section)
	{
		if (section != null)
		{
			try
			{
				_Sections.Remove(section.Name);
				return (true);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		return (false);
	}

	/// @brief Removes all sections from the IniFile.
	///        This function clears all sections stored within the IniFile instance.
	///
	/// @retval True if all sections were successfully removed and the section count is zero, otherwise false.
	public bool RemoveAllSections()
	{
		_Sections.Clear();
		return (_Sections.Count == 0);
	}

	/// @brief Retrieves an IniSection by its name.
	///        This method searches for a section with the specified name in the _Sections collection.
	///        The search is case-sensitive and trims any leading or trailing whitespace from the provided section name.
	///
	/// @param section_name The name of the section to retrieve.
	/// @retval A reference to the IniSection if found, otherwise null.
	public IniSection GetSection(string section_name)
	{
		int id;

		id = _Sections.IndexOf(section_name.Trim());

		if (id == -1)
		{
			return (null);
		}
		else
		{
			return ((IniSection)_Sections[id]);
		}

	}

	/// @brief Retrieves the value of a key from a specified section in an INI file.
	///        This method attempts to retrieve the value associated with a given key within a specified section.
	///        If the section does not exist, it is created. Similarly, if the key does not exist within the section,
	///        it is added. If the key's value is null, it defaults to an empty string.
	///
	/// @param section_name The name of the section in which to look for the key.
	/// @param key_name The name of the key whose value needs to be retrieved.
	/// @retval The value associated with the specified key, or an empty string if the key's value is null.
	public string GetKeyValue(string section_name, string key_name)
	{
		IniSection        section;
		IniSection.IniKey key;

		section = GetSection(section_name);

		if (section == null)
		{
			section = AddSection(section_name);
			key     = section.AddKey(key_name);
		}
		else
		{
			key = section.GetKey(key_name);

			if (key == null)
			{
				key = section.AddKey(key_name);
			}
			else
			{
				if (key.Value == null)
				{
					key.Value = "";
				}

				return (key.Value);
			}
		}

		return (string.Empty);
	}

	/// @brief Sets a key-value pair in the specified section of an INI file.
	///        This method adds or updates a key with a given value in a specified section.
	///        It optionally encloses the value in quotes based on the 'quotes' parameter.
	///
	/// @param section_name The name of the section where the key-value pair should be set.
	/// @param key_name The name of the key to set or update.
	/// @param value The value to assign to the key.
	/// @param quotes A boolean indicating whether the value should be enclosed in quotes (default is true).
	/// @retval True if the key-value pair was successfully set, false otherwise.
	public bool SetKeyValue(string section_name, string key_name, string value, bool quotes = true)
	{
		IniSection        section;
		IniSection.IniKey key;

		section = AddSection(section_name);

		if (section != null)
		{
			key = section.AddKey(key_name);

			if (key != null)
			{
				if (quotes)
				{
					key.Quotes = true;
				}

				key.Value = value;
				return (true);
			}
		}

		return (false);
	}

	/// @brief Renames a section in the IniFile.
	///        This function attempts to rename an existing section from old_section_name to new_section_name.
	///        If the section does not exist, it returns false. Otherwise, it delegates the renaming to the SetName method of the IniSection class.
	///
	/// @param old_section_name The current name of the section to be renamed.
	/// @param new_section_name The new name for the section.
	/// @retval true if the section was successfully renamed; otherwise, false.
	public bool RenameSection(string old_section_name, string new_section_name)
	{
		IniSection section;

		section = GetSection(old_section_name);

		if (section != null)
		{
			return (section.SetName(new_section_name));
		}

		return (false);
	}

	/// @brief Renames a key within a specified section of an INI file.
	///        This function attempts to rename a key in the given section. If the section and key exist, it renames the key to the new name provided.
	///
	/// @param section_name The name of the section containing the key to be renamed.
	/// @param old_key_name The current name of the key that needs to be renamed.
	/// @param new_key_name The new name for the key.
	/// @retval True if the key was successfully renamed, false otherwise (e.g., if the section or key does not exist).
	public bool RenameKey(string section_name, string old_key_name, string new_key_name)
	{
		IniSection        section;
		IniSection.IniKey key;

		section = GetSection(section_name);

		if (section != null)
		{
			key = section.GetKey(old_key_name);
			if (key != null)
			{
				return (key.SetName(new_key_name));
			}
		}

		return (false);
	}

	public string FileName
	{
		get
		{
			return (_File_Name);
		}
	}

	public class IniSection
	{
		private IniFile   _Ini_File;
		private string    _Section_Name;
		private ArrayList _Keys_Array;

		/// @brief Initializes a new instance of the IniSection class.
		///        This constructor initializes an IniSection object with a reference to its parent IniFile and a specified section name.
		///
		/// @param parent The IniFile that this section belongs to.
		/// @param section_name The name of the section being initialized.
		/// @retval None
		protected internal IniSection(IniFile parent, string section_name)
		{
			_Ini_File     = parent;
			_Section_Name = section_name;
			_Keys_Array   = new ArrayList();
		}

		/// @brief Compares the current IniSection object with another object for equality.
		///        This function checks if the provided object is an instance of a string and compares it to the internal section name.
		///
		/// @param obj The object to compare with the current IniSection object.
		/// @retval True if the object is a string and matches the internal section name; otherwise, false.
		public override bool Equals(Object obj)
		{
			return ((string)obj == _Section_Name);
		}

		/// @brief Generates a hash code for the IniSection object.
		///        This function computes a hash code based on the _Section_Name field of the IniSection class.
		///
		/// @retval An integer representing the hash code for this IniSection instance.
		public override int GetHashCode()
		{
			return (_Section_Name.GetHashCode());
		}

		/// @brief Get collection of keys
		///
		public System.Collections.ICollection Keys
		{
			get
			{
				return (_Keys_Array);
			}
		}

		/// @brief Get AyyaList of keys
		///
		public ArrayList KeysArray
		{
			get
			{
				return (_Keys_Array);
			}
		}

		/// @brief Get section name
		///
		public string Name
		{
			get
			{
				return (_Section_Name);
			}
		}

		/// @brief Adds a new key to the IniSection or retrieves an existing one.
		///        This method trims the provided key name and checks if it already exists in the section's keys array.
		///        If the key does not exist, it creates a new key and adds it to the array. Otherwise, it returns the existing key.
		///
		/// @param key_name The name of the key to add or retrieve.
		/// @retval A reference to the newly created IniKey object or an existing one if the key already exists.
		public IniKey AddKey(string key_name)
		{
			int               id;
			IniSection.IniKey key;

			key_name = key_name.Trim();
			key = null;

			if (key_name.Length != 0)
			{
				id = _Keys_Array.IndexOf(key_name);

				if (id == -1)
				{
					key = new IniSection.IniKey(this, key_name);
					_Keys_Array.Add(key);
				}
				else
				{
					key = (IniKey)_Keys_Array[id];
				}
			}

			return (key);
		}

		/// @brief Removes a key from the IniSection.
		///        This function removes a key specified by its name from the IniSection.
		///        It internally calls another overload of RemoveKey that takes a Key object.
		///
		/// @param key_name The name of the key to be removed.
		/// @retval True if the key was successfully removed, otherwise false.
		public bool RemoveKey(string key_name)
		{
			return (RemoveKey(GetKey(key_name)));
		}

		/// @brief Removes a key from the IniSection.
		///        This method attempts to remove an IniKey object from the internal keys collection based on its name.
		///        If the removal is successful, it returns true. Otherwise, it handles exceptions by logging the error message and returns false.
		///
		/// @param key The IniKey object to be removed.
		/// @retval True if the key was successfully removed, otherwise false.
		public bool RemoveKey(IniKey key)
		{
			if (key != null)
			{
				try
				{
					_Keys_Array.Remove(key.Name);
					return true;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}

			return (false);
		}

		/// @brief Removes all keys from the IniSection.
		///        This function clears all keys stored in the _Keys_Array of the IniSection class.
		///
		/// @retval A boolean value indicating whether the removal was successful (true if no keys remain, false otherwise).
		public bool RemoveAllKeys()
		{
			_Keys_Array.Clear();
			return (_Keys_Array.Count == 0);
		}

		/// @brief Retrieves an IniKey object by its name.
		///        This method searches for an IniKey with the specified name in the _Keys_Array list.
		///        If found, it returns the corresponding IniKey object; otherwise, it returns null.
		///
		/// @param key_name The name of the IniKey to retrieve.
		/// @retval The IniKey object if found, or null if no matching key is present.
		public IniKey GetKey(string key_name)
		{
			int id;

			id = _Keys_Array.IndexOf(key_name);

			if (id == -1)
			{
				return (null);
			}
			else
			{
				return ((IniKey)_Keys_Array[id]);
			}
		}

		/// @brief Sets the name of the INI section.
		///        This method sets a new name for the current INI section, ensuring that no other section in the INI file has the same name.
		///        It trims the provided section name and checks if it is not empty. If another section with the same name exists, it returns false.
		///        Otherwise, it updates the section name in the INI file's sections collection and returns true. Any exceptions during this process are logged.
		///
		/// @param section_name The new name to set for the INI section.
		/// @retval True if the section name was successfully updated; otherwise, false.
		public bool SetName(string section_name)
		{
			IniSection section;

			section_name = section_name.Trim();

			if (section_name.Length != 0)
			{
				section = _Ini_File.GetSection(section_name);

				if ((section != this) && (section != null))
				{
					return (false);
				}

				try
				{
					_Ini_File._Sections.Remove(_Section_Name);
					_Ini_File._Sections.Add(this);
					_Section_Name = section_name;

					return (true);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}

			return (false);
		}

		/// @brief Retrieves the name of the section.
		///        This function returns the name associated with the current instance of IniSection.
		///
		/// @retval The name of the section as a string.
		public string GetName()
		{
			return (_Section_Name);
		}

		public class IniKey
		{
			private string     Key_Name;
			private string     Key_Value;
			private string     Key_Comment;
			private bool       Key_Quote;
			private IniSection Section;

			/// @brief Initializes a new instance of the IniKey class.
			///        This constructor initializes an IniKey object with a specified parent section and key name.
			///
			/// @param parent The IniSection that this IniKey belongs to.
			/// @param key_name The name of the key being initialized.
			/// @retval None
			protected internal IniKey(IniSection parent, string key_name)
			{
				Section  = parent;
				Key_Name = key_name;
			}

			/// @brief Compares the current IniKey object with another object for equality.
			///        This function checks if the provided object, when cast to a string, matches the Key_Name of this IniKey instance.
			///
			/// @param obj The object to compare with the current IniKey instance.
			/// @retval True if the object is a string that matches the Key_Name; otherwise, false.
			public override bool Equals(Object obj)
			{
				return ((string)obj == Key_Name);
			}

			/// @brief Generates a hash code for the IniKey object.
			///        This function computes a hash code based on the Key_Name property of the IniKey class.
			///
			/// @retval An integer representing the hash code for the IniKey object.
			public override int GetHashCode()
			{
				return (Key_Name.GetHashCode());
			}

			/// @brief Get name of key
			///
			public string Name
			{
				get
				{
					return (Key_Name);
				}
			}

			/// @brief Get value of key
			///
			public string Value
			{
				get
				{
					return (Key_Value);
				}
				set
				{
					Key_Value = value;
				}
			}

			/// @brief Get comment of key
			///
			public string Comment
			{
				get
				{
					return (Key_Comment);
				}
				set
				{
					Key_Comment = value;
				}
			}

			/// @brief Get and Set if value is surrounded by quotes
			///
			public bool Quotes
			{
				get
				{
					return (Key_Quote);
				}
				set
				{
					Key_Quote = value;
				}
			}

			/// @brief Sets the value of a key.
			///        This function assigns a specified string value to the Key_Value property of the IniKey class.
			///
			/// @param value The string value to be assigned to the key.
			public void SetValue(string value)
			{
				Key_Value = value;
			}

			/// @brief Retrieves the value associated with a key.
			///        This function returns the value stored in the Key_Value property of the IniKey class.
			///
			/// @retval The value associated with the key as a string.
			public string GetValue()
			{
				return (Key_Value);
			}

			/// @brief Sets a comment for the key.
			///        This function assigns a provided string as the comment for the key.
			///
			/// @param comment The comment to be set for the key.
			public void SetComment(string comment)
			{
				Key_Comment = comment;
			}

			/// @brief Retrieves the comment associated with a key.
			///        This function returns the comment that is stored in the Key_Comment field of the IniKey class.
			///
			/// @retval The comment string associated with the key, or an empty string if no comment is set.
			public string GetComment()
			{
				return (Key_Comment);
			}

			/// @brief Sets the name of the IniKey.
			///        This method sets a new name for the IniKey instance after trimming any whitespace from the provided key_name.
			///        It checks if the trimmed key_name is not empty and whether another key with the same name already exists in the section.
			///        If no conflicts are found, it updates the key's name in the KeysArray of the Section and returns true.
			///        In case of an exception during the update process or if a conflict is detected, it logs the error message and returns false.
			///
			/// @param key_name The new name to set for the IniKey instance.
			/// @retval True if the name was successfully updated without conflicts; otherwise, false.
			public bool SetName(string key_name)
			{
				IniKey key;

				key_name = key_name.Trim();

				if (key_name.Length != 0)
				{
					key = Section.GetKey(key_name);

					if ((key != this) && (key != null))
					{
						return (false);
					}

					try
					{
						Section.KeysArray.Remove(Key_Name);
						Section.KeysArray.Add(this);

						Key_Name = key_name;

						return (true);
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
					}
				}

				return (false);
			}

			/// @brief Retrieves the name of the key.
			///        This function returns the name associated with the key.
			///
			/// @retval The name of the key as a string.
			public string GetName()
			{
				return (Key_Name);
			}
		}
	}
}

#pragma warning restore CS8618
#pragma warning restore CS8600
#pragma warning restore CS8603
#pragma warning restore CS8604
#pragma warning restore CS8765
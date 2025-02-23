namespace UnitTests;


public class ParserTest
{
	[Fact]
	public void TestCppConstructor1()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		ImportConfigDataCsvJob
		::
		ImportConfigDataCsvJob(const std::string &folder, const std::shared_ptr<FolderScanJob> &folder_scan_job, 
		const std::weak_ptr<INetworkConfigurationMap> &network_configuration_map) : 
			_Folder(folder), _Folder_Scan(folder_scan_job), _Network_Configuration_Map(network_configuration_map)
		/**************************************************************************/
		{
		";


		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("", return_type);
		Assert.Equal("ImportConfigDataCsvJob::ImportConfigDataCsvJob", method_name);
		Assert.Equal("(const std::string &folder, const std::shared_ptr<FolderScanJob> &folder_scan_job, const std::weak_ptr<INetworkConfigurationMap> &network_configuration_map)", parameters);
	}

	[Fact]
	public void TestCppConstructor2()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		ImportConfigDataCsvJob
		::
		ImportConfigDataCsvJob() : 
			_Folder(folder), _Folder_Scan(folder_scan_job), _Network_Configuration_Map(network_configuration_map)
		/**************************************************************************/
		{
		";


		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("", return_type);
		Assert.Equal("ImportConfigDataCsvJob::ImportConfigDataCsvJob", method_name);
		Assert.Equal("()", parameters);
	}

	[Fact]
	public void TestCppConstructor3()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		ImportConfigDataCsvJob
		::
		ImportConfigDataCsvJob()
		/**************************************************************************/
		{
		";


		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("", return_type);
		Assert.Equal("ImportConfigDataCsvJob::ImportConfigDataCsvJob", method_name);
		Assert.Equal("()", parameters);
	}

	[Fact]
	public void TestCppMethod1()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		void ImportConfigDataCsvJob
		::
		Test(const char *role, const std::array<const char *, N> &(*names)(void), 
			const std::array<bool, N> &
			(*enables)
			(void), 
					uint32_t 
					block_id)
		/**************************************************************************/
		{
		";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("void", return_type);
		Assert.Equal("ImportConfigDataCsvJob::Test", method_name);
		Assert.Equal("(const char *role, const std::array<const char *, N> &(*names)(void), const std::array<bool, N> &(*enables)(void), uint32_t block_id)", parameters);
	}

	[Fact]
	public void TestCppMethod2()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		void ImportConfigDataCsvJob
		::
		Test()
		/**************************************************************************/
		{
		";


		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("void", return_type);
		Assert.Equal("ImportConfigDataCsvJob::Test", method_name);
		Assert.Equal("()", parameters);
	}

	[Fact]
	public void TestCppMethod3()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		std::shared_ptr<A, 
		4, 
		std::array<int, 5>> 
		
		ImportConfigDataCsvJob
		::
		Test()
		/**************************************************************************/
		{
		";


		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("std::shared_ptr<A, 4, std::array<int, 5>>", return_type);
		Assert.Equal("ImportConfigDataCsvJob::Test", method_name);
		Assert.Equal("()", parameters);
	}

	[Fact]
	public void TestHppConstructor1()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		ImportConfigDataCsvJob(const std::string &folder, const std::shared_ptr<FolderScanJob> &folder_scan_job, 
		const std::weak_ptr<INetworkConfigurationMap> &network_configuration_map) : 
			_Folder(folder), _Folder_Scan(folder_scan_job), _Network_Configuration_Map(network_configuration_map)
		/**************************************************************************/
		{
		";


		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("", return_type);
		Assert.Equal("ImportConfigDataCsvJob", method_name);
		Assert.Equal("(const std::string &folder, const std::shared_ptr<FolderScanJob> &folder_scan_job, const std::weak_ptr<INetworkConfigurationMap> &network_configuration_map)", parameters);
	}

	[Fact]
	public void TestHppConstructor2()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		
		ImportConfigDataCsvJob<T<A<B>>, 4>() : 
			_Folder(folder), _Folder_Scan(folder_scan_job), _Network_Configuration_Map(network_configuration_map)
		/**************************************************************************/
		{
		";


		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("", return_type);
		Assert.Equal("ImportConfigDataCsvJob<T<A<B>>, 4>", method_name);
		Assert.Equal("()", parameters);
	}

	[Fact]
	public void TestHppConstructor3()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 

		ImportConfigDataCsvJob()
		/**************************************************************************/
		{
		";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("", return_type);
		Assert.Equal("ImportConfigDataCsvJob", method_name);
		Assert.Equal("()", parameters);
	}

	[Fact]
	public void TestHppMethod1()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		void 
		Test(const char *role, const std::array<const char *, N> &(*names)(void), 
			const std::array<bool, N> &
			(*enables)
			(void), 
					uint32_t 
					block_id)
		/**************************************************************************/
		{
		";


		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("void", return_type);
		Assert.Equal("Test", method_name);
		Assert.Equal("(const char *role, const std::array<const char *, N> &(*names)(void), const std::array<bool, N> &(*enables)(void), uint32_t block_id)", parameters);
	}

	[Fact]
	public void TestHppMethod2()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		void 
		Test()
		/**************************************************************************/
		{
		";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("void", return_type);
		Assert.Equal("Test", method_name);
		Assert.Equal("()", parameters);
	}

	[Fact]
	public void TestHppMethod3()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		std::shared_ptr<A, 
		4, 
		std::array<int, 5>> 
		
		
		Test()
		/**************************************************************************/
		{
		";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("std::shared_ptr<A, 4, std::array<int, 5>>", return_type);
		Assert.Equal("Test", method_name);
		Assert.Equal("()", parameters);
	}

	[Fact]
	public void TestHppMethod4()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code = @"
				explicit LoggerModule(const char *module_name)	: _Default_Level((T)0)
				{  
				";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("explicit", modifiers);
		Assert.Equal("", return_type);
		Assert.Equal("LoggerModule", method_name);
		Assert.Equal("(const char *module_name)", parameters);
	}

	[Fact]
	public void TestHppMethod5()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code = @"
					/*****************************************************************************/
					DigitalInOut &DigitalInOut::operator=(bool value)
					/*****************************************************************************/
					{
				";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("DigitalInOut", return_type);
		Assert.Equal("&DigitalInOut::operator=", method_name);
		Assert.Equal("(bool value)", parameters);
	}

	[Fact]
	public void TestCFunction1()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		template<T<A>, 
		B<C<D>>> 
		std::shared_ptr<A, 
		4, 
		std::array<int, 5>> 
		
		Test(const char *role, const std::array<const char *, N> &(*names)(void), 
			const std::array<bool, N> &
			(*enables)
			(void), 
					uint32_t 
					block_id)
		/**************************************************************************/
		{
		";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("std::shared_ptr<A, 4, std::array<int, 5>>", return_type);
		Assert.Equal("Test", method_name);
		Assert.Equal("(const char *role, const std::array<const char *, N> &(*names)(void), const std::array<bool, N> &(*enables)(void), uint32_t block_id)", parameters);
	}

	[Fact]
	public void TestCFunction2()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		int (*test(int a)) (int v)
		/**************************************************************************/
		{
		";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("", modifiers);
		Assert.Equal("int", return_type);
		Assert.Equal("(*test(int a))", method_name);
		Assert.Equal("(int v)", parameters);
	}

	[Fact]
	public void TestCFunction3()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		static const std::shared_ptr<int, 3> (*test(int a)) (int v)
		/**************************************************************************/
		{
		";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("static", modifiers);
		Assert.Equal("const std::shared_ptr<int, 3>", return_type);
		Assert.Equal("(*test(int a))", method_name);
		Assert.Equal("(int v)", parameters);
	}

	[Fact]
	public void TestCSMethod1()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		public static const SharedObj<int<a>, 3> Test<int<a>> (int v)
		/**************************************************************************/
		{
		";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("public static", modifiers);
		Assert.Equal("const SharedObj<int<a>, 3>", return_type);
		Assert.Equal("Test<int<a>>", method_name);
		Assert.Equal("(int v)", parameters);
	}

	[Fact]
	public void TestCSMethod2()
	{
		string modifiers;
		string return_type;
		string method_name;
		string parameters;

		const string code =
		    @"
		/**************************************************************************/
		public static (int start, int end) FindFunctionBody(string input, Match input_match)
		/**************************************************************************/
		{
		";

		Parser.Do(code, out modifiers, out return_type, out method_name, out parameters);

		Assert.Equal("public static", modifiers);
		Assert.Equal("(int start, int end)", return_type);
		Assert.Equal("FindFunctionBody", method_name);
		Assert.Equal("(string input, Match input_match)", parameters);
	}

}
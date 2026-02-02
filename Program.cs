using System;
using System.IO;
using static RdpOutput.CRT;

namespace RdpOutput
{
	internal class Program
	{
		internal static string sourceFileName; // current source file name

		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				args = new string[] { "Tests\\test.txt" };
			}
			sourceFileName = args[0];

			int rdp_textsize = 350000; // size of scanner text array
			int rdp_tabwidth = 4; // tab expansion width

			Compiler compiler = new Compiler(rdp_textsize, 25, 100, rdp_tabwidth, false, false, false, false);

			using (StreamReader streamReader = new StreamReader(sourceFileName))
			{
				if (!compiler.Open(streamReader, sourceFileName))
					throw new Exception("unable to open source file");
				compiler.Compile();
				string messages = compiler.GetMessages();
				Console.Write(messages);
			}
		}
	}
}

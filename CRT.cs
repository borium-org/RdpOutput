using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdpOutput
{
	internal class CRT
	{
		internal static long CurrentTimeMillis()
		{
			DateTime nowUtc = DateTime.UtcNow;

			// Define the Unix Epoch
			DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			// Calculate the TimeSpan between the DateTime and the Unix Epoch
			TimeSpan timeSinceEpoch = nowUtc - unixEpoch;

			// Get the total milliseconds
			long milliseconds = (long)timeSinceEpoch.TotalMilliseconds;

			return milliseconds;
		}

		internal const int EOF = -1;

		internal static string capitalizeFirst(string text)
		{
			return char.ToUpper(text[0]) + text.Substring(1);
		}

		internal static void fclose(TextReader file)
		{
			try
			{
				file.Close();
			}
			catch (IOException)
			{
			}
		}

		internal static bool feof(TextReader file)
		{
			try
			{
				return file.Peek() == EOF;
			}
			catch (IOException)
			{
			}
			return true;
		}

		internal static int getc(TextReader file)
		{
			try
			{
				return file.Read();
			}
			catch (IOException)
			{
			}
			return EOF;
		}

		internal static bool isalnum(int ch)
		{
			return isalpha(ch) || isdigit(ch);
		}

		internal static bool isalpha(int ch)
		{
			return "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".IndexOf((char)ch) >= 0;
		}

		internal static bool isdigit(int ch)
		{
			return "0123456789".IndexOf((char)ch) >= 0;
		}

		internal static bool isgraph(char ch)
		{
			return ch > ' ';
		}

		internal static bool isprint(int ch)
		{
			return ch >= ' ';
		}

		internal static bool isspace(int ch)
		{
			return ch <= ' ';
		}

		internal static bool isxdigit(int ch)
		{
			return "0123456789ABCDEFabcdef".IndexOf((char)ch) >= 0;
		}

		internal static int strcmp(string str1, string str2)
		{
			return string.Compare(str1, str2, StringComparison.Ordinal);
		}

		internal static long strtol(string nptr, Pointer<string> endptr, int intBase)
		{
			if (endptr != null)
			{
				Console.WriteLine("strtol: endptr is not implemented");
			}
			long result = Convert.ToInt64(nptr, intBase);
			return result;
		}
	}
}

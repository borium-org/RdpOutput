using System;
using System.Diagnostics;
using System.IO;
using static RdpOutput.CRT;
using static RdpOutput.Scanner;
using static RdpOutput.Text;
using static RdpOutput.Text.TextMessageType;

namespace RdpOutput
{
	internal class Program
	{
		internal class Compiler
		{
			const int RDP_T_34 /* " */ = SCAN_P_TOP + 0;
			const int RDP_T_39 /* ' */ = SCAN_P_TOP + 1;
			const int RDP_T_46 /* . */ = SCAN_P_TOP + 2;
			const int RDP_T_4742 /* / * */ = SCAN_P_TOP + 3;
			const int RDP_T_4747 /* // */ = SCAN_P_TOP + 4;
			const int RDP_T_59 /* ; */ = SCAN_P_TOP + 5;
			const int RDP_T_Entity = SCAN_P_TOP + 6;

			private Text text;
			private Scanner scan;

			internal Compiler(int max_text, int max_errors, int max_warnings, int tab_width,
				bool case_insensitive, bool newline_visible, bool show_skips, bool symbol_echo,
				string[] token_names)
			{
				text = new Text(max_text, max_errors, max_warnings, tab_width);
				scan = new Scanner(case_insensitive, newline_visible, show_skips, symbol_echo, token_names, text);

				LoadKeywords();
			}

			private void EntryMessage()
			{
				StackTrace stackTrace = new StackTrace();
				string callerName = stackTrace.GetFrame(1).GetMethod().Name;
				text.Message(TEXT_INFO, $"Entered '{callerName}'\n");
			}

			private void ExitMessage()
			{
				StackTrace stackTrace = new StackTrace();
				string callerName = stackTrace.GetFrame(1).GetMethod().Name;
				text.Message(TEXT_INFO, $"Exited  '{callerName}'\n");
			}

			internal void CompilationUnit()
			{
				EntryMessage();

				{
					if (scan.Test("rdp_CompilationUnit_1", SCAN_P_ID, null))
					{
						while (true)
						{
							{
								UsingDirective();
							}
							if (!scan.Test("rdp_CompilationUnit_1", SCAN_P_ID, null))
								break;
						}
					}
					if (scan.Test("rdp_CompilationUnit_3", RDP_T_Entity, null))
					{
						while (true)
						{
							{
								EntityDeclaration();
							}
							if (!scan.Test("rdp_CompilationUnit_3", RDP_T_Entity, null))
								break;
						}
					}
					scan.Test("CompilationUnit", CompilationUnit_stop, CompilationUnit_stop);
				}

				ExitMessage();
			}

			private void EntityDeclaration()
			{
				EntryMessage();

				{
					scan.Test("EntityDeclaration", RDP_T_Entity, EntityDeclaration_stop);
					scan.Scan();
					scan.Test("EntityDeclaration", EntityDeclaration_stop, EntityDeclaration_stop);
				}

				ExitMessage();
			}

			private void Identifier()
			{
				EntryMessage();

				{
					scan.Test("Identifier", SCAN_P_ID, Identifier_stop);
					scan.Scan();
					scan.Test("Identifier", Identifier_stop, Identifier_stop);
				}

				ExitMessage();
			}

			private void UsingDirective()
			{
				EntryMessage();

				{
					Identifier();
					if (scan.Test("rdp_UsingDirective_1", RDP_T_46 /* . */, null))
					{
						while (true)
						{
							{
								scan.Test("UsingDirective", RDP_T_46 /* . */, UsingDirective_stop);
								scan.Scan();
								Identifier();
							}
							if (!scan.Test("rdp_UsingDirective_1", RDP_T_46 /* . */, null))
								break;
						}
					}
					scan.Test("UsingDirective", RDP_T_59 /* ; */, UsingDirective_stop);
					scan.Scan();
					scan.Test("UsingDirective", UsingDirective_stop, UsingDirective_stop);
				}

				ExitMessage();
			}

			private void LoadKeywords()
			{
				scan.LoadKeyword("\"", "\\", RDP_T_34 /* " */, SCAN_P_STRING_ESC);
				scan.LoadKeyword("\'", "\\", RDP_T_39 /* ' */, SCAN_P_STRING_ESC);
				scan.LoadKeyword(".", null, RDP_T_46 /* . */, SCAN_P_IGNORE);
				scan.LoadKeyword("/*", "*/", RDP_T_4742 /* / * */, SCAN_P_COMMENT);
				scan.LoadKeyword("//", null, RDP_T_4747 /* // */, SCAN_P_COMMENT_LINE);
				scan.LoadKeyword(";", null, RDP_T_59 /* ; */, SCAN_P_IGNORE);
				scan.LoadKeyword("Entity", null, RDP_T_Entity, SCAN_P_IGNORE);
			}

			private static readonly Set Char_stop = new Set(SCAN_P_EOF);
			private static readonly Set Comment_first = new Set(RDP_T_4742 /* / * */, RDP_T_4747 /* // */);
			private static readonly Set Comment_stop = new Set(SCAN_P_EOF);
			private static readonly Set CompilationUnit_first = new Set(SCAN_P_ID, RDP_T_Entity);
			private static readonly Set CompilationUnit_stop = new Set(SCAN_P_EOF);
			private static readonly Set EntityDeclaration_stop = new Set(SCAN_P_EOF, RDP_T_Entity);
			private static readonly Set Identifier_stop = new Set(SCAN_P_EOF, RDP_T_46 /* . */, RDP_T_59 /* ; */);
			private static readonly Set String_stop = new Set(SCAN_P_EOF);
			private static readonly Set UsingDirective_stop = new Set(SCAN_P_ID, SCAN_P_EOF, RDP_T_Entity);
			private static readonly Set rdp_CompilationUnit_4_first = new Set(SCAN_P_ID, RDP_T_Entity);

			internal bool Open(StreamReader streamReader, string sourceFileName)
			{
				if (text.Open(streamReader, sourceFileName) == null)
					return false;
				text.GetChar();
				scan.Scan();
				return true;
			}

			internal void Compile()
			{
				CompilationUnit();
				if (text.TotalErrors() != 0)
					text.Message(TEXT_FATAL, $"{text.TotalErrors()} error{(text.TotalErrors() == 1 ? "" : "s")} detected in source file {sourceFileName}\n");
			}
		}

		internal static string sourceFileName; // current source file name

		private static string[] rdp_tokens = { "IGNORE", "ID", "INTEGER", "REAL", "CHAR", "CHAR_ESC", "STRING",
			"STRING_ESC", "COMMENT", "COMMENT_VISIBLE", "COMMENT_NEST", "COMMENT_NEST_VISIBLE", "COMMENT_LINE",
			"COMMENT_LINE_VISIBLE", "EOF", "EOLN", "'\"'", "'\''", "'.'", "'/*'", "'//'", "';'", "'Entity'" };

		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				args = new string[] { "Tests\\test.txt" };
			}
			sourceFileName = args[0];

			long rdp_start_time = CurrentTimeMillis();
			int rdp_textsize = 350000; // size of scanner text array
			int rdp_tabwidth = 4; // tab expansion width

			Compiler compiler = new Compiler(rdp_textsize, 25, 100, rdp_tabwidth, false, false, false, false, rdp_tokens);

			using (StreamReader streamReader = new StreamReader(sourceFileName))
			{
				if (!compiler.Open(streamReader, sourceFileName))
					throw new Exception("unable to open source file");
				compiler.Compile();
			}
		}
	}
}

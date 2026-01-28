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

			internal Compiler(int max_text, int max_errors, int max_warnings, int tab_width,
				bool case_insensitive, bool newline_visible, bool show_skips, bool symbol_echo,
				string[] token_names)
			{
				text_init(max_text, max_errors, max_warnings, tab_width);
				scan_init(case_insensitive, newline_visible, show_skips, symbol_echo, token_names);

				SetInitialise();
				LoadKeywords();
			}

			internal void CompilationUnit()
			{
				text_message(TEXT_INFO, "Entered 'CompilationUnit'\n");

				{
					if (scan_test("rdp_CompilationUnit_1", SCAN_P_ID, null))
					{
						while (true)
						{
							{
								UsingDirective();
							}
							if (!scan_test("rdp_CompilationUnit_1", SCAN_P_ID, null)) break;
						}
					}
					if (scan_test("rdp_CompilationUnit_3", RDP_T_Entity, null))
					{
						while (true)
						{
							{
								EntityDeclaration();
							}
							if (!scan_test("rdp_CompilationUnit_3", RDP_T_Entity, null)) break;
						}
					}
					scan_test_set("CompilationUnit", CompilationUnit_stop, CompilationUnit_stop);
				}
				text_message(TEXT_INFO, "Exited  'CompilationUnit'\n");
			}

			private void EntityDeclaration()
			{
				text_message(TEXT_INFO, "Entered 'EntityDeclaration'\n");

				{
					scan_test("EntityDeclaration", RDP_T_Entity, EntityDeclaration_stop);
					scan_();
					scan_test_set("EntityDeclaration", EntityDeclaration_stop, EntityDeclaration_stop);
				}
				text_message(TEXT_INFO, "Exited  'EntityDeclaration'\n");
			}

			private void Identifier()
			{
				text_message(TEXT_INFO, "Entered 'Identifier'\n");

				{
					scan_test("Identifier", SCAN_P_ID, Identifier_stop);
					scan_();
					scan_test_set("Identifier", Identifier_stop, Identifier_stop);
				}
				text_message(TEXT_INFO, "Exited  'Identifier'\n");
			}

			private void UsingDirective()
			{
				text_message(TEXT_INFO, "Entered 'UsingDirective'\n");

				{
					Identifier();
					if (scan_test("rdp_UsingDirective_1", RDP_T_46 /* . */, null))
					{
						while (true)
						{
							{
								scan_test("UsingDirective", RDP_T_46 /* . */, UsingDirective_stop);
								scan_();
								Identifier();
							}
							if (!scan_test("rdp_UsingDirective_1", RDP_T_46 /* . */, null)) break;
						}
					}
					scan_test("UsingDirective", RDP_T_59 /* ; */, UsingDirective_stop);
					scan_();
					scan_test_set("UsingDirective", UsingDirective_stop, UsingDirective_stop);
				}
				text_message(TEXT_INFO, "Exited  'UsingDirective'\n");
			}

			private void LoadKeywords()
			{
				scan_load_keyword("\"", "\\", RDP_T_34 /* " */, SCAN_P_STRING_ESC);
				scan_load_keyword("\'", "\\", RDP_T_39 /* ' */, SCAN_P_STRING_ESC);
				scan_load_keyword(".", null, RDP_T_46 /* . */, SCAN_P_IGNORE);
				scan_load_keyword("/*", "*/", RDP_T_4742 /* / * */, SCAN_P_COMMENT);
				scan_load_keyword("//", null, RDP_T_4747 /* // */, SCAN_P_COMMENT_LINE);
				scan_load_keyword(";", null, RDP_T_59 /* ; */, SCAN_P_IGNORE);
				scan_load_keyword("Entity", null, RDP_T_Entity, SCAN_P_IGNORE);
			}

			private static readonly Set Char_stop = new Set();
			private static readonly Set Comment_first = new Set();
			private static readonly Set Comment_stop = new Set();
			private static readonly Set CompilationUnit_first = new Set();
			private static readonly Set CompilationUnit_stop = new Set();
			private static readonly Set EntityDeclaration_stop = new Set();
			private static readonly Set Identifier_stop = new Set();
			private static readonly Set String_stop = new Set();
			private static readonly Set UsingDirective_stop = new Set();
			private static readonly Set rdp_CompilationUnit_4_first = new Set();

			private void SetInitialise()
			{
				Char_stop.assignList(SCAN_P_EOF);
				Comment_first.assignList(RDP_T_4742 /* / * */, RDP_T_4747 /* // */);
				Comment_stop.assignList(SCAN_P_EOF);
				CompilationUnit_first.assignList(SCAN_P_ID, RDP_T_Entity);
				CompilationUnit_stop.assignList(SCAN_P_EOF);
				EntityDeclaration_stop.assignList(SCAN_P_EOF, RDP_T_Entity);
				Identifier_stop.assignList(SCAN_P_EOF, RDP_T_46 /* . */, RDP_T_59 /* ; */);
				String_stop.assignList(SCAN_P_EOF);
				UsingDirective_stop.assignList(SCAN_P_ID, SCAN_P_EOF, RDP_T_Entity);
				rdp_CompilationUnit_4_first.assignList(SCAN_P_ID, RDP_T_Entity);
			}
		}

		internal static string rdp_sourcefilename; // current source file name

		private static string[] rdp_tokens = { "IGNORE", "ID", "INTEGER", "REAL", "CHAR", "CHAR_ESC", "STRING",
			"STRING_ESC", "COMMENT", "COMMENT_VISIBLE", "COMMENT_NEST", "COMMENT_NEST_VISIBLE", "COMMENT_LINE",
			"COMMENT_LINE_VISIBLE", "EOF", "EOLN", "'\"'", "'\''", "'.'", "'/*'", "'//'", "';'", "'Entity'" };

		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				args = new string[] { "Tests\\test.txt" };
			}
			rdp_sourcefilename = args[0];

			long rdp_start_time = CurrentTimeMillis();
			int rdp_textsize = 350000; // size of scanner text array
			int rdp_tabwidth = 4; // tab expansion width

			Compiler compiler = new Compiler(rdp_textsize, 25, 100, rdp_tabwidth, false, false, false, false, rdp_tokens);

			if (text_open(rdp_sourcefilename) == null)
				throw new System.Exception("unable to open source file");

			text_get_char();
			scan_();

			compiler.CompilationUnit();

			if (text_total_errors() != 0)
				text_message(TEXT_FATAL, $"{text_total_errors()} error{(text_total_errors() == 1 ? "" : "s")} detected in source file {rdp_sourcefilename}\n");
		}
	}
}

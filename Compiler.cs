using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static RdpOutput.Scanner;
using static RdpOutput.Text.TextMessageType;

namespace RdpOutput
{
	internal partial class Compiler
	{
		const int RDP_T_34 /* " */ = SCAN_P_TOP + 0;
		const int RDP_T_39 /* ' */ = SCAN_P_TOP + 1;
		const int RDP_T_46 /* . */ = SCAN_P_TOP + 2;
		const int RDP_T_4742 /* / * */ = SCAN_P_TOP + 3;
		const int RDP_T_4747 /* // */ = SCAN_P_TOP + 4;
		const int RDP_T_59 /* ; */ = SCAN_P_TOP + 5;
		const int RDP_T_Entity = SCAN_P_TOP + 6;

		private static readonly string[] token_names = { "IGNORE", "ID", "INTEGER", "REAL", "CHAR", "CHAR_ESC",
			"STRING", "STRING_ESC", "COMMENT", "COMMENT_VISIBLE", "COMMENT_NEST", "COMMENT_NEST_VISIBLE",
			"COMMENT_LINE", "COMMENT_LINE_VISIBLE", "EOF", "EOLN", "'\"'", "'\''", "'.'", "'/*'", "'//'",
			"';'", "'Entity'" };

		internal AstCompilationUnit RuleCompilationUnit()
		{
			AstCompilationUnit ast = new AstCompilationUnit();

			EntryMessage();

			{
				if (scan.Test("rdp_CompilationUnit_1", SCAN_P_ID, null))
				{
					while (true)
					{
						{
							ast.Add(RuleUsingDirective());
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
							ast.Add(RuleEntityDeclaration());
						}
						if (!scan.Test("rdp_CompilationUnit_3", RDP_T_Entity, null))
							break;
					}
				}
				scan.Test("CompilationUnit", CompilationUnit_stop, CompilationUnit_stop);
			}

			ExitMessage();

			return ast;
		}

		private AstEntityDeclaration RuleEntityDeclaration()
		{
			AstEntityDeclaration ast = new AstEntityDeclaration();

			EntryMessage();

			{
				scan.Test("EntityDeclaration", RDP_T_Entity, EntityDeclaration_stop);
				ast.Add(scan.Scan());
				scan.Test("EntityDeclaration", EntityDeclaration_stop, EntityDeclaration_stop);
			}

			ExitMessage();

			return ast;
		}

		private AstIdentifier RuleIdentifier()
		{
			AstIdentifier ast = new AstIdentifier();

			EntryMessage();

			{
				scan.Test("Identifier", SCAN_P_ID, Identifier_stop);
				ast.Add(scan.Scan());
				scan.Test("Identifier", Identifier_stop, Identifier_stop);
			}

			ExitMessage();

			return ast;
		}

		private AstUsingDirective RuleUsingDirective()
		{
			AstUsingDirective ast = new AstUsingDirective();

			EntryMessage();

			{
				ast.Add(RuleIdentifier());
				if (scan.Test("rdp_UsingDirective_1", RDP_T_46 /* . */, null))
				{
					while (true)
					{
						{
							scan.Test("UsingDirective", RDP_T_46 /* . */, UsingDirective_stop);
							ast.Add(scan.Scan());
							ast.Add(RuleIdentifier());
						}
						if (!scan.Test("rdp_UsingDirective_1", RDP_T_46 /* . */, null))
							break;
					}
				}
				scan.Test("UsingDirective", RDP_T_59 /* ; */, UsingDirective_stop);
				ast.Add(scan.Scan());
				scan.Test("UsingDirective", UsingDirective_stop, UsingDirective_stop);
			}

			ExitMessage();

			return ast;
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

		internal void Compile()
		{
			try
			{
				AstCompilationUnit astCompilationUnit = RuleCompilationUnit();
				if (text.GetTotalErrors() != 0)
				{
					text.Message(TEXT_FATAL, $"{text.GetTotalErrors()} error{(text.GetTotalErrors() == 1 ? "" : "s")} detected in source file {sourceFileName}\n");
				}
				else
				{
					astCompilationUnit.DebugPrint();
				}
			}
			catch (Exception)
			{
			}
		}
	}

	internal partial class AstCompilationUnit : Ast
	{
		protected override void PrintToken()
		{
			Debug.Write("CompilationUnit");
		}
	}

	internal partial class AstEntityDeclaration : Ast
	{
		protected override void PrintToken()
		{
			Debug.Write("EntityDeclaration");
		}
	}

	internal partial class AstIdentifier : Ast
	{
		protected override void PrintToken()
		{
			Debug.Write("Identifier");
		}
	}

	internal partial class AstUsingDirective : Ast
	{
		protected override void PrintToken()
		{
			Debug.Write("UsingDirective");
		}
	}
}

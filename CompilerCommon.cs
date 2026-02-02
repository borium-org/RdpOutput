using System.Diagnostics;
using System.IO;
using static RdpOutput.Text.TextMessageType;

namespace RdpOutput
{
	internal partial class Compiler
	{
		private Text text;
		private Scanner scan;

		private string sourceFileName;

		private readonly bool enableRuleMessages = true;

		internal Compiler(int max_text, int max_errors, int max_warnings, int tab_width,
			bool case_insensitive, bool newline_visible, bool show_skips, bool symbol_echo)
		{
			text = new Text(max_text, max_errors, max_warnings, tab_width);
			scan = new Scanner(case_insensitive, newline_visible, show_skips, symbol_echo, token_names, text);

			LoadKeywords();
		}

		private void EntryMessage()
		{
			if (enableRuleMessages)
			{
				StackTrace stackTrace = new StackTrace();
				string callerName = stackTrace.GetFrame(1).GetMethod().Name;
				text.Message(TEXT_INFO, $"Entered '{callerName}'\n");
			}
		}

		private void ExitMessage()
		{
			if (enableRuleMessages)
			{
				StackTrace stackTrace = new StackTrace();
				string callerName = stackTrace.GetFrame(1).GetMethod().Name;
				text.Message(TEXT_INFO, $"Exited  '{callerName}'\n");
			}
		}

		internal bool Open(StreamReader streamReader, string sourceFileName)
		{
			this.sourceFileName = sourceFileName;
			if (text.Open(streamReader, sourceFileName) == null)
				return false;
			text.GetChar();
			scan.Scan();
			return true;
		}

		internal string GetMessages()
		{
			return text.GetMessages();
		}
	}
}

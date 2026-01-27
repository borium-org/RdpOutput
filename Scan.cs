using System;
using static RdpOutput.CRT;
using static RdpOutput.Program;
using static RdpOutput.Set;
using static RdpOutput.Symbol;
using static RdpOutput.Text;
using static RdpOutput.Text.TextMessageType;

namespace RdpOutput
{
	internal class Scan
	{
		internal class ScanData : Symbol
		{
			internal int token;
			internal int extended;
			internal ScanCommentBlock comment_block;
			internal string sourcefilename;
			internal int line_number;
			internal int u;
			internal int i;
			internal double r;
			internal object p;
		}

		internal class ScanCommentBlock
		{
			internal string comment;
			internal int column;
			internal int sequence_number;
			internal ScanCommentBlock next;
			internal ScanCommentBlock previous;
		}

		internal const int SCAN_P_IGNORE = 0;
		internal const int SCAN_P_ID = 1;
		internal const int SCAN_P_INTEGER = 2;
		internal const int SCAN_P_REAL = 3;
		internal const int SCAN_P_CHAR = 4;
		internal const int SCAN_P_CHAR_ESC = 5;
		internal const int SCAN_P_STRING = 6;
		internal const int SCAN_P_STRING_ESC = 7;
		internal const int SCAN_P_COMMENT = 8;
		internal const int SCAN_P_COMMENT_VISIBLE = 9;
		internal const int SCAN_P_COMMENT_NEST = 10;
		internal const int SCAN_P_COMMENT_NEST_VISIBLE = 11;
		internal const int SCAN_P_COMMENT_LINE = 12;
		internal const int SCAN_P_COMMENT_LINE_VISIBLE = 13;
		internal const int SCAN_P_EOF = 14;
		internal const int SCAN_P_EOLN = 15;
		internal const int SCAN_P_TOP = 16;

		private static bool scan_case_insensitive = false;
		private static bool scan_show_skips = false;
		private static bool scan_newline_visible = false;
		private static bool scan_symbol_echo = false;
		private static string[] scan_token_names = null;
		private static ScanCommentBlock scan_comment_list = null;
		private static ScanCommentBlock scan_comment_list_end = null;
		private static ScanCommentBlock last_comment_block;

		private static SymbolTable scan_table;

		private static bool scan_lexicalise_flag = false;
		private static int last_line_number = 0;
		private static int last_column = 0;
		private static bool retain_comments = false;
		private static int scan_sequence_running_number = 0;

		internal static void memcpy(ScanData to, ScanData from)
		{
			to.next_hash = from.next_hash;
			to.last_hash.set(from.last_hash.value());
			to.next_scope = from.next_scope;
			to.scope = from.scope;
			to.hash = from.hash;
			to.id = from.id;
			to.token = from.token;
			to.extended = from.extended;
			to.comment_block = from.comment_block;
			to.sourcefilename = from.sourcefilename;
			to.line_number = from.line_number;
			to.u = from.u;
			to.i = from.i;
			to.r = from.r;
			to.p = from.p;
		}

		internal static void memset(ScanData to)
		{
			to.next_hash = null;
			to.last_hash.set(null);
			to.next_scope = null;
			to.scope = null;
			to.hash = 0;
			to.id = 0;
			to.token = 0;
			to.extended = 0;
			to.comment_block = null;
			to.sourcefilename = null;
			to.line_number = 0;
			to.u = 0;
			to.i = 0;
			to.r = 0;
			to.p = null;
		}

		internal static void scan_()
		{
			int start;
			ScanData s;
			bool nestable = false;
			int nestlevel = 0;
			int close;
			int last = ' ';
			do
			{
				start = text_top;
				memset(text_scan_data);
				// Don't do extendeds for non scanner table items
				text_scan_data.extended = SCAN_P_IGNORE;
				while (text_char != EOF && !(scan_newline_visible && text_char == '\n') && isspace(text_char))
				{
					if (scan_lexicalise_flag && text_char == '\n')
					{
						text_printf("\n");
					}
					text_get_char();
				}
				// Non zero means a token was restored at EOF
				if (text_scan_data.token != 0)
					break;

				last_column = text_column_number();
				last_line_number = text_line_number();
				if (isalpha(text_char) || text_char == '_')
				{
					/* read an identifier into text buffer */
					int first_char = text_char;
					text_scan_data.id = text_top; /* point to text table */
					if (scan_case_insensitive && text_char >= 'A' && text_char <= 'Z')
					{
						text_char -= 'A' - 'a';
					}
					text_insert_char((char)text_char);
					text_get_char();
					while (isalnum(text_char) || text_char == '_')
					{
						if (scan_case_insensitive && text_char >= 'A' && text_char <= 'Z')
						{
							text_char -= 'A' - 'a';
						}
						text_insert_char((char)text_char);
						text_get_char();
					}
					text_insert_char('\0');
					if ((s = (ScanData)symbol_lookup_key(scan_table, text_get_string(text_scan_data.id), null)) != null)
					{
						memcpy(text_scan_data, s);
						text_top = start;
					}
					else
					{
						text_scan_data.token = SCAN_P_ID;
					}
				} /* end of ID collection */
				else if (isdigit(text_char))
				{
					// read a number of some sort
					bool hex = false;
					// remember start position
					text_scan_data.id = text_top;
					// assume integer
					text_scan_data.token = SCAN_P_INTEGER;
					// Check for hexadecimal introducer
					if (text_char == '0')
					{
						text_insert_char((char)text_char);
						text_get_char();
						if (text_char == 'x' || text_char == 'X')
						{
							hex = true;
							text_insert_char((char)text_char);
							text_get_char();
						}
					}
					// Now collect decimal or hex digits
					while ((hex ? isxdigit(text_char) : isdigit(text_char)) || text_char == '_')
					{
						// suppress underscores
						if (text_char != '_')
						{
							text_insert_char((char)text_char);
						}
						text_get_char();
					}
					// check for decimal part and exponent
					if (!hex)
					{
						// get decimal with lookahead
						if (text_char == '.' && isdigit(text_bot[text_current - 1]))
						{
							text_scan_data.token = SCAN_P_REAL;
							do
							{
								text_insert_char((char)text_char);
								text_get_char();
							} while (isdigit(text_char));
						}
						// get exponent
						if (text_char == 'E' || text_char == 'e')
						{
							text_scan_data.token = SCAN_P_REAL;
							text_insert_char((char)text_char);
							text_get_char();
							if (text_char == '+' || text_char == '-' || isdigit(text_char))
							{
								do
								{
									text_insert_char((char)text_char);
									text_get_char();
								} while (isdigit(text_char));
							}
						}
					}
					// Now absorb any letters that are attached to the number
					while (isalpha(text_char))
					{
						text_insert_char((char)text_char);
						text_get_char();
					}
					text_insert_char('\0');
					if (text_scan_data.token == SCAN_P_INTEGER)
					{
						text_scan_data.i = Convert.ToInt32(text_get_string(text_scan_data.id));
					}
					else
					{
						text_scan_data.r = Convert.ToDouble(text_get_string(text_scan_data.id));
					}
				} /* end of number collection */
				else
				{
					// process non-alphanumeric symbol
					if (text_char == EOF)
					{
						text_scan_data.token = SCAN_P_EOF;
						text_scan_data.id = text_insert_string("EOF");
						text_top = start; /* scrub from text buffer */
						if (retain_comments)
						{
							scan_insert_comment_block("", 0, int.MaxValue);
						}
					}
					else if (text_char == '\n')
					{
						text_top = start; /* scrub from text buffer */
						text_scan_data.token = SCAN_P_EOLN;
						text_scan_data.id = text_insert_string("EOLN");
						text_get_char();
					}
					else
					{
						start = text_top;
						ScanData last_sym;
						ScanData this_sym = null;
						for (; ; )
						{
							last_sym = this_sym;
							text_insert_char((char)text_char);
							text_bot[text_top] = '\0';
							this_sym = (ScanData)symbol_lookup_key(scan_table, text_get_string(start), null);
							if (this_sym == null)
								break;

							text_get_char(); // collect longest match
						}
						// single character means mismatch
						if (text_top == start + 1)
						{
							char ch = text_bot[text_top - 1];
							text_message(TEXT_ERROR_ECHO, $"Unexpected character 0x{ch:X} \'"
									+ (isprint(ch) ? ch : ' ') + "\' in source file\n");
							text_top = start; /* scrub from text buffer */
							text_scan_data.token = SCAN_P_IGNORE;
							text_get_char();
						}
						else
						{
							memcpy(text_scan_data, last_sym);
						}
						text_top = start; /* discard token from text buffer */
					}
				}
				// Now do extended tokens
				if (text_scan_data.extended == SCAN_P_IGNORE)
				{
					continue;
				}
				close = text_scan_data.id;
				nestlevel = 1;
				nestable = false;
				// find string after the ID in the prototype token
				while (text_bot[close++] != 0)
				{
				}
				switch (text_scan_data.extended)
				{
					case SCAN_P_CHAR:
						text_insert_char((char)text_char);
						text_insert_char('\0');
						text_get_char();
						text_scan_data.id = start;
						break;
					case SCAN_P_CHAR_ESC:
						if (text_char == text_bot[close]) // found escape character
						{
							// translate all C escapes. Anything else returns escaped
							// character
							text_get_char(); /* skip escape character */
							switch (text_char)
							{
								case 'n':
									text_insert_char('\n');
									text_get_char();
									break;
								case 't':
									text_insert_char('\t');
									text_get_char();
									break;
								case 'b':
									text_insert_char('\b');
									text_get_char();
									break;
								case 'r':
									text_insert_char('\r');
									text_get_char();
									break;
								case 'f':
									text_insert_char('\f');
									text_get_char();
									break;
								case 'x':
								case 'X': /* hexadecimal */
									start = text_top;
									do
									{
										text_get_char();
										text_insert_char((char)text_char);
									} while (isxdigit(text_char));
									text_top = 0;
									long temp = strtol(text_get_string(start), null, 16);
									text_top = start; /* scrub from buffer */
									if (temp > 255)
									{
										text_message(TEXT_WARNING_ECHO, "Hex escape sequence overflows eight bits: wrapping\n");
									}
									text_insert_char((char)(temp % 255));
									break;
								case '0':
								case '1':
								case '2':
								case '3':
								case '4':
								case '5':
								case '6':
								case '7': /* octal */
									start = text_top;
									do
									{
										text_insert_char((char)text_char);
										text_get_char();
									} while (text_char >= '0' && text_char <= '7');
									text_top = 0; /* change last character to a null */
									temp = strtol(text_get_string(start), null, 8);
									text_top = start; /* scrub from buffer */
									if (temp > 255)
										text_message(TEXT_WARNING_ECHO, "Octal escape sequence overflows eight bits: wrapping\n");
									text_insert_char((char)(temp % 255));
									break;
								default: /* any other quoted character returns itself */
									text_insert_char((char)text_char);
									text_get_char();
									break;
							}
						}
						else
						{
							text_insert_char((char)text_char);
							text_insert_char('\0');
							text_get_char();
						}
						text_insert_char('\0');
						text_scan_data.id = start;
						break;
					case SCAN_P_STRING:
						bool loop;
						do
						{
							while (text_char != text_bot[text_scan_data.id])
							{
								if (text_char == '\n' || text_char == EOF)
								{
									text_message(TEXT_ERROR_ECHO, "Unterminated string\n");
									break;
								}
								text_insert_char((char)text_char);
								text_get_char();
							}
							text_get_char(); // get character after close
							loop = false;
							if (text_char == text_bot[text_scan_data.id])
							{
								text_insert_char((char)text_char);
								text_get_char();
								loop = true;
							}
						} while (loop);
						text_insert_char('\0');
						text_scan_data.id = start;
						break;
					case SCAN_P_STRING_ESC:
						while (text_char != text_bot[text_scan_data.id])
						{
							if (text_char == '\n' || text_char == EOF)
							{
								text_message(TEXT_ERROR_ECHO, "Unterminated string\n");
								break;
							}
							else if (text_char == text_bot[close]) // found escape
																   // character
							{
								text_get_char(); /* skip escape character */
								switch (text_char)
								{
									case 'n':
										text_insert_char('\n');
										text_get_char();
										break;
									case 't':
										text_insert_char('\t');
										text_get_char();
										break;
									case 'b':
										text_insert_char('\b');
										text_get_char();
										break;
									case 'r':
										text_insert_char('\r');
										text_get_char();
										break;
									case 'f':
										text_insert_char('\f');
										text_get_char();
										break;
									case 'x':
									case 'X': /* hexadecimal */
										start = text_top;
										do
										{
											text_get_char();
											text_insert_char((char)text_char);
										} while (isxdigit(text_char));
										text_top = 0; // change last character to a null
										long temp = strtol(text_get_string(start), null, 16);
										text_top = start; /* scrub from buffer */
										if (temp > 255)
											text_message(TEXT_WARNING_ECHO, "Hex escape sequence overflows eight bits: wrapping\n");
										text_insert_char((char)(temp % 255));
										break;
									case '0':
									case '1':
									case '2':
									case '3':
									case '4':
									case '5':
									case '6':
									case '7': /* octal */
										start = text_top;
										do
										{
											text_insert_char((char)text_char);
											text_get_char();
										} while (text_char >= '0' && text_char <= '7');
										text_top = 0; // change last character to a null
										temp = strtol(text_get_string(start), null, 8);
										text_top = start; /* scrub from buffer */
										if (temp > 255)
										{
											text_message(TEXT_WARNING_ECHO,
													"Octal escape sequence overflows eight bits: wrapping\n");
										}
										text_insert_char((char)(temp % 255));
										break;
									default:
										text_insert_char((char)text_char);
										text_get_char();
										break;
								}
							}
							else
							{
								/* ordinary character */
								text_insert_char((char)text_char);
								text_get_char();
							}
						}
						text_get_char(); /* skip close character */
						text_insert_char('\0'); /* terminate string */
						text_scan_data.id = start; /* make current id string body */
						break;

					case SCAN_P_COMMENT_LINE:
					case SCAN_P_COMMENT_LINE_VISIBLE:
						while (text_char != '\n' && text_char != EOF)
						{
							text_insert_char((char)text_char);
							text_get_char();
						}
						text_insert_char('\0'); /* terminate with a null */
						text_scan_data.id = start; /* make current id comment body */
						if (text_scan_data.extended == SCAN_P_COMMENT_LINE)
						{
							text_scan_data.token = SCAN_P_IGNORE;
							if (retain_comments)
							{
								scan_insert_comment_block(text_get_string(start), last_column, scan_sequence_running_number);
							}
							else
							{
								text_top = start; // scrub the comment from text buffer
							}
						}
						break;
					case SCAN_P_COMMENT_NEST:
					case SCAN_P_COMMENT_NEST_VISIBLE:
					case SCAN_P_COMMENT_VISIBLE:
					case SCAN_P_COMMENT:
						nestable = (text_scan_data.extended == SCAN_P_COMMENT_NEST) || (text_scan_data.extended == SCAN_P_COMMENT_NEST_VISIBLE);
						// /* We have to be a bit careful here: remember that the
						// text_get_char() routine puts a space in at the start of each
						// line to
						// delay echoing of the line in the assembler */
						do
						{
							if (text_char == EOF)
								text_message(TEXT_FATAL_ECHO, "Comment terminated by end of file\n");

							if (last != '\n')
								text_insert_char((char)text_char);

							last = text_char;
							text_get_char();
							// single close or double close
							if (text_bot[close + 1] == 0 && text_bot[close] == text_bot[text_top - 1]
									|| text_bot[close + 1] == text_bot[text_top - 1]
											&& text_bot[close] == text_bot[text_top - 2])
							{
								nestlevel--;
							}
							else if (text_bot[text_scan_data.id + 1] == 0
									&& text_bot[text_scan_data.id] == text_bot[text_top - 1]
									|| text_bot[text_scan_data.id + 1] == text_bot[text_top - 1]
											&& text_bot[text_scan_data.id] == text_bot[text_top - 2])
							{
								nestlevel += nestable ? 1 : 0;
							}
						} while (nestlevel > 0);

						if (text_bot[close + 1] != 0)
						{
							text_top--; // backup one extra character
						}
						// backup over close and terminate with a null
						text_bot[text_top - 1] = (char)0;
						text_scan_data.id = start; // make current id comment body
						if (text_scan_data.extended == SCAN_P_COMMENT || text_scan_data.extended == SCAN_P_COMMENT_NEST)
						{
							text_scan_data.token = SCAN_P_IGNORE;
							if (retain_comments)
							{
								scan_insert_comment_block(text_get_string(start), last_column, scan_sequence_running_number);
							}
							else
							{
								text_top = start; // scrub the comment from text buffer
							}
						}
						break;
					default:
						break; /* do nothing */
				}
			} while (text_scan_data.token == SCAN_P_IGNORE);
			text_scan_data.comment_block = last_comment_block;
			if (scan_sequence_running_number != text_sequence_number())
				scan_insert_comment_block(null, 0, text_sequence_number());
			scan_sequence_running_number = text_sequence_number();
			text_scan_data.sourcefilename = rdp_sourcefilename;
			text_scan_data.line_number = text_line_number();
			if (scan_symbol_echo)
			{
				Console.WriteLine("Scan symbol echo");
				text_message(TEXT_INFO, "Scanned ");
				// TODO set_print_element(text_scan_data.token, scan_token_names);
				// text_printf(" id \'%s\', sequence number %lu\n",
				// text_scan_data.id,
				// scan_sequence_running_number);
			}
			if (scan_lexicalise_flag)
			{
				Console.WriteLine("Scan lexicalise flag");
				// TODO scan_token_count++;
				// if (strcmp(text_scan_data.id, "EOF") == 0)
				// text_printf("\n****** %u tokens\n", scan_token_count - 1);
				// else if (strcmp(text_scan_data.id, "EOLN") == 0)
				{
					text_printf("\n");
					// scan_token_count --;
				}
				// else if (text_scan_data.token == SCAN_P_ID)
				// text_printf("ID ");
				// else if (text_scan_data.token == SCAN_P_INTEGER)
				// text_printf("INTEGER ");
				// else if (text_scan_data.token == SCAN_P_REAL)
				// text_printf("REAL ");
				// else if (text_scan_data.extended == SCAN_P_STRING ||
				// text_scan_data.extended
				// == SCAN_P_STRING_ESC)
				// text_printf("STRING ");
				// else if (text_scan_data.extended == SCAN_P_CHAR ||
				// text_scan_data.extended
				// ==
				// SCAN_P_CHAR_ESC)
				// text_printf("CHAR ");
				// else if (text_scan_data.extended == SCAN_P_COMMENT_VISIBLE ||
				// text_scan_data.extended == SCAN_P_COMMENT_NEST_VISIBLE ||
				// text_scan_data.extended == SCAN_P_COMMENT_LINE_VISIBLE )
				// text_printf("COMMENT ");
				// else
				// text_printf("%s ", text_scan_data.id);
			}
		}

		internal static void scan_init(bool case_insensitive, bool newline_visible, bool show_skips,
				bool symbol_echo, string[] token_names)
		{
			scan_case_insensitive = case_insensitive;
			scan_show_skips = show_skips;
			scan_newline_visible = newline_visible;
			scan_symbol_echo = symbol_echo;
			scan_token_names = token_names;

			scan_comment_list = new ScanCommentBlock();
			scan_comment_list_end = scan_comment_list;
			text_scan_data = new ScanData();
			scan_table = symbol_new_table("scan table", 101, 31, new CompareHashPrint());
			scan_insert_comment_block("", 0, 0);
		}

		internal static void scan_lexicalise()
		{
			scan_lexicalise_flag = true;
		}

		internal static void scan_load_keyword(string id1, string id2, int token, int extended)
		{
			ScanData d = new ScanData();
			d.id = text_insert_string(id1);
			if (id2 != null)
			{
				text_insert_string(id2);
			}
			d.token = token;
			d.extended = extended;
			symbol_insert_symbol(scan_table, d);
		}

		internal static bool scan_test(string production, int valid, Set stop)
		{
			if (valid != text_scan_data.token)
			{
				if (stop != null)
				{
					printScannedToken(production);
					text_printf(" while expecting ");
					set_print_element(valid, scan_token_names, true);
					text_printf("\n");
					skip(stop);
				}
				return false;
			}
			return true;
		}

		internal static bool scan_test_set(string production, Set valid, Set stop)
		{
			if (!valid.includes(text_scan_data.token))
			{
				if (stop != null)
				{
					printScannedToken(production);
					text_printf(" while expecting " + (set_cardinality(valid) == 1 ? "" : "one of "));
					valid.print(scan_token_names, 60);
					text_printf("\n");
					skip(stop);
				}
				return false;
			}
			return true;
		}

		private static void printScannedToken(string production)
		{
			if (production != null)
			{
				text_message(TEXT_ERROR_ECHO, "In rule \'" + production + "\', scanned ");
			}
			else
			{
				text_message(TEXT_ERROR_ECHO, "Scanned ");
			}
			set_print_element(text_scan_data.token, scan_token_names, true);
		}

		private static void scan_insert_comment_block(string pattern, int column, int sequence_number)
		{
			ScanCommentBlock temp = new ScanCommentBlock();
			scan_comment_list_end.comment = pattern;
			scan_comment_list_end.sequence_number = sequence_number;
			scan_comment_list_end.column = column;
			temp.previous = scan_comment_list_end;
			scan_comment_list_end.next = temp;
			scan_comment_list_end = temp;
			last_comment_block = temp;
		}

		private static void skip(Set stop)
		{
			while (!stop.includes(text_scan_data.token))
			{
				scan_();
			}
			if (scan_show_skips)
			{
				text_message(TEXT_ERROR_ECHO, "Skipping to...\n");
			}
		}
	}
}

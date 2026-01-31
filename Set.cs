using System;
using System.Collections.Generic;
using static RdpOutput.Text;

namespace RdpOutput
{
	internal class Set
	{
		internal Set(params int[] bits)
		{
			Clear();
			foreach (int bit in bits)
			{
				Insert(bit);
			}
		}

#if false
		internal interface Indent
		{
			int indent();
		}
#endif

		/// <summary>
		/// Calculate the set cardinality. The method is static to allow null set to be
		/// passed into it (with cardinality 0).
		/// </summary>
		/// <param name="src">The set or null.</param>
		/// <returns></returns>
		internal static int SetCardinality(Set src)
		{
			return src == null ? 0 : src.Cardinality();
		}

		/// <summary>
		/// Print the string associated with the set element. The method is static to avoid
		/// passing set element name atring arrays to all sets in the app. All references to
		/// this method are made directly or indirectly from the Scanner object that does not
		/// have all too many sets but for now this static will do.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="element_names"></param>
		/// <param name="comments"></param>
		/// <returns></returns>
		internal static int SetPrintElement(int element, string[] element_names, bool comments)
		{
			if (element_names == null)
			{
				return text_printf(Convert.ToString(element));
			}
			else
			{
				string elementString = element_names[element];
				if (!comments)
					elementString = elementString.Split(' ')[0];
				return text_printf(elementString);
			}
		}

		private uint[] data = new uint[10];

#if NEVER
		/// <summary>
		/// Clear a dst and then set only those bits specified by src
		/// </summary>
		/// <param name="element"></param>
		internal void assign(int element)
		{
			clear();
			set(element);
		}

		/// <summary>
		/// Assign one set to another
		/// </summary>
		/// <param name="src"></param>
		internal void assignSet(Set src)
		{
			clear();
			unite(src);
		}
#endif

		internal void Clear()
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = 0;
			}
		}

		internal bool Includes(int element)
		{
			Grow(element);
			int index = element / 32;
			element &= 0x1F;
			return (data[index] & 1 << element) != 0;
		}

#if NEVER
		internal void intersect(Set src)
		{
			/* only iterate over shortest set */
			int length = this.length() < src.length() ? this.length() : src.length();
			for (int i = 0; i < length; i++)
			{
				data[i] &= src.data[i];
			}
			/* Now clear rest of dst */
			while (length < this.length())
			{
				data[length++] = 0;
			}
		}
#endif

		internal void Print(string[] element_names, int line_length)
		{
			int column = 0;
			bool not_first = false;
			int[] elements = ToArray();
			foreach (int element in elements)
			{
				if (not_first)
				{
					column += text_printf(", ");
				}
				else
				{
					not_first = true;
				}

				if (line_length != 0 && column >= line_length)
				{
					text_printf("\n");
					column = 0;
				}
				column += SetPrintElement(element, element_names, true);
			}
		}

#if NEVER
		public delegate int IndentFunction();

		internal void print(string[] element_names, int initialOffset, IndentFunction indent, int line_length, bool comments)
		{
			int column = initialOffset;
			bool not_first = false;
			int[] elements = array();
			foreach (int element in elements)
			{
				if (not_first)
				{
					column += text_printf(", ");
				}
				else
				{
					not_first = true;
				}

				if (line_length != 0 && column >= line_length)
				{
					text_printf("\n");
					column = indent();
				}
				column += set_print_element(element, element_names, comments);
			}
		}
#endif

		internal void Insert(int element)
		{
			Grow(element);
			int index = element / 32;
			element &= 0x1F;
			data[index] |= (uint)(1 << element);
		}

#if NEVER
		internal void unite(Set src)
		{
			grow(src.length());
			for (int i = 0; i < data.Length; i++)
			{
				data[i] |= src.data[i];
			}
		}
#endif

		private int[] ToArray()
		{
			List<int> elements = new List<int>();
			for (int word = 0; word < data.Length; word++)
			{
				for (int bit = 0; bit < 32; bit++)
				{
					if ((data[word] & 1 << bit) != 0)
					{
						elements.Add(word * 32 + bit);
					}
				}
			}
			return elements.ToArray();
		}

		private int Cardinality()
		{
			int[] bitCounts = new int[] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4 };

			int cardinality = 0;
			foreach (uint bits in data)
			{
				uint b = bits;
				for (int i = 0; i < 8; i++)
				{
					cardinality += bitCounts[(int)(b & 0xF)];
					b >>= 4;
				}
			}
			return cardinality;
		}

		private void Grow(int bits)
		{
			int index = (bits + 31) / 32;
			if (index >= data.Length)
			{
				uint[] newData = new uint[index + 5];
				for (int i = 0; i < newData.Length; i++)
				{
					newData[i] = 0;
				}
				for (int i = 0; i < data.Length; i++)
				{
					newData[i] = data[i];
				}
				data = newData;
			}
		}

#if NEVER
		private int length()
		{
			return data.Length;
		}

		internal void printIndented(string[] element_names, int column, int width, int indentLevel)
		{
			bool isFirst = true;
			int[] elements = ToArray();
			foreach (int element in elements)
			{
				string elementString;
				if (element_names == null)
				{
					elementString = Convert.ToString(element);
				}
				else
				{
					elementString = element_names[element];
					elementString = elementString.Split(' ')[0];
				}
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					column += text_printf(", ");
				}
				int endColumn = column + elementString.Length + 2;
				if (width > 0 && endColumn >= width - 3)
				{
					text_printf("\n");
					for (int i = 0; i < indentLevel + 2; i++)
					{
						text_printf("\t");
					}
					column = (indentLevel + 2) * 4;
				}
				column += text_printf(elementString);
			}
		}
#endif
	}
}

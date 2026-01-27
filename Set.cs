using System;
using System.Collections.Generic;
using static RdpOutput.Text;

namespace RdpOutput
{
	internal class Set
	{
#if false
		internal interface Indent
		{
			int indent();
		}
#endif

		internal static int set_cardinality(Set src)
		{
			return src == null ? 0 : src.cardinality();
		}

		internal static int set_print_element(int element, string[] element_names, bool comments)
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

		/// <summary>
		/// Clear a dst and then set only those bits specified by src
		/// </summary>
		/// <param name="element"></param>
		internal void assign(int element)
		{
			clear();
			set(element);
		}

		internal void assignList(params int[] bits)
		{
			clear();
			foreach (int bit in bits)
			{
				set(bit);
			}
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

		internal void clear()
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = 0;
			}
		}

		internal bool includes(int element)
		{
			grow(element);
			int index = element / 32;
			element &= 0x1F;
			return (data[index] & 1 << element) != 0;
		}

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

		internal void print(string[] element_names, int line_length)
		{
			int column = 0;
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
					column = 0;
				}
				column += set_print_element(element, element_names, true);
			}
		}

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

		internal void set(int element)
		{
			grow(element);
			int index = element / 32;
			element &= 0x1F;
			data[index] |= (uint)(1 << element);
		}

		internal void unite(Set src)
		{
			grow(src.length());
			for (int i = 0; i < data.Length; i++)
			{
				data[i] |= src.data[i];
			}
		}

		private int[] array()
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

		private int cardinality()
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

		private void grow(int bits)
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

		private int length()
		{
			return data.Length;
		}

		internal void printIndented(string[] element_names, int column, int width, int indentLevel)
		{
			bool isFirst = true;
			int[] elements = array();
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
	}
}

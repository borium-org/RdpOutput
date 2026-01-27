using System;
using static RdpOutput.Text;

namespace RdpOutput
{
	internal class CompareHashPrint
	{
		internal int compare(string key, Symbol p)
		{
			string r = text_get_string(p.id);
			return string.Compare(key, r, StringComparison.Ordinal);
		}

#if false
		internal int compare(Symbol left, Symbol right)
		{
			string l = text_get_string(left.id);
			string r = text_get_string(right.id);
			return string.Compare(l, r, StringComparison.Ordinal);
		}
#endif

		internal int hash(int hash_prime, string str)
		{
			int hashnumber = 0;
			if (str != null)
			{
				for (int i = 0; i < str.Length; i++)
				{
					hashnumber = str[i] + hash_prime * hashnumber;
				}
			}
			return hashnumber & 0x7FFFFFFF;
		}

#if false
		void print(Symbol s)
		{
			s.print();
		}
#endif
	}
}

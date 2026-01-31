namespace RdpOutput
{
	internal class Symbol // : IComparable<Symbol>
	{
#if NEVER
		internal static SymbolScopeData symbol_new_scope(SymbolTable table, string id)
		{
			SymbolScopeData p = new SymbolScopeData();

			p.id = text.text_insert_string(id);
			p.next_hash = table.scopes;
			table.current = table.scopes = p;
			if (p.next_hash != null)
				p.next_hash.last_hash.set(p.next_hash);
			return p;
		}
#endif

		/** next symbol in hash list */
		internal Symbol next_hash;

		/** pointer to next pointer of last_symbol in hash list */
		internal Pointer<Symbol> last_hash = new Pointer<Symbol>();

		/** next symbol in scope list */
		internal Symbol next_scope;

		/** pointer to the scope symbol */
		internal Symbol scope;

		/** hash value for quick searching */
		internal int hash;

		internal int id;

#if NEVER
		public int CompareTo(Symbol other)
		{
			return string.Compare(text_get_string(id), text_get_string(other.id), StringComparison.Ordinal);
		}

		/// <summary>
		/// Return next symbol in scope chain. Return NULL if at end
		/// </summary>
		/// <returns></returns>
		internal Symbol nextSymbolInScope()
		{
			return next_scope;
		}

		internal void print()
		{
			text_printf(id == 0 ? "Null symbol" : text_get_string(id));
		}

		internal void unlinkSymbol()
		{
			Symbol s = this;

			s.last_hash.set(s.next_hash); /* point previous pointer to next symbol */
			if (s.next_hash != null)
			{
				s.next_hash.last_hash.set(s.last_hash.value());
			}
		}
#endif
	}
}

namespace RdpOutput
{
	internal class SymbolScopeData : Symbol
	{
#if NEVER
		internal void assign(SymbolScopeData other)
		{
			next_hash = other.next_hash;
			last_hash.set(other.last_hash.value());
			next_scope = other.next_scope;
			scope = other.scope;
			hash = other.hash;
			id = other.id;
		}

		internal void unlinkScope()
		{
			Symbol s = this;
			s = s.next_scope;
			while (s != null)
			{
				s.unlinkSymbol();
				s = s.next_scope;
			}
		}

		/// <summary>
		/// Sort a scope region. Don't change positions in the hash table: just move pointers in the scope chain
		/// </summary>
		internal void sort()
		{
			Symbol s = this;
			// attempt to sort empty list
			if (s.next_scope == null)
			{
				return;
			}
			// attempt to sort list of one
			if (s.next_scope.next_scope == null)
			{
				return;
			}
			List<Symbol> list = new List<Symbol>();
			Symbol temp_scope = s.next_scope;
			while (temp_scope != null)
			{
				// FIXME sometimes instead of next_scope being null it points to an
				// object already in the list
				if (list.Contains(temp_scope))
				{
					break;
				}
				list.Add(temp_scope);
				// System.out.println("Added " + text_get_string(temp_scope.id) +
				// " "
				// + temp_scope);
				temp_scope = temp_scope.next_scope;
			}
			Symbol[] array = list.ToArray();
			Array.Sort(array);
			foreach (Symbol sym in array)
			{
				sym.next_scope = null;
				s.next_scope = sym;
				s = sym;
			}
		}
#endif
	}
}

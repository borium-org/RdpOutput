namespace RdpOutput
{
	internal class SymbolTable
	{
		/// <summary>
		/// An identifying string
		/// </summary>
		internal string name;

		/// <summary>
		/// Table of pointers to hash lists
		/// </summary>
		internal Symbol[] table;

		/// <summary>
		/// Pointers to chain of scope lists
		/// </summary>
		internal SymbolScopeData current;

		/// <summary>
		/// Pointer to first scope list
		/// </summary>
		internal SymbolScopeData scopes;

		/// <summary>
		/// Number of buckets in symbol table
		/// </summary>
		internal int hash_size;

		/// <summary>
		/// Hashing prime: hashsize and hashprime should be coprime
		/// </summary>
		internal int hash_prime;

		internal CompareHashPrint compareHashPrint;

		internal SymbolTable(string name, int symbol_hashsize, int symbol_hashprime,
				CompareHashPrint compareHashPrint, Text text)
		{
			SymbolScopeData scope = new SymbolScopeData
			{
				id = text.InsertString("Global")
			};
			this.name = name;
			hash_size = symbol_hashsize;
			hash_prime = symbol_hashprime;
			this.compareHashPrint = compareHashPrint;
			table = new Symbol[symbol_hashsize];
			current = scopes = scope;
		}

		internal Symbol InsertSymbol(Symbol symbol, Text text)
		{
			Symbol s = symbol;

			s.hash = Hash(hash_prime, text.GetString(symbol.id));
			int hash_index = s.hash % hash_size;

			s.next_hash = table[hash_index];
			table[hash_index] = s;

			s.last_hash.set(table[hash_index]);

			// if this wasn't the start of a new list ...
			if (s.next_hash != null)
			{
				// ...point old list next back at s
				s.next_hash.last_hash.set(s.next_hash);
			}

			// now insert in scope list
			s.next_scope = current.next_scope;
			current.next_scope = s;

			// set up pointer to scope block
			s.scope = current;

			return symbol;
		}

		/// <summary>
		/// Lookup a symbol by id. Return null if it is not found
		/// </summary>
		/// <param name="key"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		internal Symbol LookupKey(string key, SymbolScopeData scope)
		{
			int hash = Hash(hash_prime, key);
			Symbol p = table[hash % hash_size];

			// look for symbol with same hash and a true compare
			while (!(p == null || Compare(key, p) == 0 && !(p.scope != scope && scope != null)))
			{
				p = p.next_hash;
			}

			return p;
		}

		internal int Compare(string key, Symbol p)
		{
			return compareHashPrint.Compare(key, p);
		}

#if NEVER
		/// <summary>
		/// Return current scope
		/// </summary>
		/// <returns></returns>
		internal SymbolScopeData getScope()
		{
			return current;
		}
#endif

		internal int Hash(int prime, string key)
		{
			return compareHashPrint.Hash(prime, key);
		}
	}
}

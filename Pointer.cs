namespace RdpOutput
{
	internal class Pointer<T>
	{
		private T internalValue;

		internal Pointer()
		{
			internalValue = default(T);
		}

		internal Pointer(T value)
		{
			internalValue = value;
		}

		internal void set(T value)
		{
			internalValue = value;
		}

		internal T value()
		{
			return internalValue;
		}
	}
}

using BestHTTP.PlatformSupport.IL2CPP;

namespace BestHTTP.PlatformSupport.Memory
{
	[Il2CppEagerStaticClassConstruction]
	public struct BufferSegment
	{
		public static readonly BufferSegment Empty;

		public readonly byte[] Data;

		public readonly int Offset;

		public readonly int Count;

		public BufferSegment(byte[] data, int offset, int count)
		{
			Data = null;
			Offset = 0;
			Count = 0;
		}

		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		public override bool Equals(object obj)
		{
			return false;
		}

		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		public bool Equals(BufferSegment other)
		{
			return false;
		}

		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		public override int GetHashCode()
		{
			return 0;
		}

		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		public static bool operator ==(BufferSegment left, BufferSegment right)
		{
			return false;
		}

		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		public static bool operator !=(BufferSegment left, BufferSegment right)
		{
			return false;
		}

		[Il2CppSetOption(Option.NullChecks, false)]
		[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
		[Il2CppSetOption(Option.DivideByZeroChecks, false)]
		public override string ToString()
		{
			return null;
		}
	}
}

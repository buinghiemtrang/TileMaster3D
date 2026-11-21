using System.Collections.Generic;
using System.IO;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.Extensions
{
	public sealed class BufferSegmentStream : Stream
	{
		private long _length;

		private List<BufferSegment> bufferList;

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => 0L;

		public override long Position
		{
			get
			{
				return 0L;
			}
			set
			{
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
		}

		public void Write(BufferSegment bufferSegment)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return 0L;
		}

		public override void SetLength(long value)
		{
		}
	}
}

using System;
using System.IO;

namespace Core.Code.AssetBundles;

public class ReportingStream : Stream
{
	private Stream source;

	private IProgress<long> progress;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => source.Length;

	public override long Position
	{
		get
		{
			return source.Position;
		}
		set
		{
			source.Position = value;
		}
	}

	public ReportingStream(Stream source, IProgress<long> progress)
	{
		this.source = source;
		this.progress = progress;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			source.Dispose();
		}
	}

	public override void Flush()
	{
		source.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = source.Read(buffer, offset, count);
		progress.Report(num);
		return num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}
}

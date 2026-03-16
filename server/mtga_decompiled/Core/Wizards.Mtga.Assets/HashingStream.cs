using System;
using System.IO;
using System.Security.Cryptography;

namespace Wizards.Mtga.Assets;

public class HashingStream : Stream
{
	private Stream source;

	private HashAlgorithm hasher;

	private bool closeSource;

	public byte[] Hash => hasher.Hash;

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
			throw new NotSupportedException();
		}
	}

	public HashingStream(Stream source, HashAlgorithm hasher, bool closeSource = false)
	{
		this.source = source;
		this.hasher = hasher;
		this.closeSource = closeSource;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			hasher.Dispose();
			if (closeSource)
			{
				source.Dispose();
			}
		}
	}

	public override void Flush()
	{
		source.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = source.Read(buffer, offset, count);
		if (num != 0)
		{
			hasher.TransformBlock(buffer, offset, num, null, 0);
		}
		else
		{
			hasher.TransformFinalBlock(buffer, offset, 0);
		}
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

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyItemImageInfoByIndexOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_ItemId;

	private uint m_ImageInfoIndex;

	public int ApiVersion
	{
		get
		{
			return m_ApiVersion;
		}
		set
		{
			m_ApiVersion = value;
		}
	}

	public EpicAccountId LocalUserId
	{
		get
		{
			if (!(m_LocalUserId == IntPtr.Zero))
			{
				return new EpicAccountId(m_LocalUserId);
			}
			return null;
		}
		set
		{
			m_LocalUserId = ((value == null) ? IntPtr.Zero : value.InnerHandle);
		}
	}

	public string ItemId
	{
		get
		{
			return m_ItemId;
		}
		set
		{
			m_ItemId = value;
		}
	}

	public uint ImageInfoIndex
	{
		get
		{
			return m_ImageInfoIndex;
		}
		set
		{
			m_ImageInfoIndex = value;
		}
	}

	public void Dispose()
	{
	}
}

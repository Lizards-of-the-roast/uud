using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CopyOfferItemByIndexOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_LocalUserId;

	[MarshalAs(UnmanagedType.LPStr)]
	private string m_OfferId;

	private uint m_ItemIndex;

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

	public string OfferId
	{
		get
		{
			return m_OfferId;
		}
		set
		{
			m_OfferId = value;
		}
	}

	public uint ItemIndex
	{
		get
		{
			return m_ItemIndex;
		}
		set
		{
			m_ItemIndex = value;
		}
	}

	public void Dispose()
	{
	}
}

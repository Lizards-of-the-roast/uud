using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct SessionSearchSetParameterOptions_ : IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_Parameter;

	private OnlineComparisonOp m_ComparisonOp;

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

	public AttributeData_ Parameter
	{
		get
		{
			return Helper.GetAllocation<AttributeData_>(m_Parameter);
		}
		set
		{
			Helper.RegisterAllocation(ref m_Parameter, value);
		}
	}

	public OnlineComparisonOp ComparisonOp
	{
		get
		{
			return m_ComparisonOp;
		}
		set
		{
			m_ComparisonOp = value;
		}
	}

	public void Dispose()
	{
		Helper.ReleaseAllocation(ref m_Parameter);
	}
}

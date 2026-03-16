using System.Collections.Generic;

public class CDCAnchorTypeComparer : IEqualityComparer<CDCAnchorType>
{
	public bool Equals(CDCAnchorType x, CDCAnchorType y)
	{
		return x == y;
	}

	public int GetHashCode(CDCAnchorType obj)
	{
		return (int)obj;
	}
}

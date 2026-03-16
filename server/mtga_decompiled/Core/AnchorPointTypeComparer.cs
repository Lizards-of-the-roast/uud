using System.Collections.Generic;

public class AnchorPointTypeComparer : IEqualityComparer<AnchorPointType>
{
	public bool Equals(AnchorPointType x, AnchorPointType y)
	{
		return x == y;
	}

	public int GetHashCode(AnchorPointType obj)
	{
		return (int)obj;
	}
}

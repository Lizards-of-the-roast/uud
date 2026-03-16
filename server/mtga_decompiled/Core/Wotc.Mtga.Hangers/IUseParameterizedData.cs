using System.Collections.Generic;

namespace Wotc.Mtga.Hangers;

public interface IUseParameterizedData
{
	void SetData(IReadOnlyDictionary<string, string> data);
}

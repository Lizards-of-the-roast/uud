using System.Collections.Generic;

namespace AssetLookupTree;

public interface IPayload
{
	IEnumerable<string> GetFilePaths();
}

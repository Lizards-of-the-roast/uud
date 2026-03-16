using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Carousel;

public class CarouselName : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.CarouselName == null)
		{
			return false;
		}
		value = bb.CarouselName;
		return true;
	}
}

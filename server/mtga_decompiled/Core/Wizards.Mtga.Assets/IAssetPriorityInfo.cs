namespace Wizards.Mtga.Assets;

public interface IAssetPriorityInfo
{
	string Name { get; }

	AssetPriority Priority { get; set; }
}

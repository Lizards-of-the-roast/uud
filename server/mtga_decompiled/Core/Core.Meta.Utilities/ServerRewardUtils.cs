namespace Core.Meta.Utilities;

public abstract class ServerRewardUtils
{
	private const string ServerRewardsPath = "Assets/Core/Meta/NetworkResources/ServerRewards/";

	public static string FormatAssetFromServerReference(string serverAssetReference, ServerRewardFileExtension fileExtension)
	{
		return "Assets/Core/Meta/NetworkResources/ServerRewards/" + serverAssetReference + "." + fileExtension.ToString().ToLower();
	}
}

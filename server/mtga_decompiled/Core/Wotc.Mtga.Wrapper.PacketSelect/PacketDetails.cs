namespace Wotc.Mtga.Wrapper.PacketSelect;

public readonly struct PacketDetails
{
	public readonly string Name;

	public readonly string PacketId;

	public readonly uint LandGrpId;

	public readonly uint ArtId;

	public readonly string[] RawColors;

	public PacketDetails(string name, string packId, uint landGrpId, uint artId, string[] rawColors = null)
	{
		Name = name;
		PacketId = packId;
		LandGrpId = landGrpId;
		ArtId = artId;
		RawColors = rawColors;
		if (RawColors == null || RawColors.Length == 0)
		{
			RawColors = new string[1] { "C" };
		}
		RawColors = PacketColorCore.SortColors(RawColors);
	}
}

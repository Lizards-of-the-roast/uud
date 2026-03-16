namespace Wotc.Mtga.Wrapper.PacketSelect;

public class NullPacketAudioProvider : IPacketAudioProvider
{
	public string GetPacketAudio(string packetId)
	{
		return string.Empty;
	}
}

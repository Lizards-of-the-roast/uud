namespace Wotc.Mtga.Wrapper.PacketSelect;

public readonly struct ServiceState
{
	public readonly PacketDetails[] SubmittedPackets;

	public readonly PacketDetails[] PacketOptions;

	public ServiceState(PacketDetails[] submitted, PacketDetails[] options)
	{
		SubmittedPackets = ((submitted != null) ? submitted : new PacketDetails[0]);
		PacketOptions = ((options != null) ? options : new PacketDetails[0]);
	}

	public bool AllPacketsSubmitted()
	{
		PacketDetails[] submittedPackets = SubmittedPackets;
		for (int i = 0; i < submittedPackets.Length; i++)
		{
			PacketDetails packetDetails = submittedPackets[i];
			if (packetDetails.Equals(default(PacketDetails)))
			{
				return false;
			}
		}
		return true;
	}

	public uint SubmissionCount()
	{
		if (AllPacketsSubmitted())
		{
			return (uint)SubmittedPackets.Length;
		}
		uint num = 0u;
		for (int i = 0; i < SubmittedPackets.Length; i++)
		{
			if (!SubmittedPackets[i].Equals(default(PacketDetails)))
			{
				num++;
			}
		}
		return num;
	}

	public bool CanSubmit(string packetId)
	{
		if (!AllPacketsSubmitted())
		{
			return !GetOptionById(packetId).Equals(default(PacketDetails));
		}
		return false;
	}

	public PacketDetails GetOptionById(string packetId)
	{
		PacketDetails[] packetOptions = PacketOptions;
		for (int i = 0; i < packetOptions.Length; i++)
		{
			PacketDetails result = packetOptions[i];
			if (result.PacketId == packetId)
			{
				return result;
			}
		}
		return default(PacketDetails);
	}

	public PacketDetails GetSubmissionById(string packetId)
	{
		PacketDetails[] submittedPackets = SubmittedPackets;
		for (int i = 0; i < submittedPackets.Length; i++)
		{
			PacketDetails result = submittedPackets[i];
			if (result.PacketId == packetId)
			{
				return result;
			}
		}
		return default(PacketDetails);
	}

	public PacketDetails GetDetailsById(string packetId)
	{
		PacketDetails result = GetSubmissionById(packetId);
		if (result.Equals(default(PacketDetails)))
		{
			result = GetOptionById(packetId);
		}
		return result;
	}
}

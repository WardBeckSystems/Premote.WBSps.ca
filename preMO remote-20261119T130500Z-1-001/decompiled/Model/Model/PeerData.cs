using System.Collections.Generic;
using System.Net;

namespace Model;

public class PeerData
{
	public uint InterfaceIndex;

	public string Name;

	public string Type;

	public string Domain;

	public IPAddress Address;

	public int Port;

	public static Dictionary<string, string> dictBonjour = new Dictionary<string, string>();

	public override string ToString()
	{
		return Name;
	}

	public override bool Equals(object other)
	{
		bool result = false;
		if (other != null)
		{
			if (this == other)
			{
				result = true;
			}
			else if (other is PeerData)
			{
				PeerData peerData = (PeerData)other;
				result = Name == peerData.Name;
			}
		}
		return result;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}
}

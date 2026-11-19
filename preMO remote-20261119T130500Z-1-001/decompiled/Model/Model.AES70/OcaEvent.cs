using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class OcaEvent
{
	public OcaEventID eventID;

	public uint emitterOno { get; set; }
}

using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Ocp1Header
{
	public OcaMessageType pduType;

	public ushort protocolVersion { get; set; }

	public uint pduSize { get; set; }

	public ushort messageCount { get; set; }
}

using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Ocp1Command
{
	public OcaMethodID methodID;

	public Ocp1Parameters parameters;

	public uint commandSize { get; set; }

	public uint handle { get; set; }

	public uint targetONo { get; set; }
}

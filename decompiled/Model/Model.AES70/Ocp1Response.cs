using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Ocp1Response
{
	public Ocp1Parameters parameters;

	public uint responseSize { get; set; }

	public uint handle { get; set; }

	public OcaStatus stattusCode { get; set; }
}

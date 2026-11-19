using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class OcaEventID
{
	public ushort treeLevel { get; set; }

	public ushort eventIndex { get; set; }
}

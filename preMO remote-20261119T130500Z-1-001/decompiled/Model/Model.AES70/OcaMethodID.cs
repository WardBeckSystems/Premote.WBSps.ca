using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class OcaMethodID
{
	public ushort treeLevel { get; set; }

	public ushort methodIndex { get; set; }
}

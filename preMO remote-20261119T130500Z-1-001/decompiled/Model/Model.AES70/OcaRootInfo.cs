using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class OcaRootInfo
{
	public bool Lockable { get; set; }

	public string Role { get; set; }

	public ushort ClassVersion { get; set; }

	public string ClassID { get; set; }

	public uint ONo { get; set; }
}

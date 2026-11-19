using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Ocp1Parameters
{
	public byte parameterCount;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
	public byte[] parameters;
}

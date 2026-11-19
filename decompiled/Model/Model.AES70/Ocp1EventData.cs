using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Ocp1EventData
{
	public OcaEvent ocaevent;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	public byte[] eventParameters;
}

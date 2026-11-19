using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Ocp1ResponsePdu
{
	public byte syncVal;

	public Ocp1Header header;

	public Ocp1Response responses;
}

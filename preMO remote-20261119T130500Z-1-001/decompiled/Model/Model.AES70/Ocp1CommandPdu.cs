using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Ocp1CommandPdu
{
	public byte syncVal;

	public Ocp1Header header;

	public Ocp1Command commands;
}

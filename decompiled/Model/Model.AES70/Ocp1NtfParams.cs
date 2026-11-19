using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Ocp1NtfParams
{
	public Ocp1EventData eventData;

	public byte parameterCount { get; set; }

	public uint context { get; set; }
}

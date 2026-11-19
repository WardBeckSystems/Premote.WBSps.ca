using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class OcaWorkerInfo
{
	public string ClassId { get; }

	public ushort ClassVersion { get; set; }

	public bool Enabled { get; set; }

	public List<string> Ports { get; set; }

	public string Label { get; set; }

	public ushort Owner { get; set; }

	public uint Latency { get; set; }
}

using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Ocp1Notification
{
	public OcaMethodID methodID;

	public Ocp1NtfParams parameters;

	public uint notificationSize { get; set; }

	public uint targetOno { get; set; }

	public Ocp1Notification(uint targetOno, Ocp1NtfParams parameters)
	{
		this.targetOno = targetOno;
		this.parameters = parameters;
	}
}

using System.Runtime.InteropServices;

namespace Model.AES70;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Ocp1NotificationPdu
{
	public byte syncVal;

	public Ocp1Header head;

	public Ocp1Notification notifications;
}

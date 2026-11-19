using System.Runtime.InteropServices;
using Model.AES70;

namespace PreMoBLL;

public class OcaUnmarshalByte
{
	private Ocp1NotificationPdu ocp1pdu;

	private Ocp1ResponsePdu responsepdu;

	public Ocp1NotificationPdu Onpacket(byte[] packet)
	{
		ocp1pdu = new Ocp1NotificationPdu();
		GCHandle gCHandle = GCHandle.Alloc(packet, GCHandleType.Pinned);
		ocp1pdu = (Ocp1NotificationPdu)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(Ocp1NotificationPdu));
		gCHandle.Free();
		return ocp1pdu;
	}

	public Ocp1ResponsePdu OnResponsePacket(byte[] packet)
	{
		responsepdu = new Ocp1ResponsePdu();
		GCHandle gCHandle = GCHandle.Alloc(packet, GCHandleType.Pinned);
		responsepdu = (Ocp1ResponsePdu)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(Ocp1ResponsePdu));
		gCHandle.Free();
		return responsepdu;
	}
}

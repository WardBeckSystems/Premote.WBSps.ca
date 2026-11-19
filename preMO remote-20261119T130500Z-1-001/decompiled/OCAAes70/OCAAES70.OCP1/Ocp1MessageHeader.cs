using Model.AES70;

namespace OCAAES70.OCP1;

public class Ocp1MessageHeader
{
	private Ocp1Header ocp1header = new Ocp1Header();

	public Ocp1Header MarshalHeader(ushort proversion, uint pdusize, OcaMessageType ocatype, ushort messagecount)
	{
		ocp1header.protocolVersion = proversion;
		ocp1header.pduSize = pdusize;
		ocp1header.pduType = ocatype;
		ocp1header.messageCount = messagecount;
		return ocp1header;
	}
}

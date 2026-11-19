using Model.AES70;

namespace OCAAES70.OCP1;

public class Ocp1CommandMessage
{
	public Ocp1CommandPdu GetCommandPdu(Ocp1Command command, Ocp1Header header, uint count)
	{
		Ocp1CommandPdu ocp1CommandPdu = new Ocp1CommandPdu();
		ocp1CommandPdu.commands = command;
		ocp1CommandPdu.header = header;
		ocp1CommandPdu.syncVal = 59;
		return ocp1CommandPdu;
	}
}

using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Model.AES70;
using OCAAES70.OCC.ControlClasses.Workers.BlocksAndMatrices;
using OCAAES70.OCP1;

namespace PreMoBLL;

public class GainBLL
{
	private CommandBLL commandbll = new CommandBLL();

	public void CommandMessage(TcpClient tcpclient, OcaGainInfo gaininfo, OcaBlock block)
	{
		uint handle = 0u;
		uint ono = 0u;
		Ocp1MessageHeader ocp1MessageHeader = new Ocp1MessageHeader();
		Ocp1MessageCommand ocp1MessageCommand = new Ocp1MessageCommand();
		Ocp1CommandMessage ocp1CommandMessage = new Ocp1CommandMessage();
		Ocp1Header ocp1Header = new Ocp1Header();
		Ocp1Command ocp1Command = new Ocp1Command();
		Ocp1CommandPdu ocp1CommandPdu = new Ocp1CommandPdu();
		switch (block.Label)
		{
		case "CH1":
			handle = 16u;
			ono = 21u;
			break;
		case "CH2":
			handle = 48u;
			ono = 53u;
			break;
		case "DOWNLOAD1":
			handle = 32u;
			ono = 35u;
			break;
		case "DOWNLOAD2":
			handle = 64u;
			ono = 67u;
			break;
		case "FRONT":
			handle = 80u;
			ono = 82u;
			break;
		case "REAR":
			handle = 96u;
			ono = 98u;
			break;
		case "SYSTEM":
			handle = 144u;
			ono = 145u;
			break;
		case "STREAM":
			handle = 112u;
			ono = 115u;
			break;
		}
		byte[] param = new byte[1] { gaininfo.Gain };
		ocp1Command = ocp1MessageCommand.GetCommand(handle, ono, param);
		byte pdusize = Convert.ToByte(5 + Marshal.SizeOf(ocp1Command));
		ocp1Header = ocp1MessageHeader.MarshalHeader(1, pdusize, OcaMessageType.OcaCmd, 1);
		ocp1CommandPdu = ocp1CommandMessage.GetCommandPdu(ocp1Command, ocp1Header, 1u);
		commandbll.SendCommand(tcpclient, ocp1CommandPdu);
	}
}

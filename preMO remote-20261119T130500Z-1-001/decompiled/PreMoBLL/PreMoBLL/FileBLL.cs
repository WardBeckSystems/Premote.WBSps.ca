using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Model.AES70;
using OCAAES70.OCC.ControlClasses.Workers.BlocksAndMatrices;
using OCAAES70.OCP1;

namespace PreMoBLL;

public class FileBLL
{
	private CommandBLL commandbll = new CommandBLL();

	public void CommandMessage(TcpClient tcpclient, OcaFileInfo fileinfo, OcaBlock ocablock)
	{
		uint num = 0u;
		uint num2 = 0u;
		Ocp1MessageHeader ocp1MessageHeader = new Ocp1MessageHeader();
		Ocp1MessageCommand ocp1MessageCommand = new Ocp1MessageCommand();
		Ocp1CommandMessage ocp1CommandMessage = new Ocp1CommandMessage();
		Ocp1Header ocp1Header = new Ocp1Header();
		Ocp1Command ocp1Command = new Ocp1Command();
		Ocp1CommandPdu ocp1CommandPdu = new Ocp1CommandPdu();
		num = ocablock.ONo;
		num2 = Operate_File(ocablock);
		byte[] param = new byte[1] { fileinfo.fileNum };
		ocp1Command = ocp1MessageCommand.GetCommand(num, num2, param);
		byte pdusize = Convert.ToByte(5 + Marshal.SizeOf(ocp1Command));
		ocp1Header = ocp1MessageHeader.MarshalHeader(1, pdusize, OcaMessageType.OcaCmd, 1);
		ocp1CommandPdu = ocp1CommandMessage.GetCommandPdu(ocp1Command, ocp1Header, 1u);
		commandbll.SendCommand(tcpclient, ocp1CommandPdu);
	}

	private uint Operate_File(OcaBlock ocablock)
	{
		uint result = 0u;
		switch (ocablock.Label)
		{
		case "select":
			result = 161u;
			break;
		case "read":
			result = 162u;
			break;
		case "write":
			result = 163u;
			break;
		}
		return result;
	}
}

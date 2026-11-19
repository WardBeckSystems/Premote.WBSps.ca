using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Model.AES70;
using OCAAES70.OCC.ControlClasses.Workers.BlocksAndMatrices;
using OCAAES70.OCP1;

namespace PreMoBLL;

public class DSPBLL
{
	private CommandBLL commandbll = new CommandBLL();

	public void CommandMessage(TcpClient tcpclient, OcaDSPInfo dspinfo, OcaBlock ocablock)
	{
		uint num = 0u;
		uint ono = 0u;
		Ocp1MessageHeader ocp1MessageHeader = new Ocp1MessageHeader();
		Ocp1MessageCommand ocp1MessageCommand = new Ocp1MessageCommand();
		Ocp1CommandMessage ocp1CommandMessage = new Ocp1CommandMessage();
		Ocp1Header ocp1Header = new Ocp1Header();
		Ocp1Command ocp1Command = new Ocp1Command();
		Ocp1CommandPdu ocp1CommandPdu = new Ocp1CommandPdu();
		num = ocablock.ONo;
		switch (num)
		{
		case 16u:
			ono = Channel1_DSP(ocablock);
			break;
		case 32u:
			ono = Download1_DSP(ocablock);
			break;
		case 48u:
			ono = Channel2_DSP(ocablock);
			break;
		case 64u:
			ono = Download2_DSP(ocablock);
			break;
		case 112u:
			ono = stream(ocablock);
			break;
		}
		byte[] param = new byte[1] { dspinfo.dspvalue };
		ocp1Command = ocp1MessageCommand.GetCommand(num, ono, param);
		byte pdusize = Convert.ToByte(5 + Marshal.SizeOf(ocp1Command));
		ocp1Header = ocp1MessageHeader.MarshalHeader(1, pdusize, OcaMessageType.OcaCmd, 1);
		ocp1CommandPdu = ocp1CommandMessage.GetCommandPdu(ocp1Command, ocp1Header, 1u);
		commandbll.SendCommand(tcpclient, ocp1CommandPdu);
	}

	private uint Channel1_DSP(OcaBlock ocablock)
	{
		uint result = 0u;
		switch (ocablock.Label)
		{
		case "hpf":
			result = 23u;
			break;
		case "treble":
			result = 24u;
			break;
		case "bass":
			result = 25u;
			break;
		}
		return result;
	}

	private uint Channel2_DSP(OcaBlock ocablock)
	{
		uint result = 0u;
		switch (ocablock.Label)
		{
		case "hpf":
			result = 55u;
			break;
		case "treble":
			result = 56u;
			break;
		case "bass":
			result = 57u;
			break;
		}
		return result;
	}

	private uint Download1_DSP(OcaBlock ocablock)
	{
		uint result = 0u;
		switch (ocablock.Label)
		{
		case "hpf":
			result = 36u;
			break;
		case "treble":
			result = 37u;
			break;
		case "bass":
			result = 38u;
			break;
		}
		return result;
	}

	private uint Download2_DSP(OcaBlock ocablock)
	{
		uint result = 0u;
		switch (ocablock.Label)
		{
		case "hpf":
			result = 68u;
			break;
		case "treble":
			result = 69u;
			break;
		case "bass":
			result = 70u;
			break;
		}
		return result;
	}

	private uint stream(OcaBlock ocablock)
	{
		uint result = 0u;
		string label = ocablock.Label;
		if (!(label == "headsetrout"))
		{
			if (label == "uploadrout")
			{
				result = 114u;
			}
		}
		else
		{
			result = 113u;
		}
		return result;
	}
}

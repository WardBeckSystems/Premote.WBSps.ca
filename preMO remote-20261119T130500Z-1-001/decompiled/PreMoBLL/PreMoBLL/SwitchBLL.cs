using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Model.AES70;
using OCAAES70.OCC.ControlClasses.Workers.BlocksAndMatrices;
using OCAAES70.OCP1;

namespace PreMoBLL;

public class SwitchBLL
{
	private CommandBLL commandbll = new CommandBLL();

	public void CommandMessage(TcpClient tcpclient, OcaSwitchInfo switchinfo, OcaBlock ocablock)
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
			ono = Channel1_Switch(ocablock);
			break;
		case 32u:
			ono = Download1_Switch(ocablock);
			break;
		case 48u:
			ono = Channel2_Switch(ocablock);
			break;
		case 64u:
			ono = Download2_Switch(ocablock);
			break;
		case 80u:
			ono = VolumeFront_Switch(ocablock);
			break;
		case 112u:
			ono = Stream_Switch(ocablock);
			break;
		case 96u:
			ono = VolumeRear_Switch(ocablock);
			break;
		case 144u:
			ono = System_Switch(ocablock);
			break;
		}
		byte[] param = new byte[1] { switchinfo.SwitchStatus };
		ocp1Command = ocp1MessageCommand.GetCommand(num, ono, param);
		byte pdusize = Convert.ToByte(5 + Marshal.SizeOf(ocp1Command));
		ocp1Header = ocp1MessageHeader.MarshalHeader(1, pdusize, OcaMessageType.OcaCmd, 1);
		ocp1CommandPdu = ocp1CommandMessage.GetCommandPdu(ocp1Command, ocp1Header, 1u);
		commandbll.SendCommand(tcpclient, ocp1CommandPdu);
	}

	private uint Channel1_Switch(OcaBlock ocablock)
	{
		uint result = 0u;
		switch (ocablock.Label)
		{
		case "source":
			result = 19u;
			break;
		case "phantom":
			result = 17u;
			break;
		case "phase":
			result = 18u;
			break;
		case "mute":
			result = 20u;
			break;
		case "pad":
			result = 22u;
			break;
		}
		return result;
	}

	private uint Channel2_Switch(OcaBlock ocablock)
	{
		uint result = 0u;
		switch (ocablock.Label)
		{
		case "source":
			result = 51u;
			break;
		case "phantom":
			result = 49u;
			break;
		case "phase":
			result = 50u;
			break;
		case "mute":
			result = 52u;
			break;
		case "pad":
			result = 54u;
			break;
		}
		return result;
	}

	private uint Download1_Switch(OcaBlock ocablock)
	{
		uint result = 0u;
		string label = ocablock.Label;
		if (!(label == "phase"))
		{
			if (label == "mute")
			{
				result = 34u;
			}
		}
		else
		{
			result = 33u;
		}
		return result;
	}

	private uint Download2_Switch(OcaBlock ocablock)
	{
		uint result = 0u;
		string label = ocablock.Label;
		if (!(label == "phase"))
		{
			if (label == "mute")
			{
				result = 66u;
			}
		}
		else
		{
			result = 65u;
		}
		return result;
	}

	private uint VolumeFront_Switch(OcaBlock ocablock)
	{
		uint result = 0u;
		string label = ocablock.Label;
		if (!(label == "mute"))
		{
			if (label == "push")
			{
				result = 83u;
			}
		}
		else
		{
			result = 81u;
		}
		return result;
	}

	private uint VolumeRear_Switch(OcaBlock ocablock)
	{
		uint result = 0u;
		string label = ocablock.Label;
		if (!(label == "mute"))
		{
			if (label == "push")
			{
				result = 99u;
			}
		}
		else
		{
			result = 97u;
		}
		return result;
	}

	private uint System_Switch(OcaBlock ocablock)
	{
		uint result = 0u;
		switch (ocablock.Label)
		{
		case "mute":
			result = 147u;
			break;
		case "display":
			result = 146u;
			break;
		case "ppm":
			result = 148u;
			break;
		}
		return result;
	}

	private uint Stream_Switch(OcaBlock ocablock)
	{
		uint result = 0u;
		string label = ocablock.Label;
		if (!(label == "hmute"))
		{
			if (label == "umute")
			{
				result = 117u;
			}
		}
		else
		{
			result = 116u;
		}
		return result;
	}
}

using System.Runtime.InteropServices;
using Model.AES70;

namespace OCAAES70.OCP1;

public class Ocp1MessageCommand
{
	public Ocp1Command GetCommand(uint handle, uint Ono, byte[] param)
	{
		Ocp1Command ocp1Command = new Ocp1Command();
		ocp1Command.handle = handle;
		ocp1Command.targetONo = Ono;
		ocp1Command.parameters = GetParameters(param);
		ocp1Command.methodID = GetMethodID();
		ocp1Command.commandSize = (uint)Marshal.SizeOf(ocp1Command);
		return ocp1Command;
	}

	private static OcaMethodID GetMethodID()
	{
		OcaMethodID ocaMethodID = new OcaMethodID();
		ocaMethodID.treeLevel = 4;
		ocaMethodID.methodIndex = 1;
		return ocaMethodID;
	}

	private static Ocp1Parameters GetParameters(byte[] param)
	{
		Ocp1Parameters ocp1Parameters = new Ocp1Parameters();
		ocp1Parameters.parameterCount = (byte)param.Length;
		for (int i = 0; i < ocp1Parameters.parameterCount; i++)
		{
			ocp1Parameters.parameters = param;
		}
		return ocp1Parameters;
	}
}

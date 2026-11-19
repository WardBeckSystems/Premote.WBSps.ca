using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Model.AES70;
using PreMoDAL;

namespace PreMoBLL;

public class CommandBLL
{
	private CommandDAL command = new CommandDAL();

	public byte[] buff;

	public void SendCommand(TcpClient tcpclient, Ocp1CommandPdu commandpdu)
	{
		int num = Marshal.SizeOf(commandpdu);
		buff = new byte[num];
		IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buff, 0);
		Marshal.StructureToPtr(commandpdu, ptr, fDeleteOld: true);
		ByteMarshal(buff, num);
		command.SendCommandAsync(tcpclient, buff);
	}

	public static void ByteMarshal(byte[] buff, int size)
	{
		byte b = buff[1];
		buff[1] = buff[2];
		buff[2] = b;
		ReverseBytes(buff, 3, 4);
		b = buff[8];
		buff[8] = buff[9];
		buff[9] = b;
		ReverseBytes(buff, 10, 4);
		ReverseBytes(buff, 14, 4);
		ReverseBytes(buff, 18, 4);
		ReverseBytes(buff, 22, 2);
		ReverseBytes(buff, 24, 2);
	}

	public static void ReverseBytes(byte[] bytes, int start, int len)
	{
		int num = start + len - 1;
		int num2 = 0;
		int num3 = start;
		while (num3 < start + len / 2)
		{
			byte b = bytes[num - num2];
			bytes[num - num2] = bytes[num3];
			bytes[num3] = b;
			num3++;
			num2++;
		}
	}
}

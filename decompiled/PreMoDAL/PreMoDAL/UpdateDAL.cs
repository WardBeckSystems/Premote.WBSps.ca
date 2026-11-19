using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Model;

namespace PreMoDAL;

public class UpdateDAL
{
	public async void SendBoot(TcpClient tcpclient, string str)
	{
		NetworkStream clientstream = tcpclient.GetStream();
		byte[] bootBuf = Encoding.ASCII.GetBytes(str);
		try
		{
			await clientstream.WriteAsync(bootBuf, 0, bootBuf.Length);
			await clientstream.FlushAsync();
		}
		catch
		{
		}
	}

	public async Task<string> WaitNotice(TcpClient tcpclient)
	{
		NetworkStream clientstream = tcpclient.GetStream();
		while (true)
		{
			try
			{
				byte[] buf = new byte[8];
				await clientstream.ReadAsync(buf, 0, buf.Length);
				string str = Encoding.ASCII.GetString(buf, 0, buf.Length);
				if (str == "DOWNLOAD")
				{
					return str;
				}
			}
			catch
			{
			}
		}
	}

	public async void SendData(FileInfo fileinfo, TcpClient tcpclient)
	{
		NetworkStream clientstream = tcpclient.GetStream();
		int packcount = 0;
		byte[] recBuff = new byte[2];
		while (true)
		{
			try
			{
				if (packcount <= fileinfo.PackNum - 1)
				{
					await clientstream.WriteAsync(fileinfo.buf, 0, 201);
					await clientstream.FlushAsync();
				}
				else
				{
					await clientstream.WriteAsync(fileinfo.buf, 0, fileinfo.ModLength + 1);
					await clientstream.FlushAsync();
					packcount++;
					fileinfo.buf[0] = Convert.ToByte(0);
					for (int i = 0; i < 200; i++)
					{
						fileinfo.buf[i + 1] = fileinfo.buffer[i];
					}
				}
				Thread.Sleep(120);
				await clientstream.ReadAsync(recBuff, 0, 2);
				if (packcount <= fileinfo.PackNum - 2)
				{
					if (Convert.ToInt32(recBuff[1]) == 201 && Convert.ToInt32(recBuff[0]) + packcount / 200 * 200 == packcount)
					{
						packcount++;
						fileinfo.buf[0] = Convert.ToByte(packcount % 200);
						for (int j = 0; j < 200; j++)
						{
							fileinfo.buf[j + 1] = fileinfo.buffer[j + 200 * packcount];
						}
					}
				}
				else if (packcount == fileinfo.PackNum - 1)
				{
					packcount++;
					fileinfo.buf[0] = Convert.ToByte(packcount % 200);
					for (int k = 0; k < fileinfo.ModLength; k++)
					{
						fileinfo.buf[k + 1] = fileinfo.buffer[k + 200 * fileinfo.PackNum];
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}

using System.Net.Sockets;

namespace PreMoDAL;

public class CommandDAL
{
	private NetworkStream clientstream;

	public async void SendCommandAsync(TcpClient tcpclient, byte[] buffer)
	{
		clientstream = tcpclient.GetStream();
		try
		{
			await clientstream.WriteAsync(buffer, 0, buffer.Length);
			await clientstream.FlushAsync();
		}
		catch
		{
		}
	}
}

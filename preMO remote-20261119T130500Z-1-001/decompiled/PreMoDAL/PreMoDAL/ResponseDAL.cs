using System.Net.Sockets;
using System.Threading.Tasks;

namespace PreMoDAL;

public class ResponseDAL
{
	private NetworkStream clientstream;

	public async Task<byte[]> WaitResponse(TcpClient tcpclient)
	{
		clientstream = tcpclient.GetStream();
		byte[] buf = new byte[200];
		await clientstream.ReadAsync(buf, 0, buf.Length);
		return buf;
	}
}

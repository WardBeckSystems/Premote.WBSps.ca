using System.Net.Sockets;
using System.Threading.Tasks;

namespace PreMoDAL;

public class NoticeDAL
{
	private NetworkStream clientstream;

	public async Task<byte[]> WaitNotice(TcpClient tcpclient)
	{
		try
		{
			clientstream = tcpclient.GetStream();
			byte[] buf = new byte[200];
			await clientstream.ReadAsync(buf, 0, buf.Length);
			return buf;
		}
		catch
		{
		}
		return null;
	}
}

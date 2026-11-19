using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace PreMoDAL;

public class TCPHelp
{
	private NetworkStream nwStream;

	public async Task<string> GetConnectionAsync(IPInfo ipinfo, TcpClient tcpclient)
	{
		byte[] buf = new byte[5];
		try
		{
			await tcpclient.ConnectAsync(IPAddress.Parse(ipinfo.IPAddress), ipinfo.intPort);
			nwStream = tcpclient.GetStream();
			await nwStream.ReadAsync(buf, 0, 5);
			return Encoding.ASCII.GetString(buf, 0, 5);
		}
		catch
		{
			tcpclient.Close();
			return "disconnect";
		}
	}
}

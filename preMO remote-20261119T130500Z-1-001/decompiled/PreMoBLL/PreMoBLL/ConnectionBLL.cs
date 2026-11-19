using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Model;
using PreMoDAL;

namespace PreMoBLL;

public class ConnectionBLL
{
	public async Task<ConnectionState> Connection(IPInfo ipinfo, TcpClient tcpclient)
	{
		if (ipinfo.IPAddress == "")
		{
			return ConnectionState.IPEmpty;
		}
		if (!IPCheck(ipinfo.IPAddress))
		{
			return ConnectionState.IllegalIP;
		}
		string str = await new TCPHelp().GetConnectionAsync(ipinfo, tcpclient);
		if (str == "START")
		{
			return ConnectionState.Connected;
		}
		if (str == "disconnect")
		{
			return ConnectionState.Disconnect;
		}
		return ConnectionState.Disconnect;
	}

	public static bool IPCheck(string IP)
	{
		return Regex.IsMatch(IP, "^((2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.){3}(2[0-4]\\d|25[0-5]|[01]?\\d\\d?)$");
	}
}

using System.Collections.Generic;
using System.Net.Sockets;

namespace Model;

public class IPInfo
{
	public static Dictionary<string, TcpClient> dictIP = new Dictionary<string, TcpClient>();

	public int intPort { get; set; }

	public string IPAddress { get; set; }
}

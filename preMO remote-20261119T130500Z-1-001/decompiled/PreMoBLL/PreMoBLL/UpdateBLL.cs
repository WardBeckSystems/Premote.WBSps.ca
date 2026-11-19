using System.Net.Sockets;
using System.Threading.Tasks;
using Model;
using PreMoDAL;

namespace PreMoBLL;

public class UpdateBLL
{
	private UpdateDAL UD = new UpdateDAL();

	public void Reboot(TcpClient tcpclient, string str)
	{
		UD.SendBoot(tcpclient, str);
	}

	public async Task<string> WaitBoot(TcpClient tcpclient)
	{
		return await Task.Run(() => UD.WaitNotice(tcpclient));
	}

	public void UpdateARM(string str, TcpClient tcpclient, FileInfo fileinfo)
	{
		if (str == "DOWNLOAD")
		{
			UD.SendData(fileinfo, tcpclient);
		}
	}
}

using System.Net.Sockets;
using System.Threading.Tasks;
using Model.AES70;
using PreMoDAL;

namespace PreMoBLL;

public class NoticeBLL
{
	private NoticeDAL noticedal = new NoticeDAL();

	private byte[] buf;

	private OcaUnmarshalByte OcaStruct = new OcaUnmarshalByte();

	private Ocp1NotificationPdu ocp1pdu;

	private OcaPropertyParameter ocaparameter;

	public async Task<byte[]> WaitNoticeBLL(TcpClient tcpclient)
	{
		buf = new byte[200];
		NoticeBLL noticeBLL = this;
		_ = noticeBLL.buf;
		noticeBLL.buf = await Task.Run(() => noticedal.WaitNotice(tcpclient));
		return buf;
	}
}

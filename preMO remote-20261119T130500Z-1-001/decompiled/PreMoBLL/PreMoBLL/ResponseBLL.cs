using System;
using Model.AES70;

namespace PreMoBLL;

public class ResponseBLL
{
	private Ocp1ResponsePdu ocp1pdu;

	private OcaFileInfo fileparameter;

	private byte[] buf;

	public OcaFileInfo WaitResponseBLL(byte[] buf)
	{
		fileparameter = new OcaFileInfo();
		fileparameter.fileParam = new byte[76];
		ocp1pdu = new Ocp1ResponsePdu();
		if (Convert.ToInt32(buf[7]) == 3)
		{
			fileparameter.fileNum = buf[20];
			Array.Copy(buf, 21, fileparameter.fileParam, 0, 76);
			Array.Clear(buf, 0, 200);
			return fileparameter;
		}
		return null;
	}
}

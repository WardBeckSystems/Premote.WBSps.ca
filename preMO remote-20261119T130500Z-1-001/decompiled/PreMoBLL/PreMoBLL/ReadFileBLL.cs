using System;
using System.IO;
using Model;

namespace PreMoBLL;

public class ReadFileBLL
{
	private Model.FileInfo fileinfo = new Model.FileInfo();

	public void SaveFile(Model.FileInfo fileinfo)
	{
		try
		{
			using FileStream fileStream = new FileStream(fileinfo.path, FileMode.OpenOrCreate, FileAccess.Read);
			int num = (fileinfo.FileLength = fileStream.Read(fileinfo.buffer, 0, 512000));
			fileinfo.PackNum = num / 200;
			fileinfo.ModLength = num % 200;
			fileinfo.buf[0] = Convert.ToByte(0);
			for (int i = 0; i < 200; i++)
			{
				fileinfo.buf[i + 1] = fileinfo.buffer[i];
			}
		}
		catch
		{
		}
	}
}

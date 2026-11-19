namespace Model;

public class FileInfo
{
	public string path;

	public byte[] buffer = new byte[1024000];

	public byte[] buf = new byte[201];

	private int _packnum;

	private int _modlength;

	private int _filelength;

	public int PackNum
	{
		get
		{
			return _packnum;
		}
		set
		{
			_packnum = value;
		}
	}

	public int ModLength
	{
		get
		{
			return _modlength;
		}
		set
		{
			_modlength = value;
		}
	}

	public int FileLength
	{
		get
		{
			return _filelength;
		}
		set
		{
			_filelength = value;
		}
	}
}

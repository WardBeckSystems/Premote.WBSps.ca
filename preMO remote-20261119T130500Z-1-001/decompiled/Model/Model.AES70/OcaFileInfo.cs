namespace Model.AES70;

public class OcaFileInfo
{
	public byte[] fileParam;

	public string ClassId { get; }

	public ushort ClassVersion { get; set; }

	public byte fileNum { get; set; }
}

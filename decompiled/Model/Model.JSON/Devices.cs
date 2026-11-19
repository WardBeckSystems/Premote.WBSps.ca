using System.Collections.Generic;

namespace Model.JSON;

public class Devices
{
	public static Dictionary<string, string> dictIP = new Dictionary<string, string>();

	public string hostname { get; set; }

	public string IPaddress { get; set; }
}
